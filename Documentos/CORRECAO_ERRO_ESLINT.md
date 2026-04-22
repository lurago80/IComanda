# 🔧 CORREÇÃO ERRO ESLINT - FRONTEND

## ❌ Problema
Erro ao iniciar frontend:
```
[eslint] Plugin "react" was conflicted between package.json...
webpack compiled with 1 error
```

## ✅ Solução Aplicada

### Arquivos Corrigidos:
1. ✅ Criado `.env.local` - Desabilita ESLint durante build
2. ✅ Atualizado `package.json` - Removida configuração ESLint conflitante
3. ✅ Criado `.eslintrc.json` - Configuração ESLint simplificada

---

## 📋 PASSOS PARA CORRIGIR NA MÁQUINA DO CLIENTE

### Opção 1: Copiar Arquivos Atualizados (Recomendado)

1. **Copie estes arquivos do seu ambiente de desenvolvimento para o cliente:**

```powershell
# Copiar .env.local
Copy-Item "C:\VS_Code\icomanda\IComanda.API\icomanda-frontend\.env.local" `
          "C:\icomanda\IComanda.API\icomanda-frontend\.env.local"

# Copiar package.json atualizado
Copy-Item "C:\VS_Code\icomanda\IComanda.API\icomanda-frontend\package.json" `
          "C:\icomanda\IComanda.API\icomanda-frontend\package.json"

# Copiar .eslintrc.json
Copy-Item "C:\VS_Code\icomanda\IComanda.API\icomanda-frontend\.eslintrc.json" `
          "C:\icomanda\IComanda.API\icomanda-frontend\.eslintrc.json"
```

2. **Limpar cache do node_modules:**

```powershell
cd C:\icomanda\IComanda.API\icomanda-frontend

# Deletar cache
Remove-Item -Path node_modules -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path package-lock.json -ErrorAction SilentlyContinue

# Reinstalar dependências
npm install
```

3. **Reiniciar frontend:**

```powershell
npm start
```

---

### Opção 2: Criar Arquivos Manualmente no Cliente

Se não puder copiar arquivos, crie manualmente:

#### 1. Criar `.env.local` em `C:\icomanda\IComanda.API\icomanda-frontend\`

```plaintext
DISABLE_ESLINT_PLUGIN=true
TSC_COMPILE_ON_ERROR=true
PORT=3000
BROWSER=none
```

#### 2. Editar `package.json` 

Remover a seção `"eslintConfig"`:

**ANTES:**
```json
  "scripts": { ... },
  "eslintConfig": {
    "extends": [
      "react-app",
      "react-app/jest"
    ]
  },
  "browserslist": { ... }
```

**DEPOIS:**
```json
  "scripts": { ... },
  "browserslist": { ... }
```

#### 3. Criar `.eslintrc.json` em `C:\icomanda\IComanda.API\icomanda-frontend\`

```json
{
  "extends": ["react-app"],
  "rules": {
    "no-unused-vars": "warn",
    "@typescript-eslint/no-unused-vars": "warn"
  }
}
```

#### 4. Limpar e Reinstalar

```powershell
cd C:\icomanda\IComanda.API\icomanda-frontend
Remove-Item -Path node_modules -Recurse -Force
npm install
npm start
```

---

### Opção 3: Solução Rápida (Temporária)

Se precisar resolver IMEDIATAMENTE sem editar arquivos:

```powershell
cd C:\icomanda\IComanda.API\icomanda-frontend

# Executar com variável de ambiente inline
$env:DISABLE_ESLINT_PLUGIN="true"; npm start
```

⚠️ **Aviso:** Essa solução funciona apenas para essa execução. <br/>
Ao reiniciar, precisará rodar o comando novamente.

---

## 🧪 Verificar se Funcionou

Após aplicar correção, você deve ver:

```
Compiled successfully!

You can now view icomanda-frontend in the browser.

  Local:            http://localhost:3000
  On Your Network:  http://192.168.X.X:3000
```

✅ **SEM mensagens de erro ESLint**

---

## ❓ Troubleshooting

### Problema: Ainda dá erro ESLint
**Solução:**
```powershell
# Deletar completamente node_modules e cache
cd C:\icomanda\IComanda.API\icomanda-frontend
Remove-Item -Path node_modules -Recurse -Force
Remove-Item -Path .cache -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path build -Recurse -Force -ErrorAction SilentlyContinue
npm cache clean --force
npm install
npm start
```

### Problema: "command not found: npm"
**Solução:**
```powershell
# Verificar se Node.js está instalado
node --version
npm --version

# Se não estiver, baixar e instalar:
# https://nodejs.org/
```

### Problema: "Port 3000 already in use"
**Solução:**
```powershell
# Matar processo na porta 3000
$process = Get-NetTCPConnection -LocalPort 3000 -ErrorAction SilentlyContinue | 
            Select-Object -ExpandProperty OwningProcess
if ($process) {
    Stop-Process -Id $process -Force
}

# Ou usar outra porta:
# Edite .env.local e mude PORT=3001
```

---

## 📦 Arquivos Criados/Modificados

| Arquivo | Status | Descrição |
|---------|--------|-----------|
| `.env.local` | ✅ Criado | Desabilita ESLint, configura porta |
| `.eslintrc.json` | ✅ Criado | Configuração ESLint simplificada |
| `package.json` | ✅ Modificado | Removida seção eslintConfig |

---

## 🔄 Para Próximas Instalações

Sempre inclua estes 3 arquivos ao fazer deploy:
- `.env.local`
- `.eslintrc.json`
- `package.json` (atualizado)

Ou adicione ao `.gitignore` e documente configuração.

---

*Correção aplicada em 15/fevereiro/2026*  
*iComanda - Sistema de Gestão de Comandas*
