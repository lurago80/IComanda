# 🔐 FASE 2 - QUALIDADE & PERFORMANCE

## ✅ Implementações Concluídas

### 1. Password Hashing com BCrypt ⭐️ **CRÍTICO**

**Problema:** Senhas armazenadas em plaintext no banco de dados Firebird (risco gravíssimo de segurança)

**Solução Implementada:**

#### 📦 Componentes Criados

1. **IPasswordHasher** (`Services/Interfaces/IPasswordHasher.cs`)
   - Interface para serviço de hashing de senhas
   - Métodos: `HashPassword()`, `VerifyPassword()`, `IsPasswordHashed()`

2. **PasswordHasher** (`Services/Implementations/PasswordHasher.cs`)
   - Implementação usando BCrypt.Net-Next
   - Work Factor: 12 (seguro para 2026)
   - ~0.3s por hash (resistente a GPU cracking)
   - Salt automático incluído no hash

3. **AuthController** - Atualizado
   - Validação de senha com BCrypt
   - **Migração automática**: Senhas em plaintext são detectadas e migradas para hash no primeiro login
   - Logs detalhados do processo de migração

4. **UsuarioRepository** - Novo método
   - `AtualizarSenhaAsync()`: Atualiza hash de senha no banco

5. **Script de Migração em Lote** (`migrar-senhas-bcrypt.ps1`)
   - Migra TODAS as senhas de uma vez (opcional)
   - Modo dry-run para testar antes
   - Backup automático recomendado

#### 🔐 Segurança

- **BCrypt**: Algoritmo adaptativo, aumenta complexidade automaticamente
- **Salt**: Gerado automaticamente para cada senha (aleatório, 128-bit)
- **Work Factor 12**: 4096 iterations (2^12)
- **Resistant**: Rainbow tables, timing attacks, brute force

#### 📊 Formato do Hash

```
$2a$12$[22 chars salt][31 chars hash]
Total: 60 caracteres

Exemplo:
$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyKw1Nbr0mXG
```

#### 🔄 Migração Automática

**Como funciona:**

1. Usuário faz login com senha plaintext
2. Sistema detecta que senha não está em hash (verifica formato BCrypt)
3. Valida senha por comparação direta (plaintext)
4. **Se válida**: Gera hash BCrypt e atualiza no banco
5. Próximo login já usa verificação BCrypt

**Log de exemplo:**
```
[INFO] Senha em plaintext detectada para usuário 1 - será migrada para hash
[INFO] ✅ Senha do usuário 1 migrada para BCrypt hash com sucesso
```

#### 📦 Package Adicionado

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

---

### 2. Refresh Token persistido em Firebird ⭐️ **IMPORTANTE**

**Problema:** Refresh tokens armazenados em memória (perdidos ao reiniciar servidor, não escala horizontalmente)

**Solução Implementada:**

#### 📦 Componentes Criados

1. **Tabela REFRESH_TOKEN** (Firebird)
   ```sql
   CREATE TABLE REFRESH_TOKEN (
       ID              INTEGER NOT NULL,
       TOKEN           VARCHAR(100) NOT NULL UNIQUE,
       USUARIO_ID      INTEGER NOT NULL,
       USUARIO_NOME    VARCHAR(100),
       USUARIO_ROLE    VARCHAR(50),
       DATA_CRIACAO    TIMESTAMP NOT NULL,
       DATA_EXPIRACAO  TIMESTAMP NOT NULL,
       REVOGADO        CHAR(1) DEFAULT '0',
       MOTIVO_REVOGACAO VARCHAR(200),
       DATA_REVOGACAO  TIMESTAMP
   );
   ```

2. **RefreshTokenEntity** (`Models/Entities/RefreshTokenEntity.cs`)
   - Entidade mapeada para tabela REFRESH_TOKEN
   - Propriedades: `IsExpired`, `IsRevoked`, `IsValid`

3. **IRefreshTokenRepository** (`Repositories/Interfaces/IRefreshTokenRepository.cs`)
   - Interface para operações CRUD de tokens
   - Métodos: `SalvarAsync()`, `BuscarPorTokenAsync()`, `RevogarAsync()`, etc.

4. **RefreshTokenRepository** (`Repositories/Implementations/RefreshTokenRepository.cs`)   - Implementação com Dapper + Firebird
   - Queries otimizadas com índices
   - Limpeza automática de tokens expirados (> 30 dias)
   - Logging detalhado

5. **RefreshTokenDatabaseService** (`Services/Implementations/RefreshTokenDatabaseService.cs`)
   - Substituição do RefreshTokenService em memória
   - Persiste todos os tokens no Firebird
   - Suporta revogação e expiração

6. **Scripts de Setup**
   - `criar-tabela-refresh-token.sql`: DDL para criar tabela
   - `executar-sql-refresh-token.ps1`: Automatiza criação da tabela

#### 🎯 Benefícios

| Aspecto | Antes (Memória) | Depois (Firebird) |
|---------|----------------|-------------------|
| **Persistência** | ❌ Perdido ao reiniciar | ✅ Permanente |
| **Escalabilidade** | ❌ Apenas 1 instância | ✅ Multi-instância |
| **Auditoria** | ❌ Sem histórico | ✅ Completo |
| **Revogação** | ⚠️ Temporária | ✅ Persistente |
| **Limpeza** | ❌ Manual | ✅ Automática |

#### 📊 Índices Criados

```sql
CREATE INDEX IDX_REFRESH_TOKEN_USUARIO ON REFRESH_TOKEN (USUARIO_ID);
CREATE INDEX IDX_REFRESH_TOKEN_TOKEN ON REFRESH_TOKEN (TOKEN);
CREATE INDEX IDX_REFRESH_TOKEN_EXPIRACAO ON REFRESH_TOKEN (DATA_EXPIRACAO);
CREATE INDEX IDX_REFRESH_TOKEN_REVOGADO ON REFRESH_TOKEN (REVOGADO);
```

#### 🔄 Limpeza Automática

**Processo:**

1. Tokens expirados são marcados como `REVOGADO='1'`
2. Tokens revogados há mais de 30 dias são deletados
3. Executado via `CleanupExpiredTokens()`

**Query de limpeza:**
```sql
-- Revogar expirados
UPDATE REFRESH_TOKEN
SET REVOGADO = '1',
    MOTIVO_REVOGACAO = 'Expirado automaticamente'
WHERE DATA_EXPIRACAO < CURRENT_TIMESTAMP
  AND REVOGADO = '0';

-- Deletar antigos
DELETE FROM REFRESH_TOKEN
WHERE REVOGADO = '1'
  AND DATA_REVOGACAO < DATEADD(-30 DAY TO CURRENT_TIMESTAMP);
```

---

## 📋 Instruções de Uso

### 1. Setup Inicial

#### A. Instalar Dependências
```powershell
cd C:\VS_Code\icomanda\IComanda.API
dotnet restore
```

#### B. Criar Tabela REFRESH_TOKEN
```powershell
# Opção 1: Script automatizado (recomendado)
.\executar-sql-refresh-token.ps1

# Opção 2: Manual (ISQL)
isql -user SYSDBA -password masterkey "C:\iComanda\Dados\DADOSG5.FDB"
SQL> INPUT 'criar-tabela-refresh-token.sql';
SQL> SHOW TABLE REFRESH_TOKEN;
SQL> EXIT;
```

#### C. Compilar Aplicação
```powershell
dotnet build -c Release
```

### 2. Migração de Senhas

#### Opção A: Deixar Migração Automática (Recomendado)
- Usuários migram senhas no primeiro login
- Sem interrupção de serviço
- Logs detalhados

#### Opção B: Migração em Lote
```powershell
# 1. BACKUP DO BANCO (OBRIGATÓRIO!)
Copy-Item "C:\iComanda\Dados\DADOSG5.FDB" "C:\iComanda\Dados\BACKUP_$(Get-Date -Format 'yyyyMMdd_HHmmss').FDB"

# 2. Dry-run (testar sem modificar)
.\migrar-senhas-bcrypt.ps1 -DryRun -Verbose

# 3. Executar migração real
.\migrar-senhas-bcrypt.ps1 -Verbose

# 4. Testar login de todos os usuários
```

### 3. Testar Implementações

#### A. Testar Password Hashing
```powershell
# Login com senha plaintext (primeira vez)
curl -X POST http://localhost:65375/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"123456"}'

# Verificar logs - deve mostrar migração
# [INFO] Senha em plaintext detectada para usuário X
# [INFO] ✅ Senha migrada para BCrypt hash

# Login novamente (senha já em hash)
curl -X POST http://localhost:65375/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{"username":"admin","password":"123456"}'

# Logs devem mostrar validação BCrypt
# [DEBUG] Password hashed successfully
```

#### B. Testar Refresh Token Persistente
```powershell
# 1. Fazer login (gerar tokens)
$login = Invoke-RestMethod -Uri "http://localhost:65375/api/auth/login" `
  -Method POST `
  -Body '{"username":"admin","password":"123456"}' `
  -ContentType "application/json"

Write-Host "Token: $($login.token)"
Write-Host "RefreshToken: $($login.refreshToken)"

# 2. Verificar no banco
isql -user SYSDBA -password masterkey "C:\iComanda\Dados\DADOSG5.FDB"
SQL> SELECT ID, USUARIO_ID, USUARIO_NOME, DATA_CRIACAO, DATA_EXPIRACAO, REVOGADO FROM REFRESH_TOKEN;
SQL> EXIT;

# 3. Renovar token (refresh)
$refresh = Invoke-RestMethod -Uri "http://localhost:65375/api/auth/refresh" `
  -Method POST `
  -Body "{`"token`":`"$($login.token)`",`"refreshToken`":`"$($login.refreshToken)`"}" `
  -ContentType "application/json"

Write-Host "Novo Token: $($refresh.token)"
Write-Host "Novo RefreshToken: $($refresh.refreshToken)"

# 4. Verificar revogação do token antigo
SQL> SELECT ID, TOKEN, REVOGADO, MOTIVO_REVOGACAO FROM REFRESH_TOKEN ORDER BY ID DESC;
# Token antigo deve estar REVOGADO='1' com motivo "Token usado para refresh"

# 5. Revogar token (logout)
$headers = @{ "Authorization" = "Bearer $($refresh.token)" }
Invoke-RestMethod -Uri "http://localhost:65375/api/auth/revoke" `
  -Method POST `
  -Headers $headers `
  -Body "{`"refreshToken`":`"$($refresh.refreshToken)`"}" `
  -ContentType "application/json"

# 6. Verificar revogação
SQL> SELECT TOKEN, REVOGADO, MOTIVO_REVOGACAO FROM REFRESH_TOKEN WHERE TOKEN = '<refresh_token>';
# Deve mostrar REVOGADO='1'
```

---

## 🎯 Próximos Passos (Fase 2 - Restante)

### 3. FluentValidation nos Endpoints (3-4 dias)
- [ ] Validar DTOs de entrada (LoginRequest, VendaRequest, etc.)
- [ ] Mensagens de erro padronizadas
- [ ] Validação de CPF, telefone, email
- [ ] Regras de negócio (quantidade > 0, preço > 0)

### 4. Testes Unitários Básicos (5-7 dias)
- [ ] xUnit + Moq
- [ ] Testes de PasswordHasher
- [ ] Testes de RefreshTokenService
- [ ] Testes de AuthController
- [ ] Coverage mínimo: 70%

### 5. Otimizar Queries N+1 (2-3 dias)
- [ ] Identificar queries N+1 com profiler
- [ ] Implementar JOINs otimizados
- [ ] Usar SELECT específico (evitar SELECT *)
- [ ] Índices adicionais se necessário

### 6. Centralizar Logging Backend (2 dias)
- [ ] Serilog já instalado - configurar sinks
- [ ] Structured logging (JSON)
- [ ] Diferentes níveis por ambiente
- [ ] Integração com Seq ou ELK (opcional)

---

## 📊 Métricas de Melhoria - Fase 2

| Item | Antes | Depois | Ganho |
|------|-------|--------|-------|
| **Segurança Senha** | ❌ Plaintext | ✅ BCrypt | 🔒 100% |
| **Persistência Token** | ⚠️ Memória | ✅ Database | ♾️ Infinito |
| **Work Factor** | - | 12 (4096 iter) | 🛡️ GPU-proof |
| **Salt** | - | ✅ Aleatório | 🎲 Rainbow-proof |
| **Auditoria** | - | ✅ Completa | 📋 Full |
| **Escalabilidade** | ❌ 1 instância | ✅ Multi-inst | ⚖️ Horizontal |

---

## ⚠️ Avisos Importantes

### 1. Backup Antes de Migrar Senhas
```powershell
Copy-Item "C:\iComanda\Dados\DADOSG5.FDB" "C:\iComanda\Dados\BACKUP_ANTES_BCRYPT.FDB"
```

### 2. Testar Login de Todos os Usuários
Após migração, testar login de cada usuário para garantir que hash foi gerado corretamente.

### 3. Tabela REFRESH_TOKEN Obrigatória
Aplicação NÃO INICIA sem a tabela criada. Execute `executar-sql-refresh-token.ps1` ANTES de rodar.

### 4. Senhas Antigas Não Funcionam Após Hash
Se gerar hash manualmente (fora da migração automática), usuários não conseguem mais logar com senha antiga.

### 5. Performance do BCrypt
- Work Factor 12 = ~0.3s por hash
- Login pode parecer "lento" comparado a plaintext
- **É intencional**: Proteção contra brute force

---

## 🔍 Troubleshooting

### Problema: "Forbidden" ao fazer login
**Causa:** Senha migrada incorretamente ou hash inválido  
**Solução:**
```sql
-- Resetar senha do admin para plaintext (emergência)
UPDATE USUARIO SET SENHA = '123456' WHERE ID = 1;
COMMIT;
```

### Problema: "RefreshToken inválido" sempre
**Causa:** Tabela REFRESH_TOKEN não existe  
**Solução:**
```powershell
.\executar-sql-refresh-token.ps1
```

### Problema: Build falha com erro BCrypt
**Causa:** Package não restaurado  
**Solução:**
```powershell
dotnet restore
dotnet build -c Release
```

### Problema: Tokens perdidos ao reiniciar
**Causa:** Ainda usando RefreshTokenService em memória  
**Solução:**
```csharp
// Verificar ServiceCollectionExtensions.cs
services.AddScoped<IRefreshTokenService, RefreshTokenDatabaseService>(); // ✅ Correto
// services.AddSingleton<IRefreshTokenService, RefreshTokenService>(); // ❌ Errado (memória)
```

---

## 📚 Referências

- [BCrypt.Net-Next Documentation](https://github.com/BcryptNet/bcrypt.net)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [JWT Best Practices](https://datatracker.ietf.org/doc/html/rfc8725)
- [Firebird 2.5 Language Reference](https://firebirdsql.org/file/documentation/html/en/refdocs/fblangref25/firebird-25-language-reference.html)

---

## ✅ Checklist de Implementação Concluída

- [x] Adicionar BCrypt.Net-Next package
- [x] Criar IPasswordHasher interface
- [x] Implementar PasswordHasher service
- [x] Atualizar AuthController para usar BCrypt
- [x] Adicionar migração automática de senhas
- [x] Criar script de migração em lote
- [x] Criar tabela REFRESH_TOKEN SQL
- [x] Criar RefreshTokenEntity model
- [x] Criar IRefreshTokenRepository interface
- [x] Implementar RefreshTokenRepository
- [x] Criar RefreshTokenDatabaseService
- [x] Separar IRefreshTokenService interface
- [x] Registrar services no DI container
- [x] Criar script PowerShell para setup SQL
- [x] Documentar no README-PHASE2.md
- [x] Compilar sem erros (0 errors)

---

## 🎉 Status: FASE 2 - PARCIALMENTE CONCLUÍDA

**Concluído (33%):** Password Hashing + Refresh Token Persistente  
**Pendente (67%):** FluentValidation + Testes + Otimização Queries + Logging

**Estimativa para conclusão total da Fase 2:** 2-3 semanas

---

*Documento gerado em 15/fevereiro/2026*  
*iComanda - Sistema de Gestão de Comandas*
