# 📦 Guia de Instalação - IComanda API

Este guia explica como instalar e configurar a aplicação IComanda em um PC cliente.

## 📋 Requisitos do Sistema

### Backend (.NET)
- **Windows 10/11** ou **Windows Server 2016+**
- **.NET 8.0 Runtime** (ou SDK para desenvolvimento)
  - Download: https://dotnet.microsoft.com/download/dotnet/8.0
- **Firebird 2.5** ou superior instalado
- **Porta 5000** (ou configurada) disponível para a API

### Frontend
- Navegador moderno (Chrome, Edge, Firefox)
- Servidor web (IIS, Nginx) ou pode ser servido pelo próprio backend

## 🚀 Passo a Passo de Instalação

### 1. Preparar Arquivos para Deploy

Na máquina de desenvolvimento, execute:

```powershell
# Windows
cd IComanda.API
.\publish.ps1
```

Isso criará:
- `IComanda.API\publish\` - Backend publicado
- `IComanda.API\icomanda-frontend\build\` - Frontend buildado

### 2. Copiar Arquivos para o PC Cliente

Copie a pasta `publish` completa para o PC do cliente (ex: `C:\IComanda\`)

### 3. Instalar .NET Runtime no PC Cliente

1. Baixe o **.NET 8.0 Runtime** (não SDK) em:
   https://dotnet.microsoft.com/download/dotnet/8.0

2. Execute o instalador e siga as instruções

3. Verifique a instalação:
```powershell
dotnet --version
```
Deve mostrar: `8.0.x` ou superior

### 4. Configurar Banco de Dados

#### 4.1. Verificar Firebird
- Certifique-se de que o Firebird está instalado e rodando
- Anote o caminho do banco de dados (ex: `C:\Dados\ICOMANDA.FDB`)

#### 4.2. Configurar Connection String

Edite o arquivo `appsettings.json` na pasta `publish`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=C:\\Dados\\ICOMANDA.FDB;User=SYSDBA;Password=masterkey;Charset=UTF8;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**⚠️ IMPORTANTE:** Ajuste:
- `Server` - IP ou nome do servidor Firebird (localhost se local)
- `Database` - Caminho completo do arquivo .FDB
- `User` - Usuário do Firebird (geralmente SYSDBA)
- `Password` - Senha do Firebird

### 5. Configurar CORS (se necessário)

Se o frontend estiver em outro servidor, ajuste o CORS no `Program.cs` ou `appsettings.json`.

### 6. Executar a Aplicação

#### Opção 1: Execução Manual
```powershell
cd C:\IComanda\publish
dotnet IComanda.API.dll
```

#### Opção 2: Criar Serviço Windows (Recomendado)

Crie um arquivo `install-service.bat`:

```batch
@echo off
sc create IComandaAPI binPath= "C:\IComanda\publish\IComanda.API.exe" start= auto
sc description IComandaAPI "IComanda API Service"
sc start IComandaAPI
```

E `uninstall-service.bat`:

```batch
@echo off
sc stop IComandaAPI
sc delete IComandaAPI
```

### 7. Configurar Frontend

#### Opção A: Servir pelo Backend (Recomendado)

Configure o backend para servir arquivos estáticos do frontend.

#### Opção B: Servidor Web Separado

1. Copie a pasta `build` do frontend para um servidor web (IIS, Nginx)
2. Configure o servidor para servir os arquivos estáticos
3. Ajuste a URL da API no frontend (se necessário)

### 8. Verificar Instalação

1. Abra o navegador e acesse:
   - API: `http://localhost:5000/swagger` (para verificar a API)
   - Frontend: `http://localhost:3000` (se servido separadamente)

2. Teste o login e funcionalidades básicas

## 🔧 Configurações Adicionais

### Porta Personalizada

Para mudar a porta, edite `appsettings.json` ou use variável de ambiente:

```powershell
$env:ASPNETCORE_URLS="http://localhost:8080"
dotnet IComanda.API.dll
```

### Firewall

Se necessário, libere a porta no firewall do Windows:

```powershell
New-NetFirewallRule -DisplayName "IComanda API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

### Logs

Os logs são salvos em: `IComanda.API\logs\`

## 📝 Checklist de Instalação

- [ ] .NET 8.0 Runtime instalado
- [ ] Firebird instalado e rodando
- [ ] Banco de dados configurado
- [ ] `appsettings.json` configurado com connection string correta
- [ ] Arquivos copiados para o PC cliente
- [ ] Aplicação testada e funcionando
- [ ] Firewall configurado (se necessário)
- [ ] Serviço Windows criado (se necessário)

## 🆘 Troubleshooting

### Erro: "dotnet não é reconhecido"
- Instale o .NET 8.0 Runtime
- Reinicie o terminal/PowerShell

### Erro: "Cannot open database"
- Verifique o caminho do banco no `appsettings.json`
- Verifique se o Firebird está rodando
- Verifique permissões de acesso ao arquivo .FDB

### Erro: "Port already in use"
- Mude a porta no `appsettings.json`
- Ou pare o processo que está usando a porta

### Erro de CORS
- Configure o CORS no `Program.cs` para permitir a origem do frontend

## 📞 Suporte

Em caso de problemas, verifique:
1. Logs em `IComanda.API\logs\`
2. Console do terminal onde a aplicação está rodando
3. Event Viewer do Windows (se instalado como serviço)

