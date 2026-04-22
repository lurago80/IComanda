# 📦 O QUE ENVIAR PARA O CLIENTE

## 🎯 Resposta Rápida

**Envie a pasta completa `deploy-package`** que está em:
```
C:\fontes\icomanda\IComanda.API\deploy-package\
```

## 📋 Passo a Passo Completo

### Opção 1: Usar o Script de Deploy (RECOMENDADO)

1. **Execute o script de deploy** para gerar o pacote atualizado:
   ```powershell
   cd C:\fontes\icomanda\IComanda.API
   .\deploy.ps1
   ```

2. **A pasta gerada será**: `deploy-package\`

3. **Envie toda a pasta `deploy-package`** para o cliente

### Opção 2: Criar ZIP Completo (MAIS FÁCIL)

1. **Execute o script que cria um ZIP pronto**:
   ```powershell
   cd C:\fontes\icomanda\IComanda.API
   .\criar-pacote-instalacao.ps1
   ```

2. **O arquivo gerado será**: `IComanda-Instalacao.zip`

3. **Envie apenas o arquivo ZIP** para o cliente

4. **No cliente**: Extraia o ZIP e siga as instruções do `GUIA_INSTALACAO.md`

## 📁 Estrutura da Pasta `deploy-package`

```
deploy-package/
├── backend/                    ← BACKEND COMPLETO (obrigatório)
│   ├── IComanda.API.exe       ← Executável principal
│   ├── IComanda.API.dll       ← Biblioteca principal
│   ├── *.dll                  ← Todas as dependências
│   ├── appsettings.json       ← CONFIGURAR COM CAMINHO DO BANCO!
│   ├── appsettings.json.example
│   ├── INSTALAR.bat           ← Script de instalação
│   ├── install-service.bat    ← Instalar como serviço Windows
│   ├── start-api.bat          ← Iniciar API manualmente
│   └── uninstall-service.bat  ← Desinstalar serviço
│
├── frontend/                   ← FRONTEND COMPILADO (opcional se usar servidor web)
│   └── [arquivos estáticos do React]
│
└── GUIA_INSTALACAO.md          ← Instruções de instalação
```

## ⚠️ IMPORTANTE - Antes de Enviar

### 1. Verificar o `appsettings.json`

**CRÍTICO**: Antes de enviar, verifique se o `appsettings.json` no `deploy-package\backend\` está com o caminho correto do banco de dados do cliente:

```json
{
  "ConnectionStrings": {
    "Firebird": "Server=localhost;Database=C:\\iComanda\\Dados\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;"
  }
}
```

**Ajuste o caminho** `C:\\iComanda\\Dados\\DADOSG5.FDB` para o caminho real do banco no cliente!

### 2. Verificar se está tudo compilado

Execute o deploy para garantir que tudo está atualizado:
```powershell
.\deploy.ps1
```

## 📤 Como Enviar para o Cliente

### Método 1: Pendrive/HD Externo
1. Copie a pasta `deploy-package` completa
2. Cole no pendrive/HD
3. No cliente, copie para `C:\iComanda\` (ou onde preferir)

### Método 2: ZIP (Recomendado)
1. Execute `criar-pacote-instalacao.ps1`
2. Envie o arquivo `IComanda-Instalacao.zip`
3. No cliente, extraia onde desejar

### Método 3: Rede/Cloud
1. Compacte a pasta `deploy-package` em ZIP
2. Envie via email, Google Drive, OneDrive, etc.
3. No cliente, baixe e extraia

## 🚀 No Cliente - Instalação

### Passo 1: Copiar Arquivos
Copie a pasta `deploy-package` para o local desejado, por exemplo:
```
C:\iComanda\
```

### Passo 2: Configurar Banco de Dados
1. Abra `C:\iComanda\deploy-package\backend\appsettings.json`
2. Ajuste o caminho do banco de dados:
   ```json
   {
     "ConnectionStrings": {
       "Firebird": "Server=localhost;Database=C:\\CAMINHO\\REAL\\DO\\BANCO\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;"
     }
   }
   ```

### Passo 3: Instalar
Execute como Administrador:
```batch
cd C:\iComanda\deploy-package\backend
INSTALAR.bat
```

Ou instale como serviço Windows:
```batch
install-service.bat
```

## 📝 Checklist Antes de Enviar

- [ ] Executei `.\deploy.ps1` para gerar o pacote atualizado
- [ ] Verifiquei o `appsettings.json` em `deploy-package\backend\` está com o caminho correto
- [ ] Testei se a aplicação compila sem erros
- [ ] Verifiquei se todas as DLLs estão presentes
- [ ] Incluí o `GUIA_INSTALACAO.md` na pasta
- [ ] Se usar ZIP, executei `criar-pacote-instalacao.ps1`

## 🎯 Resumo Final

**O que enviar**: Toda a pasta `deploy-package\` (ou o ZIP gerado)

**Onde está**: `C:\fontes\icomanda\IComanda.API\deploy-package\`

**Tamanho aproximado**: ~50-100 MB (dependendo se inclui frontend)

**Arquivos essenciais**:
- ✅ `backend\IComanda.API.exe` - Executável principal
- ✅ `backend\appsettings.json` - Configuração (AJUSTAR CAMINHO DO BANCO!)
- ✅ `backend\*.dll` - Todas as dependências
- ✅ `GUIA_INSTALACAO.md` - Instruções

## ❓ Dúvidas?

Consulte o arquivo `GUIA_INSTALACAO.md` que está dentro da pasta `deploy-package` para instruções detalhadas de instalação no cliente.
