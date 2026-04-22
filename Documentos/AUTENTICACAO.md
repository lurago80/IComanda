# Sistema de Autenticação - iComanda

## ✅ Implementação Completa

O sistema de autenticação JWT foi implementado com sucesso tanto no backend quanto no frontend.

## 🔧 Backend (.NET 8)

### Componentes Criados

1. **Models**

   - `Usuario.cs` - Entidade mapeando a tabela USUARIO do Firebird
   - `LoginRequest.cs` - DTO para request de login
   - `LoginResponse.cs` - DTO para response de login
   - `UsuarioDto.cs` - DTO com informações do usuário

2. **Repository**

   - `IUsuarioRepository` - Interface do repositório
   - `UsuarioRepository` - Implementação com Dapper

3. **Service**

   - `IAuthService` - Interface do serviço de autenticação
   - `AuthService` - Implementação com JWT

4. **Controller**
   - `AuthController` - Endpoints `/api/auth/login` e `/api/auth/validate`

### Configurações

- **JWT** configurado no `appsettings.json`
- **Authentication** e **Authorization** configurados no `Program.cs`
- **Swagger** com suporte a Bearer Token

## 🎨 Frontend (React + TypeScript)

### Componentes Criados

1. **Types**

   - `auth.ts` - Tipos TypeScript para autenticação

2. **Services**

   - `authService.ts` - Serviço de comunicação com a API

3. **Store (Zustand)**

   - `authStore.ts` - Gerenciamento de estado de autenticação

4. **Pages**

   - `Login.tsx` - Página de login moderna e responsiva
   - `Home.tsx` - Página principal com conteúdo protegido

5. **Components**
   - `ProtectedRoute.tsx` - HOC para proteção de rotas
   - `AppHeader.tsx` - Atualizado com informações do usuário e logout

## 🔐 Endpoints da API

### POST /api/auth/login

Realiza o login do usuário.

**Request:**

```json
{
  "nome": "usuario",
  "senha": "senha123"
}
```

**Response (200 OK):**

```json
{
  "id": 1,
  "nome": "usuario",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "tipo": "0",
  "podeVisualizar": true,
  "podeVerTotal": true,
  "podeCancelar": false
}
```

**Response (401 Unauthorized):**

```json
{
  "message": "Usuário ou senha inválidos"
}
```

### GET /api/auth/validate

Valida o token JWT atual (requer autenticação).

**Headers:**

```
Authorization: Bearer {token}
```

**Response (200 OK):**

```json
{
  "id": 1,
  "nome": "usuario",
  "ativo": true,
  "bloqueado": false,
  "tipo": "0",
  "podeVisualizar": true,
  "podeVerTotal": true,
  "podeCancelar": false
}
```

## 📊 Estrutura da Tabela USUARIO (Firebird 2.5)

```sql
CREATE TABLE USUARIO (
    ID          INTEGER NOT NULL,
    NOME        VARCHAR(40),
    SENHA       VARCHAR(10),
    ATIVO       CHAR(1),           -- '1' = Ativo
    BLOQUEIO    CHAR(1) DEFAULT '0', -- '1' = Bloqueado
    VISUALIZAR  CHAR(1) DEFAULT '0', -- '1' = Pode visualizar
    TOTAL       CHAR(1) DEFAULT '1', -- '1' = Pode ver total
    TIPO        CHAR(1) DEFAULT '0', -- Tipo de usuário
    CANCELAR    CHAR(1)            -- '1' = Pode cancelar
);
```

## 🚀 Como Testar

### 1. Criar um usuário de teste no Firebird:

```sql
INSERT INTO USUARIO (ID, NOME, SENHA, ATIVO, BLOQUEIO, VISUALIZAR, TOTAL, TIPO, CANCELAR)
VALUES (1, 'admin', '123456', '1', '0', '1', '1', '0', '1');
```

### 2. Iniciar a API:

```bash
cd IComanda.API
dotnet run
```

A API estará disponível em: http://localhost:65375

### 3. Iniciar o Frontend:

```bash
cd IComanda.API/icomanda-frontend
npm start
```

O frontend estará disponível em: http://localhost:3000

### 4. Fazer Login:

- Usuário: `admin`
- Senha: `123456`

## 🔒 Segurança

- **Token JWT** com validade de 8 horas
- **Senha** atualmente em texto plano (sistema legado - considerar hash para produção)
- **HTTPS** recomendado para produção
- **Token** armazenado no localStorage (considerar httpOnly cookies para maior segurança)
- **CORS** configurado para domínios específicos

## 🎯 Funcionalidades

- ✅ Login com usuário e senha
- ✅ Geração de token JWT
- ✅ Validação de token
- ✅ Proteção de rotas no frontend
- ✅ Logout
- ✅ Persistência de sessão (localStorage)
- ✅ Verificação de usuário ativo/bloqueado
- ✅ Exibição de nome do usuário no header
- ✅ Botão de logout visível

## 🛠️ Tecnologias Utilizadas

### Backend

- .NET 8
- ASP.NET Core Web API
- JWT Bearer Authentication
- Dapper (ORM)
- Firebird 2.5
- Serilog

### Frontend

- React 19
- TypeScript
- React Router DOM v6
- Zustand (State Management)
- Axios
- TailwindCSS
- Lucide React (Icons)

## 📝 Observações

- O sistema valida se o usuário está **ativo** (ATIVO = '1')
- O sistema verifica se o usuário **não está bloqueado** (BLOQUEIO != '1')
- As permissões do usuário são incluídas no token JWT
- O token expira automaticamente após 8 horas
