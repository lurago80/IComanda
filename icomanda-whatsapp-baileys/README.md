# iComanda WhatsApp (Baileys – sem Docker)

Serviço Node.js que usa **Baileys** para envio direto de WhatsApp. Não usa Docker nem Evolution API.

## Se ficar em loop ("connection errored" / "not logged in")

Apague a sessão antiga e suba de novo:

**PowerShell (na pasta icomanda-whatsapp-baileys):**
```powershell
.\limpar-sessao.ps1
npm run dev
```

Ou apague manualmente a pasta `auth_info_baileys` e depois rode `npm run dev`.

## Como usar

1. **Instalar e subir o serviço** (no terminal, na pasta do projeto):

   ```bash
   cd icomanda-whatsapp-baileys
   npm install
   npm run dev
   ```

2. O serviço sobe em **http://localhost:3001**.

3. No **iComanda**, abra o modal "Conectar WhatsApp". O QR Code aparece na tela (ou no terminal, se preferir).

4. Escaneie o QR **uma vez** com o WhatsApp do celular (Configurações → Aparelhos conectados → Conectar aparelho).

5. Depois de conectado, ao clicar em "Enviar via WhatsApp" no iComanda, a mensagem é enviada **direto**, sem abrir nova página.

## Endpoints

- `GET /qrcode` – retorna o QR em base64 (para o modal do iComanda).
- `GET /status` – retorna `{ conectado: true/false }`.
- `POST /send` – envia mensagem; body: `{ "telefone": "5511999999999", "mensagem": "Texto" }`.

## Configuração no iComanda

No `appsettings.json` da API do iComanda:

```json
"WhatsApp": {
  "Metodo": "baileys",
  "BaileysService": {
    "BaseUrl": "http://localhost:3001"
  }
}
```

A sessão fica salva na pasta `auth_info_baileys`. Na próxima vez que rodar `npm run dev`, o WhatsApp conecta sozinho, sem precisar escanear o QR de novo (a menos que você desvincule o aparelho no celular).
