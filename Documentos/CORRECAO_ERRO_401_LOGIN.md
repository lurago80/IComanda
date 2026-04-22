# Correção do Erro 401 após Login - Cliente

## 🔍 Problema Identificado

O erro 401 (Unauthorized) ocorria porque o frontend estava tentando fazer login usando uma URL hardcoded `http://localhost:65375/api/auth/login`, mesmo quando rodando em outra máquina na rede.

### Exemplo do erro:
- Frontend rodando em: `http://192.168.0.50:3000`
- Backend rodando em: `http://192.168.0.50:65375`
- Mas o login tentava acessar: `http://localhost:65375/api/auth/login` ❌

Isso fazia com que o frontend tentasse se conectar ao servidor local da máquina do navegador ao invés do servidor real.

## ✅ Solução Aplicada

Foi corrigido o arquivo `Login.tsx` para usar detecção automática de URL do servidor, igual ao resto da aplicação.

## 📦 Arquivos que Devem ser Atualizados no Cliente

### Frontend (React)
1. **`icomanda-frontend/src/pages/Login.tsx`** ⭐ CRÍTICO
   - Agora usa detecção automática de URL
   - Logs melhorados para diagnóstico
   - Tratamento de erros aprimorado

2. **`icomanda-frontend/src/services/api.ts`** ⭐ RECOMENDADO
   - Logs melhorados nos interceptors
   - Ajuda a diagnosticar problemas com tokens

### Backend (.NET)
3. **`IComanda.API/appsettings.json`** ⭐ CRÍTICO
   - Certifique-se que as configurações JWT estão idênticas entre desenvolvimento e cliente:
     - `Jwt:Key` deve ser a mesma
     - `Jwt:Issuer` deve ser `"IComanda.API"`
     - `Jwt:Audience` deve ser `"IComanda.Client"`

4. **Repositórios atualizados** (se aplicável):
   - `Repositories/Implementations/OperadorComissaoRepository.cs` (novo)
   - `Repositories/Implementations/RecebimentoRepository.cs` (atualizado)
   - `Repositories/Interfaces/IGrupoProdutoRepository.cs`
   - `Repositories/Interfaces/IOperadorComissaoRepository.cs`
   - `Repositories/Interfaces/IRecebimentoRepository.cs`

5. **Controllers atualizados**:
   - `Controllers/GrupoProdutoController.cs`
   - `Controllers/OperadorComissaoController.cs`
   - `Controllers/RecebimentoController.cs`

## 🚀 Passos para Atualizar no Cliente

### 1. Parar os serviços
```powershell
# Parar backend e frontend
.\parar-tudo.bat
```

### 2. Atualizar os arquivos
Copie os arquivos listados acima para o servidor do cliente, mantendo a estrutura de pastas.

### 3. Verificar appsettings.json
Abra `IComanda.API/appsettings.json` e confira a seção JWT:
```json
{
  "Jwt": {
    "Key": "icomanda-super-secret-key-2024-change-in-production-minimum-32-characters",
    "Issuer": "IComanda.API",
    "Audience": "IComanda.Client",
    "ExpirationHours": 2,
    "RefreshExpirationDays": 7
  }
}
```

### 4. Recompilar o frontend
```powershell
cd IComanda.API\icomanda-frontend
npm install  # Se houver dependências novas
npm run build
```

### 5. Reiniciar os serviços
```powershell
# Voltar à raiz do projeto
cd ..\..
.\iniciar-tudo.bat
```

## 🧪 Como Testar

### 1. Abrir o DevTools do navegador (F12)
- Vá para a aba **Console**
- Vá para a aba **Network**

### 2. Fazer login
- Digite usuário e senha
- Clique em "Entrar no Sistema"

### 3. Verificar os logs no Console
Você deve ver logs similares a:
```
🧹 [Login] Limpando tokens antigos...
🔐 [Login] Tentando login em: http://192.168.0.50:65375/api/auth/login
✅ [Login] Login bem-sucedido: { id: 1, nome: "Admin" }
🔑 [Login] Token recebido (primeiros 20 chars): eyJhbGciOiJIUzI1NiIs...
💾 [Login] Token salvo no localStorage
💾 [Login] Informações do usuário salvas
🔄 [Login] Recarregando página...
```

### 4. Verificar requisições na aba Network
Após o login, ao acessar `/vendas/abertas`, você deve ver:
```
Request Headers:
  Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

Response:
  Status: 200 OK
```

## 🐛 Solução de Problemas

### Ainda recebe 401 após login?

#### 1. Confira o token no localStorage
Abra o Console do DevTools e execute:
```javascript
localStorage.getItem('jwt_token')
```
Se retornar `null`, o token não foi salvo. Verifique os logs do login.

#### 2. Verifique se o token está sendo enviado
Na aba Network, clique na requisição que deu 401 e veja os **Request Headers**.
Deve conter:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

Se não estiver presente, limpe o cache do navegador e tente novamente.

#### 3. Valide o token em jwt.io
- Copie o token do localStorage
- Cole em https://jwt.io/
- Verifique se os campos são:
  - `iss`: `"IComanda.API"`
  - `aud`: `"IComanda.Client"`
  - Claims incluem `UserId`, `Role`, etc.

#### 4. Confira o horário do sistema
Se o horário do servidor e do cliente estiverem muito diferentes, o token pode ser rejeitado como expirado.
Sincronize os relógios.

#### 5. Verifique a chave JWT
Se o backend do cliente estiver usando uma chave JWT diferente do ambiente de desenvolvimento, os tokens não serão compatíveis.

Confira em `appsettings.json`:
```json
"Jwt": {
  "Key": "icomanda-super-secret-key-2024-change-in-production-minimum-32-characters"
}
```

### Logs úteis para diagnóstico

No Console do navegador, você verá:
- `🔐 [Login]` - Processo de login
- `🔑 [API]` - Token sendo anexado às requisições
- `❌ [Axios]` - Erros nas requisições
- `⚠️ [Axios]` - Avisos (como falta de token)

## ✨ Melhorias Implementadas

1. **Detecção automática de URL do servidor**
   - Funciona em localhost e rede local
   - Não precisa configurar manualmente

2. **Logs de diagnóstico**
   - Facilita identificar problemas
   - Mostra exatamente onde ocorreu o erro

3. **Melhor tratamento de erros**
   - Mensagens mais claras
   - Informações úteis para debug

4. **Validação do token**
   - Verifica se o token existe antes de enviar
   - Loga o início do token para conferência

## 📞 Suporte

Se o problema persistir após seguir todos os passos:
1. Capture os logs completos do Console (F12)
2. Capture a requisição de login na aba Network
3. Envie para análise junto com o `appsettings.json` (sem expor senhas)
