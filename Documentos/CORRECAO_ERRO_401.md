# 🔴 ERRO 401 UNAUTHORIZED - CORREÇÃO RÁPIDA

## ❌ Problema

Cliente está recebendo erro:
```
[21:13:33 INF] Authorization failed. These requirements were not met:
DenyAnonymousAuthorizationRequirement: Requires an authenticated user.
[21:13:33 INF] Request finished HTTP/1.1 GET http://localhost:65375/api/vendas/abertas - 401 null
```

## 🎯 Causa

**Token JWT expirado ou inválido**. Isso aconteceu porque:
1. Reduzimos expiração de JWT de 240h (10 dias) para **2h**
2. Tokens antigos ainda estão no localStorage do navegador
3. Servidor foi reiniciado (RefreshTokens em memória foram perdidos)

---

## ✅ SOLUÇÃO IMEDIATA (5 minutos)

### 1️⃣ LIMPAR CACHE DO NAVEGADOR

**No navegador do cliente:**

1. Pressione **F12** (abrir DevTools)
2. Vá na aba **Console**
3. Execute:
   ```javascript
   localStorage.clear()
   sessionStorage.clear()
   console.log("✅ Cache limpo!")
   ```
4. Feche as DevTools
5. Pressione **Ctrl + Shift + R** (hard reload)
6. **Faça login novamente**

✅ O erro deve sumir!

---

### 2️⃣ SE NÃO RESOLVER: Verificar Backend

**No servidor do cliente:**

```powershell
# 1. Parar tudo
cd C:\icomanda
.\parar-tudo.bat

# 2. Verificar se tabela REFRESH_TOKEN existe
cd C:\icomanda\IComanda.API
.\executar-sql-refresh-token.ps1

# 3. Reiniciar tudo
cd C:\icomanda
.\iniciar-tudo.bat
```

---

### 3️⃣ VERIFICAR APPSETTINGS.JSON

**Arquivo:** `C:\icomanda\IComanda.API\appsettings.json`

**Deve conter:**
```json
{
  "Jwt": {
    "SecretKey": "SUA_CHAVE_SECRETA_AQUI_MINIMO_32_CARACTERES",
    "Issuer": "IComandaAPI",
    "Audience": "IComandaFrontend",
    "ExpirationHours": 2,
    "RefreshExpirationDays": 7
  }
}
```

⚠️ **Se `SecretKey` estiver diferente** entre dev e produção, tokens não funcionam!

---

## 🔍 DIAGNÓSTICO COMPLETO

Execute no servidor do cliente:

```powershell
cd C:\icomanda
.\diagnosticar-jwt-401.ps1
```

O script verificará:
- ✅ Backend rodando?
- ✅ Login funciona?
- ✅ Token válido?
- ✅ Endpoint acessível?
- ✅ Tabela REFRESH_TOKEN existe?
- ✅ Configuração JWT correta?

---

## 📋 CHECKLIST DE CORREÇÃO

- [ ] **1. Limpar localStorage do navegador** (`localStorage.clear()`)
- [ ] **2. Hard reload** (Ctrl + Shift + R)
- [ ] **3. Fazer login novamente**
- [ ] **4. Verificar tabela REFRESH_TOKEN existe**
- [ ] **5. Verificar appsettings.json tem SecretKey**
- [ ] **6. Reiniciar backend se necessário**

---

## 🧪 TESTAR CORREÇÃO

**No navegador (F12 → Console):**

```javascript
// Verificar se token está salvo
console.log("Token:", localStorage.getItem('token'));
console.log("User:", localStorage.getItem('user'));

// Se retornar valores válidos → ✅ OK
// Se retornar null → ❌ Precisa fazer login
```

**PowerShell (testar API):**

```powershell
# Fazer login
$body = '{"username":"admin","password":"123456"}'
$login = Invoke-RestMethod -Uri "http://localhost:65375/api/auth/login" -Method POST -Body $body -ContentType "application/json"

# Testar endpoint protegido
$headers = @{ "Authorization" = "Bearer $($login.token)" }
$vendas = Invoke-RestMethod -Uri "http://localhost:65375/api/vendas/abertas" -Headers $headers

Write-Host "✅ Total de vendas: $($vendas.Count)"
```

Se aparecer **"Total de vendas: X"** → ✅ **FUNCIONANDO!**

---

## 🚨 PROBLEMAS COMUNS

### Problema: "localStorage.clear() não resolve"

**Causa:** Cookie de sessão antigo

**Solução:**
1. Abrir navegador em **modo anônimo/privado**
2. Acessar `http://localhost:3000`
3. Fazer login
4. Se funcionar → Limpar cookies e cache do navegador normal

### Problema: "Login funciona mas endpoint retorna 401"

**Causa:** Chave JWT diferente ou token corrompido

**Solução:**
```powershell
# Verificar logs do backend
# Deve mostrar: "✅ JWT configurado: Expiração Token=2h, Refresh=7d"

# Se não mostrar, appsettings.json está incorreto
```

### Problema: "Backend não inicia após criar tabela REFRESH_TOKEN"

**Causa:** Erro SQL ou banco corrompido

**Solução:**
```powershell
# Verificar logs de erro do backend
cd C:\icomanda\IComanda.API
dotnet run --configuration Release

# Procurar por erros de SQL no console
```

---

## 📞 SUPORTE RÁPIDO

**Erro persiste?** Execute e envie resultado:

```powershell
cd C:\icomanda
.\diagnosticar-jwt-401.ps1 > diagnostico.txt
notepad diagnostico.txt
```

Envie o conteúdo de `diagnostico.txt` para análise.

---

## ⚡ RESUMO: 3 PASSOS RÁPIDOS

```
1. F12 → Console → localStorage.clear()
2. Ctrl + Shift + R
3. Fazer login novamente
```

**Tempo: 30 segundos** ⏱️

---

*Guia criado em 15/fevereiro/2026*  
*iComanda - Sistema de Gestão de Comandas*
