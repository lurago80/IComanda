# 📦 Como Instalar o IComanda

Este guia explica como instalar todas as dependências do IComanda (Backend e Frontend).

## 🚀 Instalação Automática (Recomendado)

### Opção 1: Usando o Script Batch (Mais Fácil)

1. **Clique duas vezes** no arquivo:
   ```
   INSTALAR.bat
   ```

2. O script irá:
   - ✅ Verificar se todos os pré-requisitos estão instalados
   - ❌ Avisar o que está faltando
   - 📦 Instalar dependências do Backend (.NET)
   - 📦 Instalar dependências do Frontend (Node.js)

### Opção 2: Usando PowerShell

Abra o PowerShell na pasta do projeto e execute:

```powershell
.\INSTALAR.ps1
```

## 🔍 Verificar Pré-requisitos (Sem Instalar)

Se você quiser apenas verificar o que está instalado, sem instalar nada:

1. **Clique duas vezes** no arquivo:
   ```
   VERIFICAR_PRE_REQUISITOS.bat
   ```

Ou via PowerShell:

```powershell
.\VERIFICAR_PRE_REQUISITOS.ps1
```

## 📋 Pré-requisitos Necessários

### Backend (.NET)
- ✅ **.NET SDK 8.0 ou superior**
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
  - Verificar: `dotnet --version`

### Frontend (React)
- ✅ **Node.js 16 ou superior**
  - Download: https://nodejs.org/
  - Verificar: `node --version`
- ✅ **npm** (vem com Node.js)
  - Verificar: `npm --version`

## 🛠️ Instalação Manual

Se preferir instalar manualmente:

### Backend

```powershell
cd IComanda.API
dotnet restore
dotnet build
```

### Frontend

```powershell
cd icomanda-frontend
npm install
```

## ⚙️ Opções do Instalador

O script `INSTALAR.ps1` aceita os seguintes parâmetros:

```powershell
# Instalar apenas o backend
.\INSTALAR.ps1 -BackendOnly

# Instalar apenas o frontend
.\INSTALAR.ps1 -FrontendOnly

# Pular verificação de pré-requisitos (não recomendado)
.\INSTALAR.ps1 -SkipChecks
```

## ✅ Verificação Pós-Instalação

Após a instalação, você pode verificar se tudo está OK:

### Backend
```powershell
cd IComanda.API
dotnet build
```

### Frontend
```powershell
cd icomanda-frontend
npm run build
```

## 🚀 Iniciar o Sistema

Após a instalação, você pode iniciar o sistema:

### Opção 1: Script Automático
```
INICIAR_TUDO.bat
```

### Opção 2: Manual

**Terminal 1 - Backend:**
```powershell
cd IComanda.API
dotnet run
```

**Terminal 2 - Frontend:**
```powershell
cd icomanda-frontend
npm start
```

## ❓ Problemas Comuns

### Erro: "PowerShell não encontrado"
- Instale o PowerShell: https://aka.ms/powershell

### Erro: ".NET SDK não encontrado"
- Instale o .NET SDK 8.0: https://dotnet.microsoft.com/download/dotnet/8.0
- Reinicie o terminal após instalar

### Erro: "Node.js não encontrado"
- Instale o Node.js 16+: https://nodejs.org/
- Reinicie o terminal após instalar

### Erro: "npm install falhou"
- Verifique sua conexão com a internet
- Tente limpar o cache: `npm cache clean --force`
- Tente novamente: `npm install`

### Erro: "dotnet restore falhou"
- Verifique sua conexão com a internet
- Tente limpar o cache: `dotnet nuget locals all --clear`
- Tente novamente: `dotnet restore`

## 📝 Estrutura Esperada

O instalador espera a seguinte estrutura:

```
icomanda/
├── IComanda.API/
│   ├── IComanda.API.csproj
│   └── INSTALAR.ps1
└── icomanda-frontend/
    └── package.json
```

## 🔄 Reinstalar

Se precisar reinstalar tudo:

1. **Backend:**
   ```powershell
   cd IComanda.API
   Remove-Item -Recurse -Force obj, bin -ErrorAction SilentlyContinue
   dotnet restore
   ```

2. **Frontend:**
   ```powershell
   cd icomanda-frontend
   Remove-Item -Recurse -Force node_modules -ErrorAction SilentlyContinue
   npm install
   ```

Ou simplesmente execute `INSTALAR.bat` novamente - ele detecta se já está instalado e atualiza se necessário.

## 📞 Suporte

Se tiver problemas:
1. Execute `VERIFICAR_PRE_REQUISITOS.bat` para ver o que está faltando
2. Verifique os logs de erro
3. Consulte a documentação do projeto

---

**Última atualização**: Janeiro 2025
