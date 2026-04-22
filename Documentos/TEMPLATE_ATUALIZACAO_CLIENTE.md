# 📋 TEMPLATE DE ATUALIZAÇÃO PARA CLIENTE

> **Este é um template:** A cada atualização/correção/implementação, será criado um arquivo específico com base neste modelo.

---

## 🎯 RESUMO DA ATUALIZAÇÃO

**Data:** [DATA]  
**Tipo:** [Correção | Nova Feature | Atualização | Otimização]  
**Descrição:** [Breve descrição do que foi feito]

---

## 📂 ARQUIVOS MODIFICADOS/CRIADOS

### ✅ Backend - Copiar para `C:\iComanda\IComanda.API\`

```
[Lista de arquivos modificados no backend]
```

**Exemplo:**
```
✏️  MODIFICADO: Controllers/AuthController.cs
✏️  MODIFICADO: Services/Implementations/PasswordHasher.cs
➕ NOVO: criar-tabela-refresh-token.sql
➕ NOVO: executar-sql-refresh-token.ps1
```

### ✅ Frontend - Copiar para `C:\iComanda\icomanda-frontend\`

```
[Lista de arquivos modificados no frontend]
```

**Exemplo:**
```
✏️  MODIFICADO: src/pages/Login.tsx
➕ NOVO: .env.local
➕ NOVO: .eslintrc.json
❌ REMOVIDO: .eslintrc.js
```

### ✅ Raiz do Projeto - Copiar para `C:\iComanda\`

```
[Lista de arquivos na raiz]
```

**Exemplo:**
```
➕ NOVO: CORRECAO_ERRO_401.md
✏️  MODIFICADO: iniciar-tudo.bat
```

---

## 🔄 AÇÕES NECESSÁRIAS NO CLIENTE

### 1️⃣ Parar Aplicação
```powershell
cd C:\iComanda
.\parar-tudo.bat
```

### 2️⃣ Copiar Arquivos Modificados
```powershell
# Backend
Copy-Item "D:\atualizacao\Controllers\AuthController.cs" -Destination "C:\iComanda\IComanda.API\Controllers\" -Force
Copy-Item "D:\atualizacao\criar-tabela-refresh-token.sql" -Destination "C:\iComanda\IComanda.API\" -Force

# Frontend
Copy-Item "D:\atualizacao\.env.local" -Destination "C:\iComanda\icomanda-frontend\" -Force
```

### 3️⃣ Executar Scripts/Migrações (se houver)
```powershell
# Exemplo: Criar nova tabela
cd C:\iComanda\IComanda.API
.\executar-sql-refresh-token.ps1
```

### 4️⃣ Recompilar (se necessário)
```powershell
# Backend
cd C:\iComanda\IComanda.API
dotnet build -c Release

# Frontend (se mudou código-fonte)
cd C:\iComanda\icomanda-frontend
npm install  # Apenas se package.json mudou
npm run build
```

### 5️⃣ Reiniciar Aplicação
```powershell
cd C:\iComanda
.\iniciar-tudo.bat
```

---

## ⚙️ CONFIGURAÇÕES A VERIFICAR

```
[ ] appsettings.json - Caminho do banco correto?
[ ] .env.local - Configurações corretas?
[ ] Tabelas novas criadas no banco?
[ ] Dependências instaladas (npm install)?
```

---

## 🧪 TESTES PÓS-ATUALIZAÇÃO

```
[ ] Backend inicia sem erros
[ ] Frontend inicia sem erros
[ ] Login funciona
[ ] Funcionalidade X testada
[ ] Sem erros 401/403
[ ] Logs não mostram erros
```

---

## 🆘 ROLLBACK (em caso de problema)

```powershell
# Voltar backup dos arquivos antigos
Copy-Item "C:\iComanda\backup\[DATA]\*" -Destination "C:\iComanda\" -Recurse -Force

# Reiniciar
.\parar-tudo.bat
.\iniciar-tudo.bat
```

---

## 📝 NOTAS ADICIONAIS

[Informações extras importantes sobre a atualização]

---

*Template gerado em 15/Fevereiro/2026*  
*iComanda - Sistema de Gestão de Comandas*
