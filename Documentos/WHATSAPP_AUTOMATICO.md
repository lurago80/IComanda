# 📱 Envio Automático de Mensagens via WhatsApp Web

## 🎯 Funcionalidade

O sistema agora possui **envio automático de mensagens** via WhatsApp Web usando automação com Selenium WebDriver. As mensagens são enviadas **automaticamente**, sem necessidade de intervenção manual.

## ⚙️ Como Funciona

1. **Backend (.NET)** usa Selenium WebDriver para controlar o navegador Chrome
2. **Abre o WhatsApp Web** automaticamente
3. **Navega para o contato** e preenche a mensagem
4. **Clica no botão "Enviar"** automaticamente
5. **Mensagem é enviada** sem intervenção do usuário

## 📋 Pré-requisitos

1. **Chrome ou Edge** instalado no servidor
2. **ChromeDriver** (instalado automaticamente via NuGet)
3. **Primeira configuração**: Fazer login manual no WhatsApp Web uma vez

## 🚀 Primeira Configuração

### Passo 1: Inicializar o WhatsApp Web

Na primeira vez, você precisa fazer o login manualmente:

1. A API abrirá automaticamente o navegador Chrome
2. Escaneie o QR Code do WhatsApp Web
3. Após o login, a sessão será salva automaticamente
4. Nas próximas vezes, o login será automático

### Passo 2: Verificar Status

Você pode verificar se está conectado através do endpoint:

```
GET /api/whatsapp/status
```

## 🔧 Configuração

O serviço é registrado automaticamente como **Singleton** no `Program.cs`, mantendo uma única instância do navegador aberta.

### Perfil do Usuário

O perfil do Chrome é salvo em:
```
%LocalAppData%\IComanda\WhatsAppProfile
```

Isso mantém a sessão do WhatsApp Web ativa entre reinicializações.

## 📡 Endpoints da API

### Enviar Mensagem
```
POST /api/whatsapp/enviar
Body: {
  "telefone": "5511999999999",
  "mensagem": "Sua mensagem aqui"
}
```

### Verificar Status
```
GET /api/whatsapp/status
Response: { "conectado": true/false }
```

### Inicializar (abrir navegador)
```
POST /api/whatsapp/inicializar
```

## 💻 Uso no Frontend

O frontend já está configurado para usar automaticamente:

1. **Tenta enviar via automação** (backend)
2. **Se falhar**, usa o método manual (abre WhatsApp Web)

```typescript
await enviarWhatsApp(telefone, mensagem);
```

## ⚠️ Importante

- O navegador Chrome ficará **aberto** enquanto a aplicação estiver rodando
- Na **primeira vez**, é necessário fazer login manual
- A sessão é **mantida** entre reinicializações
- Se o WhatsApp Web desconectar, será necessário fazer login novamente

## 🐛 Troubleshooting

### Navegador não abre
- Verifique se o Chrome/Edge está instalado
- Verifique se o ChromeDriver foi instalado corretamente

### Mensagem não é enviada
- Verifique se o WhatsApp Web está conectado: `GET /api/whatsapp/status`
- Verifique os logs da aplicação
- Tente reinicializar: `POST /api/whatsapp/inicializar`

### Login perdido
- Delete a pasta do perfil: `%LocalAppData%\IComanda\WhatsAppProfile`
- Reinicialize o serviço
- Faça login novamente

## 🔒 Segurança

- O navegador roda apenas no servidor
- Não expõe credenciais
- Usa perfil isolado do Chrome
- Sessão mantida localmente
