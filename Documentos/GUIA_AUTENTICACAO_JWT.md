# 🔐 Guia de Autenticação JWT - Phase 1 (Segurança)

## Status de Implementação - Phase 1 (Urgente)

✅ **CONCLUÍDO:**
1. ✅ Reabilitado JWT com roles/permissions
2. ✅ Implementado rate limiting middleware 
3. ✅ Melhorado CORS policy (já estava configurado)
4. ⏳ Em progresso: Adicionar [Authorize] nos controllers

---

## 📋 Resumo das Implementações

### 1. JWT Token Provider Service (`JwtTokenProvider.cs`)
- Gera tokens JWT com claims de role e permissões
- Valida tokens e extrai informações de usuário
- Suporta 8 horas de expiração (configurável)

### 2. Roles e Permissões (`UserRole.cs`)
Papéis disponíveis:
- **Admin**: Acesso total ao sistema
- **Gerente**: Acesso a relatórios, fechamento de caixa, cancelamento de vendas
- **Garçom**: Criar vendas e comandas
- **Caixa**: Recebimentos e fechamento de vendas
- **Entregador**: Acesso apenas a pedidos de delivery

Permissões granulares:
- CreateVendas, EditVendas, CancelVendas
- AccessRecebimentos, CloseSales
- AccessReports, ViewAudit
- ManageUsers, AdminSettings, AccessDelivery

### 3. Rate Limiting Middleware (`RateLimitingMiddleware.cs`)
- Limite: 100 requisições/minuto por IP/usuário
- Limite adicional: 1000 requisições/hora para usuários autenticados
- Armazenamento em memória
- Limpeza automática a cada 60 segundos

### 4. Auth Controller (`AuthController.cs`)
Endpoints disponíveis:
- `POST /api/auth/login` - Realiza login e retorna token JWT
- `POST /api/auth/validate` - Valida um token existente

### 5. Authorization Extensions (`AuthorizationExtensions.cs`)
- Atributos customizados: `[AuthorizeRoles]`, `[AuthorizePermission]`
- Policies: AdminOnly, GerenteOrAdmin, CaixaOrAbove, DeliveryAccess

---

## 🔑 Como Usar

### 1. Realizar Login

**Request:**
```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "nome_usuario",
  "password": "senha"
}
```

**Response (Sucesso):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": 1,
  "username": "administrador",
  "role": "Admin",
  "expiresIn": 8,
  "tokenType": "Bearer"
}
```

### 2. Usar Token em Requisições

Adicione o token no header `Authorization`:

```bash
GET /api/vendas/abertas
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Validar Token

```bash
POST /api/auth/validate
Content-Type: application/json

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## 🛡️ Protegendo Endpoints

### Exemplo: VendasController

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requer autenticação
public class VendasController : ControllerBase
{
    // Qualquer usuário autenticado pode acessar
    [HttpGet("abertas")]
    public async Task<IActionResult> GetVendasAbertas() { }

    // Apenas Admin e Gerente
    [HttpPost("delivery")]
    [Authorize(Roles = "Admin,Gerente,Caixa")]
    public async Task<IActionResult> CreateVendaDelivery() { }

    // Apenas Admin
    [HttpDelete("{nota}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteVenda(string nota) { }

    // Sem autenticação (endpoint público)
    [HttpGet("parametros")]
    [AllowAnonymous]
    public async Task<IActionResult> GetParametros() { }
}
```

### Usar Policies

```csharp
[HttpPost("confirmar")]
[Authorize(Policy = "CaixaOrAbove")] // Admin, Gerente ou Caixa
public async Task<IActionResult> ConfirmarVenda() { }
```

---

## 📊 Mapeamento de Permissões por Role

| Role | CreateVendas | EditVendas | CancelVendas | AccessRecibimentos | CloseSales | AccessReports | ManageUsers |
|------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Admin | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Gerente | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |
| Caixa | ✅ | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| Garçom | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Entregador | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

---

## 🚨 Taxa de Limite (Rate Limiting)

### Comportamento
- Se exceder 100 req/min: Retorna `429 Too Many Requests`
- Pode ser por IP ou por UserId (se autenticado)
- Tentativa de ataque DDoS é bloqueada automaticamente

### Response de Rate Limit Excedido
```json
{
  "error": "Too Many Requests",
  "message": "Limite de requisições excedido. Máximo de 100 requisições por minuto.",
  "retryAfter": 60
}
```

---

## 🔒 Autenticação no Firebird

### Mapeamento de Usuários

A tabela `USUARIO` possui os campos:
- `ID`: Identificador único
- `NOME`: Nome de usuário
- `SENHA`: Senha em texto plano
- `ATIVO`: '1' = ativo
- `BLOQUEIO`: '1' = bloqueado
- `CANCELAR`: '1' = pode cancelar (Gerente/Admin)
- `VISUALIZAR`: '1' = pode visualizar/caixa

### Lógica de Determinação de Role

```
Se CANCELAR='1' e alguém está bloqueado → Gerente (pode cancelar vendas)
Se VISUALIZAR='1' → Caixa (acesso a recibimentos)
Senão → Garçom (padrão)
```

---

## ⚠️ Importante: Segurança em Produção

### TODO: Implementar Hashing de Senha
Atualmente, as senhas são comparadas em texto plano! Em produção:

```csharp
// ❌ NUNCA fazer em produção:
if (usuario.Senha != request.Password) { }

// ✅ Usar bcrypt ou Argon2:
var passwordHasher = new PasswordHasher<Usuario>();
var result = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Password);
```

### TODO: Usar Variáveis de Ambiente
Em produção, o JWT:Key deve vir de variável de ambiente:

```csharp
// ❌ NUNCA hardcode a chave:
"Jwt:Key": "icomanda-super-secret-key-2024..."

// ✅ Usar variável de ambiente:
dotnet run --Jwt:Key="chave-segura-minimo-32-caracteres"
```

---

## 📝 Próximas Etapas

1. **Adicionar [Authorize] a todos os controllers** (em progresso)
   - VendasController
   - ClientesController
   - RecebimentosController
   - RelatoriosController
   - E todos os 15+ controllers

2. **Implementar hashing de senha** (urgente em produção)
   - Usar BCrypt ou Argon2
   - Rehashing de senhas legadas

3. **Audit logging** (recomendado)
   - Registrar quem fez login
   - Registrar ações sensíveis
   - Rastrear acesso a dados

4. **Refresh tokens** (para sessões longas)
   - Implementar refresh token de 7 dias
   - Validação adicional

5. **Two-Factor Authentication** (para admin)
   - SMS ou TOTP
   - Backup codes

---

## 📚 Referências

- [Microsoft JWT Documentation](https://docs.microsoft.com/dotnet/api/system.identitymodel.tokens.jwt)
- [ASP.NET Core Authorization](https://docs.microsoft.com/aspnet/core/security/authorization)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)

---

## ✅ Checklist de Implementação

- [x] JWT baseado em roles implementado
- [x] Rate limiting aplicado
- [x] AuthController criado
- [x] Permissões granulares definidas
- [ ] Adicionar [Authorize] aos controllers
- [ ] Implementar password hashing
- [ ] Audit logging
- [ ] Refresh tokens
- [ ] Two-Factor Auth (opcional)
- [ ] Testes de segurança
