/**
 * Serviço WhatsApp com Baileys para iComanda (sem Docker).
 * Rode: npm install && npm run dev
 * Escaneie o QR uma vez; depois as mensagens saem direto.
 */
import express from 'express';
import makeWASocket, { useMultiFileAuthState, DisconnectReason, fetchLatestWaWebVersion } from '@whiskeysockets/baileys';
import QRCode from 'qrcode';
import { Boom } from '@hapi/boom';
import pino from 'pino';
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const authDir = path.join(__dirname, 'auth_info_baileys');

const app = express();
app.use(express.json());

const PORT = process.env.PORT || 3001;

let sock = null;
let currentQR = null;   // base64 PNG do QR
let connected = false;
let reconnecting = false;

/** Versão atual do WhatsApp Web (evita "Connection Failure" por versão desatualizada) */
let waVersion = null;

async function connectToWhatsApp() {
  reconnecting = false;

  // Usar versão mais recente do WhatsApp Web para evitar Connection Failure
  if (!waVersion) {
    try {
      const { version } = await fetchLatestWaWebVersion({});
      waVersion = version;
      console.log('Versão WhatsApp Web:', waVersion?.join('.') || 'padrão');
    } catch (e) {
      console.warn('Não foi possível obter versão do WhatsApp Web, usando padrão:', e.message);
    }
  }

  const { state, saveCreds } = await useMultiFileAuthState(authDir);
  const socketConfig = {
    auth: state,
    browser: ['iComanda', 'Chrome', '1.0.0'],
    logger: pino({ level: 'silent' }),
    connectTimeoutMs: 60000,
  };
  if (waVersion) socketConfig.version = waVersion;

  const socket = makeWASocket(socketConfig);

  socket.ev.on('connection.update', async (update) => {
    const { connection, lastDisconnect, qr } = update;

    if (qr) {
      try {
        currentQR = await QRCode.toDataURL(qr, { type: 'image/png', margin: 1 });
        console.log('QR Code gerado! Abra o modal "Conectar WhatsApp" no iComanda para escanear.');
      } catch (e) {
        console.error('Erro ao gerar QR em base64:', e);
        currentQR = null;
      }
    }

    if (connection === 'open') {
      connected = true;
      currentQR = null;
      console.log('WhatsApp conectado! Envio direto ativo.');
    }

    if (connection === 'close') {
      connected = false;
      sock = null;
      const statusCode = (lastDisconnect?.error instanceof Boom)
        ? lastDisconnect.error.output?.statusCode
        : null;
      const shouldReconnect = statusCode !== DisconnectReason.loggedOut;
      console.log('Conexão fechada. Reconnect:', shouldReconnect);
      if (statusCode === DisconnectReason.loggedOut) {
        console.log('');
        console.log('>>> Sessão anterior encerrada. Para ver o QR de novo:');
        console.log('    1. Pare o serviço (Ctrl+C)');
        console.log('    2. Apague a pasta auth_info_baileys nesta pasta');
        console.log('    3. Rode de novo: npm run dev');
        console.log('');
      }
      if (shouldReconnect && !reconnecting) {
        reconnecting = true;
        console.log('Reconectando em 10 segundos... (se ficar em loop, apague a pasta auth_info_baileys e rode de novo)');
        setTimeout(connectToWhatsApp, 10000);
      }
    }
  });

  socket.ev.on('creds.update', saveCreds);
  sock = socket;
  return socket;
}

// GET / — página simples para quem abrir no navegador (o app é em http://localhost:3000)
app.get('/', (req, res) => {
  const status = connected ? 'Conectado' : 'Aguardando QR';
  res.type('html').send(`
<!DOCTYPE html>
<html>
<head><meta charset="utf-8"><title>WhatsApp Baileys - iComanda</title></head>
<body style="font-family:sans-serif;max-width:480px;margin:60px auto;padding:20px;text-align:center;">
  <h1>WhatsApp Baileys</h1>
  <p>Status: <strong>${status}</strong></p>
  <p>Este é o serviço de envio direto. O aplicativo iComanda abre em:</p>
  <p><a href="http://localhost:3000" style="font-size:1.2em;">http://localhost:3000</a></p>
  <p><a href="http://localhost:3000" style="display:inline-block;margin-top:12px;padding:10px 20px;background:#25D366;color:#fff;text-decoration:none;border-radius:8px;">Abrir iComanda</a></p>
</body>
</html>`);
});

// GET /qrcode — retorna QR em base64 (ou data URL) para exibir no modal
app.get('/qrcode', (req, res) => {
  if (connected) {
    return res.json({ base64: null, conectado: true });
  }
  if (currentQR) {
    const base64 = currentQR.startsWith('data:') ? currentQR.split(',')[1] : currentQR;
    return res.json({ base64, conectado: false });
  }
  res.json({ base64: null, conectado: false, erro: 'Aguardando QR. O serviço está iniciando.' });
});

// GET /status — verifica se está conectado
app.get('/status', (req, res) => {
  res.json({ conectado: connected });
});

// POST /send — envia mensagem (telefone e mensagem no body)
app.post('/send', async (req, res) => {
  if (!connected || !sock) {
    return res.status(503).json({
      sucesso: false,
      mensagem: 'WhatsApp não está conectado. Rode o serviço e escaneie o QR.',
    });
  }
  let { telefone, mensagem } = req.body;
  if (!telefone || !mensagem) {
    return res.status(400).json({ sucesso: false, mensagem: 'telefone e mensagem são obrigatórios' });
  }
  telefone = String(telefone).replace(/\D/g, '');
  if (telefone.length < 10) {
    return res.status(400).json({ sucesso: false, mensagem: 'Telefone inválido' });
  }
  if (!telefone.startsWith('55') && telefone.length === 11) {
    telefone = '55' + telefone;
  }
  const jid = telefone + '@s.whatsapp.net';
  try {
    await sock.sendMessage(jid, { text: mensagem });
    res.json({ sucesso: true, mensagem: 'Mensagem enviada.' });
  } catch (e) {
    console.error('Erro ao enviar:', e);
    res.status(500).json({ sucesso: false, mensagem: e.message || 'Erro ao enviar' });
  }
});

// POST /reset — apaga a sessão e reconecta (gera novo QR)
app.post('/reset', async (req, res) => {
  try {
    console.log('🔄 Reset de sessão solicitado...');

    // Fechar socket atual, se houver
    if (sock) {
      try {
        sock.ev.removeAllListeners();
        await sock.logout().catch(() => {});
        sock.ws?.close();
      } catch (_) { /* ignora */ }
      sock = null;
    }
    connected = false;
    currentQR = null;

    // Apagar pasta auth_info_baileys
    if (fs.existsSync(authDir)) {
      fs.rmSync(authDir, { recursive: true, force: true });
      console.log('🗑️  Pasta auth_info_baileys apagada.');
    }

    // Aguardar um instante e reconectar
    setTimeout(() => {
      connectToWhatsApp().catch(e => console.error('Erro ao reconectar:', e));
    }, 1000);

    res.json({ sucesso: true, mensagem: 'Sessão resetada. Aguarde o QR Code aparecer.' });
  } catch (e) {
    console.error('Erro ao resetar sessão:', e);
    res.status(500).json({ sucesso: false, mensagem: e.message || 'Erro ao resetar sessão' });
  }
});

app.listen(PORT, async () => {
  console.log(`iComanda WhatsApp Baileys rodando em http://localhost:${PORT}`);
  console.log('Endpoints: GET /qrcode, GET /status, POST /send');
  await connectToWhatsApp();
});
