import { MessageCircle, Loader2, CheckCircle, RefreshCw, AlertTriangle } from 'lucide-react';
import React, { useEffect, useState, useRef } from 'react';
import { whatsAppService } from '../services/api';
import { Button } from './ui/button';

interface ConectarWhatsAppModalProps {
  isOpen: boolean;
  onClose: () => void;
}

/**
 * Modal para conectar WhatsApp (Evolution API / Baileys).
 * Mostra o QR Code na própria tela; escaneia uma vez e depois as mensagens são enviadas direto, sem abrir nova página.
 */
const ConectarWhatsAppModal: React.FC<ConectarWhatsAppModalProps> = ({ isOpen, onClose }) => {
  const [qrBase64, setQrBase64] = useState<string | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const [conectado, setConectado] = useState(false);
  const [loading, setLoading] = useState(true);
  const [resetting, setResetting] = useState(false);
  const [resetMsg, setResetMsg] = useState<string | null>(null);
  const autoCloseRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const carregarQrRef = useRef<() => Promise<void>>(async () => {});

  useEffect(() => {
    if (!isOpen) return;

    let intervalConectado: ReturnType<typeof setInterval> | null = null;
    let intervalQr: ReturnType<typeof setInterval> | null = null;

    const carregarQr = async () => {
      try {
        const res = await whatsAppService.getQrCode();
        if (res.conectado) {
          setConectado(true);
          setQrBase64(null);
          setErro(null);
          return;
        }
        if (res.base64) {
          setQrBase64(res.base64);
          setErro(null);
        } else if (res.erro) {
          setErro(res.erro);
          setQrBase64(null);
        }
      } catch (e) {
        setErro('Não foi possível obter o QR Code. Verifique se a API e o serviço Baileys estão rodando.');
        setQrBase64(null);
      } finally {
        setLoading(false);
      }
    };

    carregarQrRef.current = carregarQr;

    const verificarConectado = async () => {      try {
        const res = await whatsAppService.getConectado();
        if (res.conectado) {
          setConectado(true);
          setQrBase64(null);
          setErro(null);
        }
      } catch {
        // ignora
      }
    };

    setConectado(false);
    setLoading(true);
    setQrBase64(null);
    setErro(null);
    carregarQr();

    intervalConectado = setInterval(verificarConectado, 4000);
    // Atualizar QR a cada 3s quando ainda não temos QR (serviço pode demorar 15–30s para gerar)
    intervalQr = setInterval(carregarQr, 3000);

    return () => {
      if (intervalConectado) clearInterval(intervalConectado);
      if (intervalQr) clearInterval(intervalQr);
    };
  }, [isOpen]);

  const handleReset = async () => {
    if (resetting) return;
    setResetting(true);
    setResetMsg(null);
    setQrBase64(null);
    setErro(null);
    setConectado(false);
    try {
      const res = await whatsAppService.resetarSessao();
      setResetMsg(res.mensagem || 'Sessão resetada! Aguarde o novo QR...');
      // Recarregar QR após 3s (tempo para o Baileys subir novamente)
      setTimeout(() => {
        setResetMsg(null);
        setLoading(true);
        carregarQrRef.current?.();
      }, 3000);
    } catch {
      setResetMsg('Erro ao resetar. Verifique se o serviço Baileys está rodando.');
    } finally {
      setResetting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[200] flex items-center justify-center bg-black/50 p-4">
      <div
        className="bg-card border border-border rounded-2xl shadow-xl max-w-md w-full p-6 space-y-4"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-full bg-amber-100 flex items-center justify-center">
            <MessageCircle className="w-6 h-6 text-amber-600" />
          </div>
          <div>
            <h3 className="text-lg font-semibold text-text-primary">Conectar WhatsApp (envio direto)</h3>
            <p className="text-sm text-text-muted">Escaneie o QR Code uma vez. Depois disso, ao clicar em &quot;Enviar via WhatsApp&quot; a mensagem será enviada direto, sem abrir nova página.</p>
          </div>
        </div>

        {conectado ? (
          <>
            <div className="bg-green-50 border border-green-200 rounded-lg p-4 flex items-center gap-3">
              <CheckCircle className="w-10 h-10 text-green-600 flex-shrink-0" />
              <div>
                <p className="font-semibold text-green-800">WhatsApp conectado!</p>
                <p className="text-sm text-green-700">As mensagens serão enviadas direto. Esta janela fecha em 2 segundos.</p>
              </div>
            </div>
            <Button onClick={onClose} className="w-full">Fechar agora</Button>
          </>
        ) : erro && !qrBase64 ? (
          <>
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 text-sm text-amber-800">
              {erro}
            </div>
            <p className="text-sm text-muted-foreground">
              O QR pode levar 15–30 segundos para aparecer após <strong>npm run dev</strong>. O modal atualiza sozinho a cada 3 segundos.
            </p>
            <p className="text-sm font-medium text-text-secondary">Passos (Baileys, sem Docker):</p>
            <ol className="text-sm text-text-secondary space-y-2 list-decimal list-inside">
              <li>No terminal: <strong>cd icomanda-whatsapp-baileys</strong> e depois <strong>npm install</strong> e <strong>npm run dev</strong>.</li>
              <li>Aguarde o serviço subir (até aparecer &quot;QR Code gerado&quot; no terminal). O QR aparecerá aqui em breve.</li>
              <li>Escaneie com o WhatsApp do celular uma vez. Depois as mensagens saem direto.</li>
            </ol>
            {resetMsg && (
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-sm text-blue-800 flex items-center gap-2">
                <RefreshCw className="w-4 h-4 flex-shrink-0 animate-spin" />
                {resetMsg}
              </div>
            )}
            <div className="flex gap-2 pt-2">
              <Button variant="outline" onClick={onClose} className="flex-1">Fechar</Button>
              <Button variant="outline" onClick={() => { setLoading(true); setErro(null); carregarQrRef.current?.(); }} className="flex-1">
                Atualizar
              </Button>
              <Button
                variant="outline"
                onClick={handleReset}
                disabled={resetting}
                className="flex-1 border-red-300 text-red-600 hover:bg-red-50"
                title="Limpa a sessão salva e gera um novo QR Code"
              >
                {resetting ? <Loader2 className="w-4 h-4 animate-spin mr-1" /> : <AlertTriangle className="w-4 h-4 mr-1" />}
                Reconectar
              </Button>
            </div>
          </>
        ) : (
          <>
            {loading ? (
              <div className="flex flex-col items-center justify-center py-8 gap-3">
                <Loader2 className="w-10 h-10 text-primary animate-spin" />
                <p className="text-sm text-text-muted">
                  {resetMsg ? resetMsg : 'Carregando QR Code...'}
                </p>
              </div>
            ) : qrBase64 ? (
              <div className="flex flex-col items-center gap-3">
                <p className="text-sm text-text-secondary">Escaneie com o WhatsApp do celular (Configurações → Aparelhos conectados → Conectar aparelho)</p>
                <img
                  src={qrBase64.startsWith('data:') ? qrBase64 : `data:image/png;base64,${qrBase64}`}
                  alt="QR Code WhatsApp"
                  className="w-64 h-64 border border-border rounded-lg bg-white"
                />
                <p className="text-xs text-text-muted">O QR Code é atualizado automaticamente. Quando conectar, esta tela será atualizada.</p>
              </div>
            ) : null}
            {resetMsg && (
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-3 text-sm text-blue-800 flex items-center gap-2">
                <RefreshCw className="w-4 h-4 flex-shrink-0 animate-spin" />
                {resetMsg}
              </div>
            )}
            <div className="flex gap-2 pt-2">
              <Button variant="outline" onClick={onClose} className="flex-1">Fechar</Button>
              <Button
                variant="outline"
                onClick={handleReset}
                disabled={resetting}
                className="flex-1 border-red-300 text-red-600 hover:bg-red-50"
                title="Limpa a sessão salva e gera um novo QR Code para escanear"
              >
                {resetting ? <Loader2 className="w-4 h-4 animate-spin mr-1" /> : <AlertTriangle className="w-4 h-4 mr-1" />}
                Reconectar
              </Button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default ConectarWhatsAppModal;
