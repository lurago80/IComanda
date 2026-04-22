# 🚀 Guia Rápido de Deploy

## Na Máquina de Desenvolvimento

### 1. Criar Pacote de Deploy

Execute o script de deploy:

```powershell
cd IComanda.API
.\deploy.ps1
```

Isso criará uma pasta `deploy-package` com tudo necessário.

### 2. Copiar para o PC Cliente

Copie a pasta `deploy-package` completa para o PC do cliente (ex: via pendrive, rede, etc.)

## No PC do Cliente

### 1. Instalar .NET 8.0 Runtime

- Baixe em: https://dotnet.microsoft.com/download/dotnet/8.0
- Instale o **Runtime** (não precisa do SDK)
- Verifique: `dotnet --version` (deve mostrar 8.0.x)

### 2. Configurar Banco de Dados

1. Copie a pasta `backend` para onde desejar (ex: `C:\IComanda\`)
2. Edite `appsettings.json` e ajuste a connection string:
   ```json
   "ConnectionStrings": {
     "Firebird": "Server=localhost;Database=C:\\Caminho\\Para\\Seu\\Banco.FDB;User=SYSDBA;Password=sua_senha;Port=3050;"
   }
   ```

### 3. Executar

```powershell
cd C:\IComanda\backend
dotnet IComanda.API.dll
```

Ou execute `start-api.bat` (duplo clique)

### 4. Acessar

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger

## Instalar como Serviço Windows (Opcional)

Execute `install-service.bat` como Administrador.

## ⚠️ Importante

- Certifique-se de que o Firebird está instalado e rodando
- Ajuste a connection string com o caminho correto do banco
- Libere a porta no firewall se necessário

Para mais detalhes, consulte `GUIA_INSTALACAO.md`

