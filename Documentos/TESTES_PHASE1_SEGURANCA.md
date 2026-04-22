# 🧪 Testes da Phase 1 - Segurança

## Iniciando a API

```bash
cd c:\VS_Code\icomanda\IComanda.API
dotnet run
# Aguarde: "Server iniciado em http://localhost:65375"
```

---

## 🔐 Teste 1: Login (Sem Token)

### Request
```bash
curl -X POST http://localhost:65375/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "administrador",
    "password": "masterkey"
  }'
```

### Expected Response (200 OK)
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

**Salve o token para os próximos testes:**
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## ✅ Teste 2: Validar Token

### Request
```bash
curl -X POST http://localhost:65375/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{"token":"'$TOKEN'"}'
```

### Expected Response (200 OK)
```json
{
  "valid": true,
  "userId": 1,
  "username": "administrador",
  "role": "Admin"
}
```

---

## 📋 Teste 3: Acessar Endpoint Protegido COM Token

### Request
```bash
curl -X GET http://localhost:65375/api/vendas/abertas \
  -H "Authorization: Bearer $TOKEN"
```

### Expected Response (200 OK)
```json
[
  {
    "nota": "000001",
    "data": "2024-01-15",
    "cliente": "Balcão",
    "valor": 150.00
  }
]
```

---

## ❌ Teste 4: Acessar Endpoint Protegido SEM Token

### Request
```bash
curl -X GET http://localhost:65375/api/vendas/abertas
```

### Expected Response (401 Unauthorized)
```json
{
  "error": "Unauthorized",
  "message": "Token inválido ou expirado. Por favor, faça login novamente."
}
```

---

## 🚫 Teste 5: Token Inválido

### Request
```bash
curl -X GET http://localhost:65375/api/vendas/abertas \
  -H "Authorization: Bearer invalid.token.here"
```

### Expected Response (401 Unauthorized)
```json
{
  "error": "Unauthorized",
  "message": "Token inválido ou expirado. Por favor, faça login novamente."
}
```

---

## 🛡️ Teste 6: Rate Limiting

### Script para Testing Rate Limit
```bash
#!/bin/bash
echo "Testando rate limiting (100 req/min)..."

TOKEN="<seu-token-aqui>"
COUNT=0

for i in {1..105}; do
  RESPONSE=$(curl -s -w "\n%{http_code}" \
    -H "Authorization: Bearer $TOKEN" \
    http://localhost:65375/api/vendas/abertas)
  
  HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
  
  COUNT=$((COUNT + 1))
  
  if [ "$HTTP_CODE" = "429" ]; then
    echo "✅ Rate limit ativado na requisição $COUNT (HTTP $HTTP_CODE)"
    break
  elif [ "$COUNT" -eq 105 ]; then
    echo "⚠️ Rate limit NÃO ativado após $COUNT requisições"
  fi
done
```

### Expected Result
- Requisições 1-100: `200 OK`
- Requisição 101+: `429 Too Many Requests`

---

## 👤 Teste 7: Login com Credenciais Inválidas

### Request
```bash
curl -X POST http://localhost:65375/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "usuario_inexistente",
    "password": "senha_errada"
  }'
```

### Expected Response (401 Unauthorized)
```json
{
  "error": "Usuário ou senha inválidos"
}
```

---

## 🔄 Teste 8: Swagger com Autenticação

1. Abra http://localhost:65375/swagger
2. Clique em "Authorize" (cadeado no canto superior direito)
3. Cole o token no formato: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
4. Clique em "Authorize"
5. Teste algum endpoint como POST /api/vendas

---

## 📊 Teste 9: Verificar Claims do Token

### Decodificar Token (use https://jwt.io)

Cole o token para ver os claims:

```json
{
  "iss": "IComanda.API",
  "aud": "IComanda.Client",
  "iat": 1702123456,
  "exp": 1702159456,
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "administrador",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
  "UserId": "1",
  "Role": "Admin",
  "RoleValue": "1",
  "Permission": [
    "CreateVendas",
    "EditVendas",
    "CancelVendas",
    "AccessRecebimentos",
    "CloseSales",
    "AccessReports",
    "ManageUsers",
    "AccessDelivery",
    "AdminSettings",
    "ViewAudit"
  ]
}
```

---

## 🧪 Teste 10: Testar Diferentes Roles

### Criar usuários de teste (SQL)
```sql
-- Admin (CANCELAR=1, VISUALIZAR=1)
INSERT INTO USUARIO (ID, NOME, SENHA, ATIVO, CANCELAR, VISUALIZAR) 
VALUES (2, 'gerente', 'senha123', '1', '1', '1');

-- Caixa (CANCELAR=0, VISUALIZAR=1)
INSERT INTO USUARIO (ID, NOME, SENHA, ATIVO, CANCELAR, VISUALIZAR) 
VALUES (3, 'caixa', 'senha123', '1', '0', '1');

-- Garçom (CANCELAR=0, VISUALIZAR=0)
INSERT INTO USUARIO (ID, NOME, SENHA, ATIVO, CANCELAR, VISUALIZAR) 
VALUES (4, 'garcom', 'senha123', '1', '0', '0');
```

### Teste de Login com Caixa
```bash
curl -X POST http://localhost:65375/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"caixa","password":"senha123"}'
```

### Response esperado
```json
{
  "role": "Caixa",  // Note: agora é Caixa, não Admin
  "token": "..."
}
```

---

## 📈 Monitoramento Durante Testes

### Verificar Logs da API
```bash
# Terminal onde a API está rodando
# Deve mostrar:
# - [INF] Usuário autenticado com sucesso: 1
# - [WRN] Rate limit excedido para USER_1: 101 req/min
# - [WRN] Usuário não encontrado: usuario_inexistente
```

---

## ⚠️ Possíveis Problemas

### Problema 1: "Jwt:Key não está configurada"
**Solução:** Verificar `appsettings.json`:
```json
{
  "Jwt": {
    "Key": "icomanda-super-secret-key-2024-change-in-production-minimum-32-characters",
    "Issuer": "IComanda.API",
    "Audience": "IComanda.Client",
    "ExpirationHours": 8
  }
}
```

### Problema 2: "Usuário ou senha inválidos"
**Solução:** Verificar tabela USUARIO no Firebird:
```sql
SELECT ID, NOME, ATIVO, BLOQUEIO FROM USUARIO;
-- Garantir que NOME='administrador' existe e ATIVO='1'
```

### Problema 3: Token expira muito rápido
**Solução:** Modificar `appsettings.json`:
```json
"ExpirationHours": 24  // Aumentar de 8 para 24
```

### Problema 4: 429 Too Many Requests muito cedo
**Solução:** Taxa é: 100 req/minuto. Para testes, pode aumentar em `RateLimitingMiddleware.cs`:
```csharp
private const int MaxRequestsPerMinute = 1000; // Em desenvolvimento
```

---

## 📋 Checklist de Testes

- [ ] Teste 1: Login com credenciais válidas ✅
- [ ] Teste 2: Validar token ✅
- [ ] Teste 3: Acessar endpoint protegido com token ✅
- [ ] Teste 4: Acessar endpoint protegido sem token ✅
- [ ] Teste 5: Token inválido ✅
- [ ] Teste 6: Rate limiting ✅
- [ ] Teste 7: Credenciais inválidas ✅
- [ ] Teste 8: Swagger com autenticação ✅
- [ ] Teste 9: Verificar claims do token ✅
- [ ] Teste 10: Diferentes roles ✅

---

## 🚀 Próximas Ações

1. **Passar estes testes** em ambiente de staging
2. **Implementar password hashing** antes de produção
3. **Configurar HTTPS** e remover comentário de `UseHttpsRedirection()`
4. **Audit logging** para operações sensíveis
5. **Monitoramento** de tentativas de login falhadas

---

## 📚 Referências

- [GUIA_AUTENTICACAO_JWT.md](GUIA_AUTENTICACAO_JWT.md) - Guia completo
- [IMPLEMENTACAO_PHASE1_SEGURANCA.md](IMPLEMENTACAO_PHASE1_SEGURANCA.md) - Detalhes de implementação
- [Program.cs](Program.cs) - Configuração de serviços
- [AuthController.cs](Controllers/AuthController.cs) - Endpoints de autenticação

