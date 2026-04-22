# 🚀 Como Iniciar o Projeto IComanda

## 📋 Pré-requisitos

Antes de iniciar, certifique-se de ter instalado:

1. **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Node.js 18+** - [Download](https://nodejs.org/)
3. **Firebird 2.5** - Instalado e rodando
4. **Banco de dados** - `DADOSG5.FDB` configurado

---

## ⚙️ Configuração Inicial

### 1. Verificar Connection String

Edite o arquivo `IComanda.API/appsettings.json` e verifique a connection string:

```json
{
  "ConnectionStrings": {
    "Firebird": "Server=localhost;Database=C:\\CAMINHO\\PARA\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;"
  }
}
```

**⚠️ IMPORTANTE:** Ajuste o caminho do banco de dados para o seu ambiente!

### 2. Verificar Porta

A porta padrão está configurada em `appsettings.json`:

```json
{
  "Kestrel": {
    "Port": 65375
  }
}
```

---

## 🎯 Formas de Iniciar o Projeto

### Opção 1: Iniciar Backend e Frontend Juntos (Recomendado) ⭐

**Windows:**
```bash
cd IComanda.API
start-dev.bat
```

Este script:
- ✅ Inicia o Backend na porta **65375**
- ✅ Inicia o Frontend na porta **3000**
- ✅ Abre em janelas separadas
- ✅ Instala dependências automaticamente se necessário

**Acessos:**
- Backend: http://localhost:65375
- Frontend: http://localhost:3000
- Swagger: http://localhost:65375/swagger

---

### Opção 2: Iniciar Apenas o Backend

#### Windows (HTTP apenas):
```bash
cd IComanda.API
run-http-only.bat
```

#### Windows (genérico):
```bash
cd IComanda.API
run.bat
```

#### Manual (qualquer sistema):
```bash
cd IComanda.API

# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar
set ASPNETCORE_URLS=http://localhost:65375
dotnet run --no-launch-profile
```

**Acesso:**
- API: http://localhost:65375
- Swagger: http://localhost:65375/swagger
- Health Check: http://localhost:65375/health

---

### Opção 3: Iniciar Apenas o Frontend

```bash
cd IComanda.API/icomanda-frontend

# Instalar dependências (primeira vez)
npm install

# Iniciar
npm start
```

**Acesso:**
- Frontend: http://localhost:3000

---

## 🔍 Verificar se Está Funcionando

### 1. Health Check
Abra no navegador:
```
http://localhost:65375/health
```

Deve retornar:
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy"
  }
}
```

### 2. Swagger
Abra no navegador:
```
http://localhost:65375/swagger
```

Você verá toda a documentação da API e poderá testar os endpoints.

### 3. Testar Endpoint
```bash
# Listar produtos
curl http://localhost:65375/api/produtos/buscar?q=teste

# Listar grupos
curl http://localhost:65375/api/grupos
```

---

## 🐛 Solução de Problemas

### Erro: "Porta já está em uso"

**Solução 1:** Parar o processo que está usando a porta
```bash
# Windows
netstat -ano | findstr :65375
taskkill /PID <PID> /F
```

**Solução 2:** Alterar a porta no `appsettings.json`
```json
{
  "Kestrel": {
    "Port": 5000
  }
}
```

### Erro: "Não foi possível conectar ao banco"

**Verifique:**
1. ✅ Firebird está rodando?
2. ✅ Caminho do banco está correto?
3. ✅ Usuário e senha estão corretos?
4. ✅ Porta do Firebird está correta (3050)?

**Teste a conexão:**
```bash
# No Windows, verifique se o Firebird está rodando
sc query FirebirdServerDefaultInstance
```

### Erro: ".NET SDK não encontrado"

**Instale o .NET 8 SDK:**
- Download: https://dotnet.microsoft.com/download/dotnet/8.0
- Verifique a instalação:
```bash
dotnet --version
```

### Erro: "npm não encontrado"

**Instale o Node.js:**
- Download: https://nodejs.org/
- Verifique a instalação:
```bash
node --version
npm --version
```

---

## 📝 Comandos Úteis

### Backend

```bash
# Restaurar dependências
dotnet restore

# Compilar
dotnet build

# Executar em modo Release
dotnet run --configuration Release

# Publicar
dotnet publish -c Release -o publish
```

### Frontend

```bash
# Instalar dependências
npm install

# Iniciar desenvolvimento
npm start

# Build para produção
npm run build
```

---

## 🎯 Estrutura de URLs

### Backend
- **API Base:** http://localhost:65375
- **Swagger:** http://localhost:65375/swagger
- **Health Check:** http://localhost:65375/health

### Endpoints Principais
- `/api/produtos` - Produtos
- `/api/grupos` - Grupos/Categorias
- `/api/clientes` - Clientes
- `/api/vendas` - Vendas
- `/api/recebimentos` - Recebimentos
- `/api/caixas` - Caixas
- `/api/mesas` - Mesas
- `/api/relatorios` - Relatórios
- `/api/notificacoes` - Notificações
- `/api/historico` - Histórico

### Frontend
- **Aplicação:** http://localhost:3000

---

## ✅ Checklist de Inicialização

Antes de iniciar, verifique:

- [ ] .NET 8 SDK instalado
- [ ] Node.js 18+ instalado
- [ ] Firebird 2.5 rodando
- [ ] Connection string configurada corretamente
- [ ] Porta 65375 disponível (ou alterada no appsettings.json)
- [ ] Porta 3000 disponível (para frontend)

---

## 🚀 Início Rápido

**Para iniciar tudo de uma vez (Windows):**

```bash
cd IComanda.API
start-dev.bat
```

Aguarde alguns segundos e acesse:
- **Frontend:** http://localhost:3000
- **Swagger:** http://localhost:65375/swagger

---

## 📞 Suporte

Se encontrar problemas:
1. Verifique os logs em `IComanda.API/logs/`
2. Verifique o console do backend
3. Verifique o console do frontend (F12 no navegador)
4. Teste o Health Check: http://localhost:65375/health

---

**Boa sorte! 🎉**

