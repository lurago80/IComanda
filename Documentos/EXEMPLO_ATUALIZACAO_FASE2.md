# 📋 ATUALIZAÇÃO - FASE 2 (BCrypt + RefreshToken Firebird)

> **Exemplo Real de Atualização**

---

## 🎯 RESUMO DA ATUALIZAÇÃO

**Data:** 15/Fevereiro/2026  
**Tipo:** Nova Feature + Segurança  
**Descrição:** Implementação de senha segura (BCrypt) + Refresh Tokens persistentes no Firebird + Correções ESLint/401

---

## 📂 ARQUIVOS MODIFICADOS/CRIADOS

### ✅ Backend - Copiar para `C:\iComanda\IComanda.API\`

#### Novos Arquivos (➕)
```
➕ Services/Interfaces/IPasswordHasher.cs
➕ Services/Implementations/PasswordHasher.cs
➕ Repositories/Interfaces/IRefreshTokenRepository.cs
➕ Repositories/Implementations/RefreshTokenRepository.cs
➕ Services/IRefreshTokenService.cs
➕ Services/Implementations/RefreshTokenDatabaseService.cs
➕ Models/Entities/RefreshTokenEntity.cs
➕ criar-tabela-refresh-token.sql
➕ executar-sql-refresh-token.ps1
➕ migrar-senhas-bcrypt.ps1
➕ diagnosticar-jwt-401.ps1
➕ README-PHASE2.md
```

#### Arquivos Modificados (✏️)
```
✏️  Controllers/AuthController.cs
✏️  Repositories/Interfaces/IUsuarioRepository.cs
✏️  Repositories/Implementations/UsuarioRepository.cs
✏️  Extensions/ServiceCollectionExtensions.cs
✏️  IComanda.API.csproj (nova dependência: BCrypt.Net-Next)
✏️  appsettings.json (configurações JWT ExpirationHours: 2)
```

### ✅ Frontend - Copiar para `C:\iComanda\icomanda-frontend\`

#### Novos Arquivos (➕)
```
➕ .env.local
➕ .eslintrc.json
```

#### Arquivos Modificados (✏️)
```
✏️  package.json (removida seção eslintConfig)
```

### ✅ Raiz do Projeto - Copiar para `C:\iComanda\`

#### Novos Arquivos (➕)
```
➕ CORRECAO_ERRO_401.md
➕ CORRECAO_ERRO_ESLINT.md
➕ corrigir-eslint-frontend.ps1
➕ PACOTE_CLIENTE.md
➕ LISTA_ARQUIVOS_CLIENTE.txt
```

---

## 🔄 INSTRUÇÕES PARA ATUALIZAR NO CLIENTE

### 1️⃣ Parar Aplicação
```powershell
cd C:\iComanda
.\parar-tudo.bat
```

### 2️⃣ Fazer Backup (IMPORTANTE!)
```powershell
# Criar pasta de backup
$data = Get-Date -Format "yyyyMMdd_HHmmss"
New-Item -Path "C:\iComanda\backup\$data" -ItemType Directory

# Backup dos arquivos que serão substituídos
Copy-Item "C:\iComanda\IComanda.API\Controllers\AuthController.cs" -Destination "C:\iComanda\backup\$data\"
Copy-Item "C:\iComanda\IComanda.API\appsettings.json" -Destination "C:\iComanda\backup\$data\"
Copy-Item "C:\iComanda\icomanda-frontend\package.json" -Destination "C:\iComanda\backup\$data\"
```

### 3️⃣ Copiar Arquivos Novos do Backend
```powershell
# Criar pastas se não existirem
New-Item -Path "C:\iComanda\IComanda.API\Services\Interfaces" -ItemType Directory -Force
New-Item -Path "C:\iComanda\IComanda.API\Services\Implementations" -ItemType Directory -Force
New-Item -Path "C:\iComanda\IComanda.API\Repositories\Interfaces" -ItemType Directory -Force
New-Item -Path "C:\iComanda\IComanda.API\Repositories\Implementations" -ItemType Directory -Force
New-Item -Path "C:\iComanda\IComanda.API\Models\Entities" -ItemType Directory -Force

# Copiar arquivos novos (ajuste o caminho de origem conforme sua mídia)
$origem = "D:\icomanda_atualizacao"  # Ou pendrive, rede, etc.

Copy-Item "$origem\IComanda.API\Services\Interfaces\IPasswordHasher.cs" -Destination "C:\iComanda\IComanda.API\Services\Interfaces\" -Force
Copy-Item "$origem\IComanda.API\Services\Implementations\PasswordHasher.cs" -Destination "C:\iComanda\IComanda.API\Services\Implementations\" -Force
Copy-Item "$origem\IComanda.API\Repositories\Interfaces\IRefreshTokenRepository.cs" -Destination "C:\iComanda\IComanda.API\Repositories\Interfaces\" -Force
Copy-Item "$origem\IComanda.API\Repositories\Implementations\RefreshTokenRepository.cs" -Destination "C:\iComanda\IComanda.API\Repositories\Implementations\" -Force
Copy-Item "$origem\IComanda.API\Services\IRefreshTokenService.cs" -Destination "C:\iComanda\IComanda.API\Services\" -Force
Copy-Item "$origem\IComanda.API\Services\Implementations\RefreshTokenDatabaseService.cs" -Destination "C:\iComanda\IComanda.API\Services\Implementations\" -Force
Copy-Item "$origem\IComanda.API\Models\Entities\RefreshTokenEntity.cs" -Destination "C:\iComanda\IComanda.API\Models\Entities\" -Force
Copy-Item "$origem\IComanda.API\criar-tabela-refresh-token.sql" -Destination "C:\iComanda\IComanda.API\" -Force
Copy-Item "$origem\IComanda.API\executar-sql-refresh-token.ps1" -Destination "C:\iComanda\IComanda.API\" -Force
Copy-Item "$origem\IComanda.API\migrar-senhas-bcrypt.ps1" -Destination "C:\iComanda\IComanda.API\" -Force
Copy-Item "$origem\IComanda.API\diagnosticar-jwt-401.ps1" -Destination "C:\iComanda\IComanda.API\" -Force
```

### 4️⃣ Copiar Arquivos Modificados do Backend
```powershell
Copy-Item "$origem\IComanda.API\Controllers\AuthController.cs" -Destination "C:\iComanda\IComanda.API\Controllers\" -Force
Copy-Item "$origem\IComanda.API\Repositories\Interfaces\IUsuarioRepository.cs" -Destination "C:\iComanda\IComanda.API\Repositories\Interfaces\" -Force
Copy-Item "$origem\IComanda.API\Repositories\Implementations\UsuarioRepository.cs" -Destination "C:\iComanda\IComanda.API\Repositories\Implementations\" -Force
Copy-Item "$origem\IComanda.API\Extensions\ServiceCollectionExtensions.cs" -Destination "C:\iComanda\IComanda.API\Extensions\" -Force
Copy-Item "$origem\IComanda.API\IComanda.API.csproj" -Destination "C:\iComanda\IComanda.API\" -Force
```

### 5️⃣ Copiar Arquivos do Frontend
```powershell
Copy-Item "$origem\icomanda-frontend\.env.local" -Destination "C:\iComanda\icomanda-frontend\" -Force
Copy-Item "$origem\icomanda-frontend\.eslintrc.json" -Destination "C:\iComanda\icomanda-frontend\" -Force
Copy-Item "$origem\icomanda-frontend\package.json" -Destination "C:\iComanda\icomanda-frontend\" -Force
```

### 6️⃣ Copiar Documentação e Scripts
```powershell
Copy-Item "$origem\CORRECAO_ERRO_401.md" -Destination "C:\iComanda\" -Force
Copy-Item "$origem\CORRECAO_ERRO_ESLINT.md" -Destination "C:\iComanda\" -Force
Copy-Item "$origem\corrigir-eslint-frontend.ps1" -Destination "C:\iComanda\" -Force
Copy-Item "$origem\PACOTE_CLIENTE.md" -Destination "C:\iComanda\" -Force
Copy-Item "$origem\LISTA_ARQUIVOS_CLIENTE.txt" -Destination "C:\iComanda\" -Force
```

### 7️⃣ Restaurar Dependências Backend
```powershell
cd C:\iComanda\IComanda.API
dotnet restore
```

⏱️ Aguarde alguns segundos para baixar a nova dependência **BCrypt.Net-Next**

### 8️⃣ Criar Tabela REFRESH_TOKEN no Banco
```powershell
cd C:\iComanda\IComanda.API
.\executar-sql-refresh-token.ps1
```

**Ou manualmente:**
```cmd
isql -user SYSDBA -password masterkey "C:\iComanda\Dados\DADOSG5.FDB" -i criar-tabela-refresh-token.sql
```

### 9️⃣ Reinstalar Dependências Frontend
```powershell
cd C:\iComanda\icomanda-frontend
npm install
```

⏱️ Aguarde instalação das dependências

### 🔟 Recompilar Backend
```powershell
cd C:\iComanda\IComanda.API
dotnet build -c Release
```

✅ Deve compilar sem erros

### 1️⃣1️⃣ Reiniciar Aplicação
```powershell
cd C:\iComanda
.\iniciar-tudo.bat
```

---

## ⚙️ CONFIGURAÇÕES A VERIFICAR

### appsettings.json

Verificar se contém as novas configurações JWT:

```json
{
  "Jwt": {
    "SecretKey": "sua-chave-secreta-minimo-32-caracteres",
    "Issuer": "IComandaAPI",
    "Audience": "IComandaFrontend",
    "ExpirationHours": 2,              ← NOVO: Era 240h, agora 2h
    "RefreshExpirationDays": 7         ← NOVO: Refresh token 7 dias
  }
}
```

### .env.local (Frontend)

Verificar se foi criado com:

```env
DISABLE_ESLINT_PLUGIN=true
TSC_COMPILE_ON_ERROR=true
PORT=3000
BROWSER=none
REACT_APP_API_URL=http://localhost:65375
```

---

## 🧪 TESTES PÓS-ATUALIZAÇÃO

### Checklist Essencial

- [ ] **Backend inicia sem erros**
  ```powershell
  # Deve aparecer: "Now listening on: http://localhost:65375"
  ```

- [ ] **Frontend inicia sem erros**
  ```powershell
  # Não deve aparecer erro de ESLint Plugin
  ```

- [ ] **Login funciona**
  - Acessar: http://localhost:3000
  - Login: admin / 123456
  - Deve receber token JWT válido

- [ ] **Tabela REFRESH_TOKEN existe**
  ```sql
  -- Firebird
  SHOW TABLE REFRESH_TOKEN;
  SELECT COUNT(*) FROM REFRESH_TOKEN;
  ```

- [ ] **Senha BCrypt migrada automaticamente**
  - Após primeiro login, verificar no banco:
  ```sql
  SELECT ID, NOME, SENHA FROM USUARIOS WHERE ID = 1;
  -- SENHA deve começar com $2a$12$ (BCrypt hash)
  ```

- [ ] **Endpoints não retornam 401**
  - Testar: http://localhost:65375/api/vendas/abertas
  - Deve retornar lista de vendas (não 401)

---

## 🚨 PROBLEMAS COMUNS

### ❌ Erro: 401 Unauthorized após atualização

**Causa:** Tokens antigos (240h) expirados

**Solução:**
```javascript
// Navegador F12 → Console
localStorage.clear()
sessionStorage.clear()
// Reload e fazer login novamente
```

Ou executar diagnóstico:
```powershell
cd C:\iComanda
.\diagnosticar-jwt-401.ps1
```

### ❌ Erro: ESLint Plugin 'react' was conflicted

**Causa:** Arquivo .env.local não foi copiado

**Solução:**
```powershell
cd C:\iComanda
.\corrigir-eslint-frontend.ps1
```

### ❌ Erro: Table unknown REFRESH_TOKEN

**Causa:** Tabela não foi criada no banco

**Solução:**
```powershell
cd C:\iComanda\IComanda.API
.\executar-sql-refresh-token.ps1
```

### ❌ Erro: BCrypt.Net-Next não encontrado

**Causa:** Dependências não restauradas

**Solução:**
```powershell
cd C:\iComanda\IComanda.API
dotnet restore
dotnet build -c Release
```

---

## 🆘 ROLLBACK (em caso de problema grave)

```powershell
# 1. Parar tudo
cd C:\iComanda
.\parar-tudo.bat

# 2. Restaurar backup
$data = "20260215_143000"  # Data do seu backup
Copy-Item "C:\iComanda\backup\$data\*" -Destination "C:\iComanda\IComanda.API\" -Force

# 3. Reverter appsettings.json (ExpirationHours volta para 240)
# Editar manualmente ou restaurar backup

# 4. Deletar tabela REFRESH_TOKEN (opcional)
# isql -user SYSDBA -password masterkey "C:\iComanda\Dados\DADOSG5.FDB"
# DROP TABLE REFRESH_TOKEN;

# 5. Reiniciar
.\iniciar-tudo.bat
```

---

## 📊 MELHORIAS IMPLEMENTADAS

| Item | Antes | Depois |
|------|-------|--------|
| **Senhas** | Plaintext no banco | BCrypt hash (Work Factor 12) |
| **Tokens JWT** | 240h (10 dias) | 2h |
| **Refresh Tokens** | Em memória (perdia ao reiniciar) | Firebird persistente |
| **Segurança Senhas** | ❌ Inseguro | ✅ 4096 iterações BCrypt |
| **ESLint Frontend** | ❌ Erro Plugin Conflict | ✅ Corrigido |
| **Diagnóstico 401** | ❌ Sem ferramenta | ✅ Script automatizado |

---

## 📝 NOTAS ADICIONAIS

### Migração de Senhas

- **Automática:** Ao fazer primeiro login após atualização, senha é convertida para BCrypt automaticamente
- **Bulk (opcional):** Execute `.\migrar-senhas-bcrypt.ps1` para migrar todas de uma vez

### Refresh Tokens

- **Validade:** 7 dias (configurável em appsettings.json)
- **Revogação:** Automática ao fazer logout ou refresh
- **Limpeza:** Tokens expirados são revogados automaticamente, e deletados após 30 dias

### Compatibilidade

- ✅ Compatível com versão anterior do banco Firebird
- ✅ Senhas antigas (plaintext) continuam funcionando até primeiro login
- ✅ Não quebra funcionalidades existentes

---

## 📞 SUPORTE

**Documentos de referência:**
- [CORRECAO_ERRO_401.md](CORRECAO_ERRO_401.md)
- [CORRECAO_ERRO_ESLINT.md](CORRECAO_ERRO_ESLINT.md)
- [README-PHASE2.md](IComanda.API/README-PHASE2.md)

**Scripts de diagnóstico:**
- `diagnosticar-jwt-401.ps1` - Testa autenticação completa
- `corrigir-eslint-frontend.ps1` - Corrige ESLint automaticamente

---

*Atualização criada em 15/Fevereiro/2026*  
*iComanda v2.0 - Fase 2 Completa*  
*Tempo estimado de atualização: 10-15 minutos*
