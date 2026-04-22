# 📱 Alternativas para Envio de Mensagens WhatsApp

> **Padrão do iComanda:** envio direto via **Baileys** (Node.js, sem Docker). Rode `npm run dev` na pasta `icomanda-whatsapp-baileys` e conecte pelo modal "Conectar WhatsApp".

Este documento descreve as diferentes formas de enviar mensagens WhatsApp disponíveis no sistema.

## 🎯 Métodos Disponíveis

O sistema suporta **3 métodos** para envio de mensagens WhatsApp:

1. **Baileys (Node)** – Padrão, sem Docker ✅
2. **Link Direto** – Abre WhatsApp Web com mensagem pronta
3. **Selenium WebDriver** – Alternativa legada ⚠️

---

## 1️⃣ Link Direto (Padrão)

### ✅ Vantagens
- **Não requer configuração** - Funciona imediatamente
- **100% confiável** - Sempre funciona
- **Não depende de serviços externos**
- **Não requer Chrome ou navegador especial**

### Como Funciona
O sistema gera um link do WhatsApp Web com o número e mensagem pré-preenchidos. O usuário clica no link e o WhatsApp Web abre automaticamente com tudo pronto - só precisa clicar em "Enviar".

### Configuração
```json
{
  "WhatsApp": {
    "Metodo": "link"
  }
}
```

**Ou simplesmente não configure nada** - o link direto é o padrão!

### Uso
Quando você clica em "Enviar WhatsApp", o sistema:
1. Gera o link automaticamente
2. Abre o WhatsApp Web no navegador
3. Mensagem já está preenchida
4. Você só clica em "Enviar"

---

## 2️⃣ Evolution API (Recomendado para Automação)

### ✅ Vantagens
- **Envio totalmente automático** - Sem intervenção manual
- **Muito mais confiável que Selenium**
- **Open Source e gratuito**
- **API REST simples**
- **Suporta múltiplas instâncias**

### ⚠️ Requisitos
- Evolution API rodando (Docker ou servidor)
- Configuração inicial necessária

### Como Instalar Evolution API

#### Opção 1: Docker (Mais Fácil)
```bash
docker run -d \
  --name evolution-api \
  -p 8080:8080 \
  -e AUTHENTICATION_API_KEY=sua-chave-secreta \
  atendai/evolution-api:latest
```

#### Opção 2: Servidor Dedicado
Siga a documentação oficial: https://doc.evolution-api.com/

### Configuração no appsettings.json
```json
{
  "WhatsApp": {
    "Metodo": "evolution",
    "EvolutionApi": {
      "BaseUrl": "http://localhost:8080",
      "ApiKey": "sua-chave-secreta",
      "InstanceName": "default"
    }
  }
}
```

### Primeira Configuração
1. Inicie o Evolution API
2. Acesse o painel web (geralmente http://localhost:8080)
3. Crie uma instância
4. Escaneie o QR Code com seu WhatsApp
5. Configure no appsettings.json
6. Pronto! Mensagens serão enviadas automaticamente

### Como Funciona
O sistema faz uma chamada HTTP para o Evolution API, que envia a mensagem diretamente pelo WhatsApp conectado. **Totalmente automático!**

---

## 3️⃣ Selenium WebDriver (Método Original)

### ⚠️ Desvantagens
- Requer Chrome com remote debugging
- Pode quebrar com atualizações do WhatsApp Web
- Mais lento e menos confiável
- Requer manutenção constante

### Quando Usar
Apenas se você já estava usando e não quer mudar. **Não recomendado para novos projetos.**

### Configuração
```json
{
  "WhatsApp": {
    "Metodo": "selenium"
  }
}
```

Requer:
- Chrome iniciado com `--remote-debugging-port=9222`
- WhatsApp Web aberto e logado
- Manter Chrome aberto sempre

---

## 🔄 Como o Sistema Escolhe o Método

O sistema tenta usar o método configurado em ordem de preferência:

1. **Método configurado** em `WhatsApp:Metodo`
2. Se não disponível, tenta **Evolution API**
3. Se não disponível, tenta **Selenium**
4. Se nenhum funcionar, usa **Link Direto** como fallback

---

## 📊 Comparação dos Métodos

| Característica | Link Direto | Evolution API | Selenium |
|---------------|-------------|---------------|----------|
| **Configuração** | Nenhuma | Média | Complexa |
| **Confiabilidade** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ |
| **Automação** | Manual | Total | Total |
| **Manutenção** | Nenhuma | Baixa | Alta |
| **Velocidade** | Rápido | Muito Rápido | Lento |
| **Requer Chrome** | ❌ | ❌ | ✅ |
| **Requer Servidor** | ❌ | ✅ | ❌ |

---

## 🚀 Recomendações

### Para Uso Simples (Poucas Mensagens)
👉 **Use Link Direto** - É o padrão e funciona perfeitamente!

### Para Automação Completa (Muitas Mensagens)
👉 **Use Evolution API** - Mais confiável e profissional

### Para Manter Sistema Antigo
👉 **Use Selenium** - Mas considere migrar para Evolution API

---

## 🔧 Como Mudar o Método

1. Abra `appsettings.json`
2. Altere `WhatsApp:Metodo` para:
   - `"link"` - Link direto
   - `"evolution"` - Evolution API
   - `"selenium"` - Selenium WebDriver
3. Se usar Evolution API, configure também `WhatsApp:EvolutionApi`
4. Reinicie a aplicação

---

## 📝 Exemplo de Configuração Completa

```json
{
  "WhatsApp": {
    "Metodo": "evolution",
    "EvolutionApi": {
      "BaseUrl": "http://localhost:8080",
      "ApiKey": "minha-chave-secreta-123",
      "InstanceName": "icomanda"
    }
  }
}
```

---

## ❓ Perguntas Frequentes

### Qual método devo usar?
- **Comece com Link Direto** - É o mais simples
- Se precisar de automação total, migre para **Evolution API**

### O Link Direto é confiável?
✅ Sim! É o método mais confiável. Só requer um clique manual no botão "Enviar".

### Evolution API é gratuito?
✅ Sim! É open source e totalmente gratuito.

### Preciso manter o Chrome aberto?
- **Link Direto**: Não
- **Evolution API**: Não
- **Selenium**: Sim

### Posso usar múltiplos métodos?
O sistema escolhe automaticamente o melhor método disponível baseado na configuração.

---

## 🆘 Suporte

Se tiver problemas:
1. Verifique os logs da aplicação
2. Acesse `/api/whatsapp/diagnostico` para ver o status de todos os métodos
3. Verifique a configuração no `appsettings.json`

---

**Última atualização**: Janeiro 2025
