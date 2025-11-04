# ⚙️ Configurações do Sistema de Login

## 🔐 Configurações JWT (appsettings.json)

```json
"Jwt": {
  "Key": "icomanda-super-secret-key-2024-change-in-production-minimum-32-characters",
  "Issuer": "IComanda.API",
  "Audience": "IComanda.Client",
  "ExpirationHours": 8
}
```

### ⚠️ IMPORTANTE para Produção:

1. **Alterar a chave JWT** - Use uma chave forte e única
2. **Usar variáveis de ambiente** - Não commitar senhas no código
3. **HTTPS obrigatório** - Sempre use SSL/TLS em produção
4. **Implementar hash de senha** - O sistema legado usa texto plano

## 📦 Pacotes NuGet Adicionados

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
```

## 📦 Pacotes NPM Adicionados

```bash
npm install react-router-dom@6
```

## 🗂️ Estrutura de Arquivos Criados

### Backend

```
IComanda.API/
├── Models/
│   ├── Entities/
│   │   └── Usuario.cs
│   └── DTOs/
│       ├── LoginRequest.cs
│       ├── LoginResponse.cs
│       └── UsuarioDto.cs
├── Repositories/
│   ├── Interfaces/
│   │   └── IUsuarioRepository.cs
│   └── Implementations/
│       └── UsuarioRepository.cs
├── Services/
│   ├── Interfaces/
│   │   └── IAuthService.cs
│   └── Implementations/
│       └── AuthService.cs
├── Controllers/
│   └── AuthController.cs
└── Scripts/
    └── criar_usuario_teste.sql
```

### Frontend

```
icomanda-frontend/src/
├── types/
│   └── auth.ts
├── services/
│   └── authService.ts
├── store/
│   └── authStore.ts
├── pages/
│   ├── Login.tsx
│   └── Home.tsx
└── components/
    ├── ProtectedRoute.tsx
    └── ui/
        └── AppHeader.tsx (atualizado)
```

## 🔄 Fluxo de Autenticação

```
1. Usuário acessa http://localhost:3000
   └─> Sem autenticação? → Redireciona para /login

2. Usuário digita credenciais
   └─> Frontend → POST /api/auth/login
       └─> Backend valida no Firebird
           ├─> Válido? → Gera JWT Token
           └─> Inválido? → Retorna 401

3. Frontend recebe token
   └─> Salva no localStorage
   └─> Configura header Authorization
   └─> Redireciona para /

4. Usuário navega no sistema
   └─> Todas as requisições incluem: Authorization: Bearer {token}

5. Token expira após 8h
   └─> Próxima requisição → 401
   └─> Redirect para /login

6. Usuário clica em Logout
   └─> Remove token do localStorage
   └─> Redirect para /login
```

## 🎨 Customizações Possíveis

### Alterar tempo de expiração do token:

**appsettings.json:**

```json
"ExpirationHours": 24  // Token válido por 24 horas
```

### Adicionar mais campos ao token:

**AuthService.cs → GerarTokenJwt:**

```csharp
claims.Add(new Claim("email", usuario.Email));
claims.Add(new Claim("departamento", usuario.Departamento));
```

### Customizar a página de login:

**Login.tsx:** Edite cores, logo, textos, etc.

### Adicionar "Lembrar-me":

Altere `authService.ts` para usar `localStorage` ou `sessionStorage`

## 🔍 Validações Implementadas

### Backend:

- ✅ Usuário existe?
- ✅ Usuário está ativo? (ATIVO = '1')
- ✅ Usuário não está bloqueado? (BLOQUEIO != '1')
- ✅ Senha correta? (comparação texto plano)
- ✅ Token válido?
- ✅ Token não expirado?

### Frontend:

- ✅ Campos preenchidos?
- ✅ Token presente no localStorage?
- ✅ Rotas protegidas?
- ✅ Redirect automático para login se não autenticado

## 🚀 Melhorias Futuras Sugeridas

1. **Hash de Senhas**

   - Implementar BCrypt ou PBKDF2
   - Migrar senhas existentes

2. **Refresh Token**

   - Implementar refresh token para renovação automática
   - Evitar logout forçado a cada X horas

3. **2FA (Two-Factor Authentication)**

   - Adicionar autenticação de dois fatores
   - SMS ou Authenticator App

4. **Auditoria**

   - Log de tentativas de login
   - Histórico de acessos

5. **Recuperação de Senha**

   - Fluxo de reset de senha por email
   - Token temporário de recuperação

6. **Permissões Granulares**

   - Sistema de roles e permissions
   - Controle de acesso por funcionalidade

7. **HttpOnly Cookies**
   - Usar cookies ao invés de localStorage
   - Maior segurança contra XSS

## 📊 Campos da Tabela USUARIO

| Campo      | Tipo        | Descrição           | Valores              |
| ---------- | ----------- | ------------------- | -------------------- |
| ID         | INTEGER     | Identificador único | AUTO (via generator) |
| NOME       | VARCHAR(40) | Nome de usuário     | Qualquer texto       |
| SENHA      | VARCHAR(10) | Senha (texto plano) | Qualquer texto       |
| ATIVO      | CHAR(1)     | Usuário ativo?      | '1' = Sim, '0' = Não |
| BLOQUEIO   | CHAR(1)     | Usuário bloqueado?  | '1' = Sim, '0' = Não |
| VISUALIZAR | CHAR(1)     | Pode visualizar     | '1' = Sim, '0' = Não |
| TOTAL      | CHAR(1)     | Pode ver total      | '1' = Sim, '0' = Não |
| TIPO       | CHAR(1)     | Tipo de usuário     | '0' = Padrão, ...    |
| CANCELAR   | CHAR(1)     | Pode cancelar       | '1' = Sim, '0' = Não |

## 🔗 URLs Importantes

- **API:** http://localhost:65375
- **Swagger:** http://localhost:65375
- **Frontend:** http://localhost:3000
- **Login:** http://localhost:3000/login
- **Endpoint Login:** http://localhost:65375/api/auth/login
- **Endpoint Validate:** http://localhost:65375/api/auth/validate

## 💡 Dicas

1. **Desenvolvimento:** Use o Swagger para testar os endpoints
2. **Debug:** Abra o DevTools (F12) e veja a aba Network
3. **Token:** Copie o token do localStorage no DevTools → Application → Local Storage
4. **Erros:** Veja os logs no console do backend (Serilog)
