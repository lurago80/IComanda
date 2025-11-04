# 🚀 Início Rápido - Sistema de Login iComanda

## 📋 Pré-requisitos

- ✅ .NET 8 SDK instalado
- ✅ Node.js instalado
- ✅ Firebird 2.5 rodando
- ✅ Banco de dados DADOSG5.FDB configurado

## 🔧 Passo 1: Criar Usuário de Teste

Execute o script SQL no Firebird:

```sql
INSERT INTO USUARIO (
    ID,
    NOME,
    SENHA,
    ATIVO,
    BLOQUEIO,
    VISUALIZAR,
    TOTAL,
    TIPO,
    CANCELAR
) VALUES (
    GEN_ID(USUARIO_ID_GEN, 1),
    'admin',
    '123456',
    '1',
    '0',
    '1',
    '1',
    '0',
    '1'
);
COMMIT;
```

**Ou use o script pronto:** `Scripts/criar_usuario_teste.sql`

## 🎯 Passo 2: Iniciar o Backend

```bash
cd IComanda.API
dotnet run
```

✅ Backend disponível em: **http://localhost:65375**
✅ Swagger disponível em: **http://localhost:65375**

## 💻 Passo 3: Iniciar o Frontend

```bash
cd IComanda.API/icomanda-frontend
npm start
```

✅ Frontend disponível em: **http://localhost:3000**

## 🔐 Passo 4: Fazer Login

1. Abra o navegador em **http://localhost:3000**
2. Você será redirecionado para a tela de login
3. Digite as credenciais:
   - **Usuário:** `admin`
   - **Senha:** `123456`
4. Clique em **Entrar**

## ✨ Funcionalidades Disponíveis

Após o login, você terá acesso a:

- 🛒 Sistema completo de pedidos
- 📦 Catálogo de produtos por grupos
- 🔍 Busca de produtos
- 🛍️ Carrinho de compras
- 📊 Histórico de vendas
- 👤 Informações do usuário logado no header
- 🚪 Botão de logout (ícone vermelho no canto superior direito)

## 🔒 Segurança

- ✅ Token JWT com 8 horas de validade
- ✅ Rotas protegidas no frontend
- ✅ Validação de usuário ativo e não bloqueado
- ✅ Sessão persistente (mantém login após refresh)

## 🧪 Testar API Diretamente

### Usando cURL:

```bash
# Login
curl -X POST http://localhost:65375/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"nome":"admin","senha":"123456"}'

# Validar Token (substitua {TOKEN} pelo token recebido)
curl -X GET http://localhost:65375/api/auth/validate \
  -H "Authorization: Bearer {TOKEN}"
```

### Usando Swagger:

1. Acesse **http://localhost:65375**
2. Vá até `/api/auth/login`
3. Clique em **Try it out**
4. Digite as credenciais
5. Clique em **Execute**
6. Copie o token retornado
7. Clique no botão **Authorize** no topo
8. Cole o token e clique em **Authorize**
9. Agora você pode testar todos os endpoints protegidos!

## 🐛 Troubleshooting

### Frontend não conecta com o backend:

- Verifique se a API está rodando em `http://localhost:65375`
- Verifique as configurações de CORS no `Program.cs`

### Erro "Usuário ou senha inválidos":

- Verifique se o usuário existe no banco
- Execute: `SELECT * FROM USUARIO WHERE NOME = 'admin';`
- Verifique se ATIVO = '1' e BLOQUEIO = '0'

### Token expirou:

- Faça logout e login novamente
- Ou altere `ExpirationHours` no `appsettings.json`

## 📚 Documentação Completa

Para mais detalhes, consulte: **AUTENTICACAO.md**
