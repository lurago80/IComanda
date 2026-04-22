# 📦 PACOTE COMPLETO PARA INSTALAÇÃO NO CLIENTE

> **Última atualização:** 15/Fevereiro/2026  
> **Versão:** 2.0 - Inclui Fase 2 (BCrypt + RefreshToken Firebird)

---

## 🎯 RESPOSTA RÁPIDA

### Enviar para o Cliente:

```
📁 PASTA COMPLETA: C:\VS_Code\icomanda\
```

**Ou criar ZIP com script:**
```powershell
cd C:\VS_Code\icomanda\IComanda.API
.\criar-pacote-instalacao.ps1
```

---

## 📂 ESTRUTURA DE ARQUIVOS PARA O CLIENTE

### ✅ OBRIGATÓRIOS - Backend

```
C:\VS_Code\icomanda\
│
├── IComanda.API\                           ← PASTA PRINCIPAL BACKEND
│   ├── bin\Release\net8.0\publish\         ← BINÁRIOS COMPILADOS
│   │   ├── IComanda.API.exe                ← Executável
│   │   ├── IComanda.API.dll                ← DLL principal
│   │   ├── FirebirdSql.Data.FirebirdClient.dll
│   │   ├── BCrypt.Net-Next.dll             ← Senha segura (Fase 2)
│   │   ├── Dapper.dll
│   │   ├── Swashbuckle.AspNetCore.*.dll
│   │   └── [todas as outras DLLs]
│   │
│   ├── appsettings.json                    ← ⚠️ CONFIGURAR CAMINHO DO BANCO!
│   ├── appsettings.json.example            ← Exemplo
│   │
│   ├── INSTALAR.bat                        ← Script instalação
│   ├── iniciar-tudo.bat                    ← Inicia backend+frontend
│   ├── parar-tudo.bat                      ← Para tudo
│   │
│   ├── executar-sql-refresh-token.ps1      ← Criar tabela REFRESH_TOKEN
│   ├── criar-tabela-refresh-token.sql      ← SQL da tabela
│   │
│   ├── diagnosticar-jwt-401.ps1            ← Diagnóstico 401 errors
│   ├── CORRECAO_ERRO_401.md                ← Como resolver 401
│   │
│   ├── GUIA_INSTALACAO.md                  ← Guia completo
│   ├── README.md                           ← Documentação
│   └── README-PHASE2.md                    ← Doc Fase 2
│
├── icomanda-frontend\                      ← PASTA FRONTEND
│   ├── build\                              ← BUILD DE PRODUÇÃO
│   │   ├── index.html
│   │   ├── static\
│   │   │   ├── css\
│   │   │   └── js\
│   │   └── [arquivos estáticos]
│   │
│   ├── .env.local                          ← Fix ESLint (Novo!)
│   ├── .eslintrc.json                      ← Config ESLint (Novo!)
│   ├── package.json
│   └── node_modules\                       ← ⚠️ NÃO ENVIAR! Reinstalar no cliente
│
├── icomanda-whatsapp-baileys\              ← WHATSAPP (OPCIONAL)
│   ├── server.js
│   ├── package.json
│   └── node_modules\                       ← ⚠️ NÃO ENVIAR! Reinstalar no cliente
│
├── CORRECAO_ERRO_ESLINT.md                 ← Como corrigir ESLint no cliente
├── CORRECAO_ERRO_401.md                    ← Como corrigir 401 no cliente
└── README.md                               ← Documentação geral
```

---

## 📦 OPÇÃO 1: CRIAR PACOTE ZIP (RECOMENDADO)

### Script Automático

```powershell
cd C:\VS_Code\icomanda\IComanda.API
.\criar-pacote-instalacao.ps1
```

**Resultado:** `IComanda-Instalacao.zip` com tudo necessário

**Enviar:** Apenas o arquivo ZIP para o cliente

---

## 📦 OPÇÃO 2: COPIAR PASTA COMPLETA

### Passo a Passo

1. **Compilar Backend:**
   ```powershell
   cd C:\VS_Code\icomanda\IComanda.API
   dotnet publish IComanda.API.csproj -c Release -o bin\Release\net8.0\publish
   ```

2. **Buildar Frontend:**
   ```powershell
   cd C:\VS_Code\icomanda\icomanda-frontend
   npm run build
   ```

3. **Copiar para Pendrive/Rede:**
   - Copie `C:\VS_Code\icomanda\` completo
   - **EXCETO:** `node_modules\` (muito grande, reinstalar no cliente)
   - **EXCETO:** `bin\Debug\` e `obj\` (não necessário)

---

## ⚙️ CONFIGURAÇÕES CRÍTICAS ANTES DE ENVIAR

### 1️⃣ appsettings.json - Caminho do Banco

**Arquivo:** `C:\VS_Code\icomanda\IComanda.API\appsettings.json`

```json
{
  "ConnectionStrings": {
    "Firebird": "Server=localhost;Database=C:\\iComanda\\Dados\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8;"
  },
  "Jwt": {
    "SecretKey": "sua-chave-secreta-minimo-32-caracteres-aqui-12345678",
    "Issuer": "IComandaAPI",
    "Audience": "IComandaFrontend",
    "ExpirationHours": 2,
    "RefreshExpirationDays": 7
  }
}
```

**⚠️ AJUSTAR:**
- `Database` - Caminho do banco Firebird do cliente
- `SecretKey` - Chave JWT (mínimo 32 caracteres)

### 2️⃣ Frontend .env.local - Config

**Arquivo:** `C:\VS_Code\icomanda\icomanda-frontend\.env.local`

```env
DISABLE_ESLINT_PLUGIN=true
TSC_COMPILE_ON_ERROR=true
PORT=3000
BROWSER=none
REACT_APP_API_URL=http://localhost:65375
```

**✅ Já incluído!** (conserta erro ESLint Plugin Conflict)

### 3️⃣ Criar Tabela REFRESH_TOKEN

**No cliente, executar:**
```powershell
cd C:\iComanda\IComanda.API
.\executar-sql-refresh-token.ps1
```

Ou executar SQL manualmente:
```sql
-- Ver arquivo: criar-tabela-refresh-token.sql
```

---

## 🚀 INSTALAÇÃO NO CLIENTE - GUIA RÁPIDO

### Pré-requisitos

**No PC do cliente, instalar:**

1. **.NET 8.0 Runtime** (não SDK)
   - https://dotnet.microsoft.com/download/dotnet/8.0
   - Baixar: **.NET Desktop Runtime 8.0.x** (Windows x64)

2. **Node.js 18+ LTS** (para frontend)
   - https://nodejs.org/
   - Baixar: LTS (Long Term Support)

3. **Firebird 2.5 Server** (se não tiver)
   - https://firebirdsql.org/

---

### Passos de Instalação

#### 1. Extrair/Copiar Arquivos

```
ZIP → Extrair para: C:\iComanda\
```

Ou copiar pasta completa para `C:\iComanda\`

#### 2. Configurar Banco de Dados

Edite: `C:\iComanda\IComanda.API\appsettings.json`

Ajuste o caminho do banco (Database):
```json
"Database": "C:\\CAMINHO\\REAL\\DO\\BANCO\\DADOSG5.FDB"
```

#### 3. Criar Tabela REFRESH_TOKEN

```powershell
cd C:\iComanda\IComanda.API
.\executar-sql-refresh-token.ps1
```

Ou conectar no Firebird e executar `criar-tabela-refresh-token.sql`

#### 4. Instalar Dependências do Frontend

```powershell
cd C:\iComanda\icomanda-frontend
npm install
```

⚠️ **Se der erro de ESLint**, execute:
```powershell
cd C:\iComanda
.\corrigir-eslint-frontend.ps1
```

#### 5. Iniciar Aplicação

```batch
cd C:\iComanda
.\iniciar-tudo.bat
```

**Ou manualmente:**

**Backend:**
```powershell
cd C:\iComanda\IComanda.API
dotnet run --configuration Release
```

**Frontend (outra janela):**
```powershell
cd C:\iComanda\icomanda-frontend
npm start
```

#### 6. Acessar Sistema

- **Frontend:** http://localhost:3000
- **API:** http://localhost:65375
- **Swagger:** http://localhost:65375/swagger

**Login padrão:**
- Usuário: `admin`
- Senha: `123456`

---

## 🔧 TROUBLESHOOTING - PROBLEMAS COMUNS

### ❌ Erro: 401 Unauthorized

**Sintoma:** Login funciona mas endpoints retornam 401

**Solução:**
```powershell
# Executar diagnóstico
cd C:\iComanda
.\diagnosticar-jwt-401.ps1

# Limpar localStorage navegador
# F12 → Console → localStorage.clear()
# Fazer login novamente
```

**Documento completo:** [CORRECAO_ERRO_401.md](CORRECAO_ERRO_401.md)

### ❌ Erro: ESLint Plugin 'react' was conflicted

**Sintoma:** Frontend não compila, erro de plugin ESLint

**Solução:**
```powershell
cd C:\iComanda
.\corrigir-eslint-frontend.ps1
```

**Documento completo:** [CORRECAO_ERRO_ESLINT.md](CORRECAO_ERRO_ESLINT.md)

### ❌ Erro: Firebird - cannot open database

**Sintoma:** Backend não conecta no banco

**Solução:**
1. Verificar caminho do banco em `appsettings.json`
2. Verificar se arquivo `.FDB` existe
3. Verificar se Firebird Server está rodando:
   ```powershell
   netstat -ano | Select-String "3050"
   ```
4. Testar conexão:
   ```cmd
   isql -user SYSDBA -password masterkey "C:\iComanda\Dados\DADOSG5.FDB"
   ```

### ❌ Erro: Port 65375 already in use

**Sintoma:** Backend não inicia, porta ocupada

**Solução:**
```powershell
# Ver processo usando porta
netstat -ano | Select-String "65375"

# Matar processo (substitua PID)
taskkill /PID 12345 /F

# Ou reiniciar tudo
.\parar-tudo.bat
.\iniciar-tudo.bat
```

### ❌ Erro: npm ERR! node_modules

**Sintoma:** Frontend não instala dependências

**Solução:**
```powershell
cd C:\iComanda\icomanda-frontend

# Limpar cache
Remove-Item -Recurse -Force node_modules
Remove-Item -Force package-lock.json

# Reinstalar
npm cache clean --force
npm install
```

---

## 📋 CHECKLIST COMPLETO

### Antes de Enviar ao Cliente

- [ ] Backend compilado (`dotnet publish -c Release`)
- [ ] Frontend buildado (`npm run build`)
- [ ] `appsettings.json` configurado com caminho correto
- [ ] `.env.local` criado no frontend
- [ ] `.eslintrc.json` criado no frontend
- [ ] `package.json` sem `eslintConfig` duplicado
- [ ] Script `executar-sql-refresh-token.ps1` incluído
- [ ] SQL `criar-tabela-refresh-token.sql` incluído
- [ ] Scripts de diagnóstico incluídos
- [ ] Documentação incluída (README, guias)
- [ ] `node_modules` removido (será reinstalado)
- [ ] Testado localmente antes de enviar

### No Cliente - Pós-Instalação

- [ ] .NET 8.0 Runtime instalado
- [ ] Node.js 18+ instalado
- [ ] Firebird 2.5 instalado e rodando
- [ ] Arquivo `.FDB` existe e está acessível
- [ ] `appsettings.json` ajustado com caminho do banco
- [ ] Tabela `REFRESH_TOKEN` criada no banco
- [ ] `npm install` executado no frontend
- [ ] Backend inicia sem erros
- [ ] Frontend inicia sem erros
- [ ] Login funciona (admin/123456)
- [ ] Endpoints retornam dados (não 401)
- [ ] Vendas abertas aparecem
- [ ] Produtos são buscados corretamente

---

## 📁 ARQUIVOS NOVOS DA FASE 2

**Importante incluir:**

### Backend (Segurança + Performance)

```
IComanda.API\
├── Services\Interfaces\IPasswordHasher.cs          ← BCrypt interface
├── Services\Implementations\PasswordHasher.cs      ← BCrypt implementation
├── Repositories\Interfaces\IRefreshTokenRepository.cs
├── Repositories\Implementations\RefreshTokenRepository.cs
├── Services\Implementations\RefreshTokenDatabaseService.cs
├── Models\Entities\RefreshTokenEntity.cs
├── criar-tabela-refresh-token.sql                  ← SQL tabela
├── executar-sql-refresh-token.ps1                  ← Script criar tabela
├── migrar-senhas-bcrypt.ps1                        ← Migrar senhas (opcional)
├── diagnosticar-jwt-401.ps1                        ← Diagnóstico 401
└── README-PHASE2.md                                ← Documentação Fase 2
```

### Frontend (Fixes ESLint)

```
icomanda-frontend\
├── .env.local                                      ← Fix ESLint plugin
├── .eslintrc.json                                  ← Config ESLint
└── src\utils\logger.ts                             ← Logger profissional
```

### Documentação

```
├── CORRECAO_ERRO_ESLINT.md                         ← Como corrigir ESLint
├── CORRECAO_ERRO_401.md                            ← Como corrigir 401
├── README-PHASE2.md                                ← Fase 2 completa
└── PACOTE_CLIENTE.md                               ← Este arquivo
```

---

## 🎯 RESUMO EXECUTIVO

### O que enviar?

**Opção Simples:** Pasta completa `C:\VS_Code\icomanda\`  
**Opção Melhor:** ZIP criado por `criar-pacote-instalacao.ps1`

### Tamanho estimado

- **Backend:** ~30-50 MB (com DLLs)
- **Frontend (build):** ~5-10 MB
- **Frontend (node_modules):** ~200-300 MB ⚠️ **NÃO ENVIAR!**
- **Total ZIP:** ~40-70 MB

### Arquivos críticos

1. ✅ `IComanda.API.exe` e DLLs
2. ✅ `appsettings.json` (configurar!)
3. ✅ `criar-tabela-refresh-token.sql`
4. ✅ `executar-sql-refresh-token.ps1`
5. ✅ `.env.local` (frontend)
6. ✅ `.eslintrc.json` (frontend)
7. ✅ Scripts de diagnóstico
8. ✅ Documentação (README, guias)

### Primeiro teste no cliente

```powershell
# 1. Criar tabela
.\executar-sql-refresh-token.ps1

# 2. Instalar frontend
cd icomanda-frontend
npm install

# 3. Iniciar tudo
cd ..
.\iniciar-tudo.bat

# 4. Acessar
# http://localhost:3000
# Login: admin / 123456
```

### Se der problema

```powershell
# Diagnóstico 401
.\diagnosticar-jwt-401.ps1

# Corrigir ESLint
.\corrigir-eslint-frontend.ps1

# Reiniciar tudo
.\parar-tudo.bat
.\iniciar-tudo.bat
```

---

## 📞 SUPORTE

**Documentos de referência:**
- [CORRECAO_ERRO_401.md](CORRECAO_ERRO_401.md) - Erros 401 Unauthorized
- [CORRECAO_ERRO_ESLINT.md](CORRECAO_ERRO_ESLINT.md) - Erros ESLint frontend
- [README-PHASE2.md](IComanda.API/README-PHASE2.md) - Fase 2 completa
- [GUIA_INSTALACAO.md](IComanda.API/GUIA_INSTALACAO.md) - Instalação detalhada

**Scripts de diagnóstico:**
- `diagnosticar-jwt-401.ps1` - Testa backend, login, tokens
- `corrigir-eslint-frontend.ps1` - Corrige ESLint automaticamente

---

*Gerado em 15/Fevereiro/2026*  
*iComanda v2.0 - Fase 2 (BCrypt + RefreshToken Firebird)*
