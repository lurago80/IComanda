# ✅ SISTEMA PRONTO PARA USO

## 🚀 Como Iniciar

### Método 1: Script Automático (RECOMENDADO)
```batch
iniciar-sistema.bat
```

### Método 2: Manual
1. **Backend:**
   ```powershell
   cd C:\VS_Code\icomanda\IComanda.API
   dotnet bin\Debug\net8.0\IComanda.API.dll
   ```

2. **Frontend:**
   - Abrir navegador: http://localhost:3000

---

## 🔑 Credenciais de Teste

| Usuário | Senha  | Permissão |
|---------|--------|-----------|
| INOVE   | 1401   | Garçom    |
| LOJA    | 1234   | Caixa     |
| admin   | 123456 | Caixa     |

---

## 🧪 Testar Autenticação

```powershell
.\testar-autenticacao-completa.ps1
```

**Resultado Esperado:**
```
Status: 200
Token recebido (primeiros 30 chars): eyJhbGciOiJIUzI1NiIs...
Username: INOVE
Role: Garcom
Expira em: 2 horas
AUTENTICACAO FUNCIONANDO PERFEITAMENTE!
```

---

## 🌐 URLs Importantes

- **Frontend:** http://localhost:3000
- **Backend API:** http://localhost:5000
- **Swagger (Documentação):** http://localhost:5000/swagger
- **Health Check:** http://localhost:5000/health

---

## ⚙️ Como Compilar

### Backend:
```powershell
cd C:\VS_Code\icomanda\IComanda.API
dotnet build
```

### Frontend:
```powershell
cd C:\VS_Code\icomanda\IComanda.API\icomanda-frontend
npm run build
```

---

## 🛑 Como Parar

```batch
parar-tudo.bat
```

Ou manualmente:
```powershell
Stop-Process -Name "dotnet" -Force
```

---

## 📊 Status Atual

✅ **Backend:** Compilado e funcionando  
✅ **Frontend:** Build completo  
✅ **Autenticação:** JWT funcionando  
✅ **Banco de Dados:** Firebird conectado  
✅ **CORS:** Configurado  
✅ **Login:** Funcionando perfeitamente  

---

## 🔧 Correções Recentes

### Login 401 Unauthorized (RESOLVIDO)
- **Problema:** Frontend salvava campos errados após login
- **Solução:** Corrigido mapeamento `data.nome` → `data.username` e `data.id` → `data.userId`
- **Documentação:** [CORRECAO_TOKEN_401.md](CORRECAO_TOKEN_401.md)

### Erro "Senha inválida" (RESOLVIDO)
- **Problema:** Usuário usando senha errada
- **Solução:** Senha correta do INOVE é `1401` (não `1234`)
- **Documentação:** [SOLUCAO_LOGIN_COMPLETA.md](SOLUCAO_LOGIN_COMPLETA.md)

### Campo SENHA pequeno (TEMPORÁRIO)
- **Problema:** Campo SENHA não suporta BCrypt (60 chars)
- **Solução Atual:** BCrypt desabilitado, senhas em plaintext
- **Solução Definitiva:** Executar script [MIGRAR_AUTENTICACAO_COMPLETA.sql](IComanda.API/MIGRAR_AUTENTICACAO_COMPLETA.sql)

---

## 📝 Documentação Completa

1. [SOLUCAO_LOGIN_COMPLETA.md](SOLUCAO_LOGIN_COMPLETA.md) - Problemas de login
2. [CORRECAO_TOKEN_401.md](CORRECAO_TOKEN_401.md) - Erro 401 Unauthorized
3. [MIGRAR_AUTENTICACAO_COMPLETA.sql](IComanda.API/MIGRAR_AUTENTICACAO_COMPLETA.sql) - Migração de autenticação

---

## ⚠️ Troubleshooting

### Backend não inicia
```powershell
# Verificar se porta 5000 está em uso
netstat -ano | findstr ":5000"

# Parar processo
Stop-Process -Id <PID> -Force
```

### Frontend não conecta com backend
1. Verificar se backend está rodando: http://localhost:5000/health
2. Limpar cache do navegador (Ctrl+Shift+Delete)
3. Verificar console do navegador (F12)

### Erro "Nenhuma conexão pôde ser feita"
- Backend não está rodando
- Executar: `iniciar-sistema.bat`

---

**Última Atualização:** 16/02/2026  
**Desenvolvido por:** GitHub Copilot + Claude Sonnet 4.5
