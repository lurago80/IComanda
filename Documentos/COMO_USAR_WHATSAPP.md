# 📱 Como Usar o WhatsApp Web Automático

> **Envio direto (sem abrir navegador a cada mensagem):**  
> Use **Baileys (sem Docker)**. Rode o serviço na pasta `icomanda-whatsapp-baileys` (npm run dev), escaneie o QR uma vez no modal "Conectar WhatsApp" do iComanda e as mensagens saem direto.

---

## ⚠️ IMPORTANTE: O sistema NÃO abre nova janela automaticamente

O sistema foi configurado para **NÃO abrir nova janela do Chrome**. Ele apenas conecta ao Chrome que você já tem aberto.

## 🚀 Configuração Inicial (Uma vez apenas)

### Passo 1: Iniciar o Chrome com Remote Debugging

**Opção A: Usar o script automático (Recomendado)**
1. Execute o arquivo: `iniciar-chrome-whatsapp.bat`
2. O Chrome será iniciado automaticamente com remote debugging

**Opção B: Manual**
1. Feche **todas** as janelas do Chrome
2. Abra o PowerShell ou CMD como Administrador
3. Execute:
   ```cmd
   "C:\Program Files\Google\Chrome\Application\chrome.exe" --remote-debugging-port=9222
   ```
   (Ajuste o caminho se o Chrome estiver em outro local)

### Passo 2: Abrir WhatsApp Web

1. No Chrome que abriu, vá para: `https://web.whatsapp.com`
2. Escaneie o QR Code com seu celular
3. Faça o login
4. **Mantenha o WhatsApp Web aberto**

### Passo 3: Usar o Sistema

Agora você pode usar o sistema normalmente:
- Clique no botão do WhatsApp
- O sistema conectará automaticamente ao Chrome já aberto
- A mensagem será enviada automaticamente
- **Nenhuma nova janela será aberta**

## ✅ Verificar se está funcionando

1. Abra o sistema
2. Clique em qualquer botão do WhatsApp
3. Se funcionar: mensagem será enviada automaticamente
4. Se não funcionar: aparecerá uma mensagem pedindo para iniciar o Chrome corretamente

## 🔧 Solução de Problemas

### Erro: "WhatsApp Web não está conectado"

**Causa:** O Chrome não está rodando com remote debugging ou o WhatsApp Web não está aberto.

**Solução:**
1. Feche todas as janelas do Chrome
2. Execute `iniciar-chrome-whatsapp.bat`
3. Abra o WhatsApp Web manualmente
4. Tente novamente

### O sistema ainda abre nova janela

**Causa:** O Chrome não está rodando com remote debugging na porta 9222.

**Solução:**
1. Verifique se o Chrome foi iniciado com `--remote-debugging-port=9222`
2. Use sempre o script `iniciar-chrome-whatsapp.bat` para iniciar o Chrome
3. Não inicie o Chrome manualmente sem o remote debugging

### Mensagem não é enviada

**Causa:** O WhatsApp Web não está conectado ou não está na página correta.

**Solução:**
1. Verifique se o WhatsApp Web está aberto e conectado (sem QR Code)
2. Verifique se está na página principal do WhatsApp Web
3. Tente recarregar a página do WhatsApp Web (F5)

## 📝 Notas Importantes

- ⚠️ **SEMPRE** use o script `iniciar-chrome-whatsapp.bat` para iniciar o Chrome
- ⚠️ **NÃO** inicie o Chrome manualmente sem remote debugging
- ✅ O sistema **NÃO** abre nova janela - apenas conecta à existente
- ✅ Você pode manter o Chrome aberto o dia todo
- ✅ O sistema conectará automaticamente quando necessário

## 🎯 Resumo Rápido

1. Execute `iniciar-chrome-whatsapp.bat`
2. Abra WhatsApp Web manualmente
3. Use o sistema normalmente
4. **Pronto!** Nenhuma nova janela será aberta
