# SOLUCAO: Erro MSB3027 - Arquivo DLL Bloqueado

## Problema
```
error MSB3027: nao foi possivel copiar "obj\Release\net8.0\IComanda.API.dll" 
para "bin\Release\net8.0\IComanda.API.dll". 
O arquivo e bloqueado por: ".NET Host (27380)"
```

## Causa
O processo `dotnet.exe` esta executando a DLL e bloqueando o arquivo durante a compilacao.

---

## SOLUCAO RAPIDA

### Metodo 1: Script Automatico (RECOMENDADO)
Este script verifica e libera TODAS as portas automaticamente:
- Porta 5000 (Backend API)
- Porta 3000 (Frontend React)
- Porta 3001 (WhatsApp Baileys)
- Porta 3050 (Firebird - nao encerra)

```batch
cd C:\VS_Code\icomanda\IComanda.API
iniciar-sistema.bat
```
OU
```powershell
cd C:\VS_Code\icomanda\IComanda.API
.\iniciar-sistema.ps1
```

### Metodo 2: Parar Sistema
```batch
parar-sistema.bat
```

### Metodo 3: Manual
```powershell
# 1. Parar TODOS os processos dotnet
Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# 2. Limpar arquivos de build
cd C:\VS_Code\icomanda\IComanda.API
Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue

# 3. Compilar do zero
dotnet build --configuration Release --no-incremental

# 4. Executar
dotnet bin\Release\net8.0\IComanda.API.dll
```

---

## PREVENCAO

### Sempre use este comando para parar:
```powershell
Stop-Process -Name "dotnet" -Force
```

### NUNCA feche a janela do terminal com "X"
Isso deixa o processo rodando em background.

---

## VERIFICAR SE AINDA ESTA RODANDO

### Ver processos dotnet ativos:
```powershell
Get-Process -Name "dotnet"
```

### Ver qual processo esta usando porta 5000:
```powershell
netstat -ano | findstr ":5000"
```

### Matar processo especifico:
```powershell
Stop-Process -Id <PID> -Force
```

---

## SCRIPTS DISPONIVEIS

| Script | Descricao |
|--------|-----------|
| `iniciar-sistema.bat` | Inicia backend automaticamente |
| `iniciar-sistema.ps1` | Versao PowerShell com mais logs |
| `parar-tudo.bat` | Para todos os processos |
| `testar-autenticacao-completa.ps1` | Testa se esta funcionando |

---

## SE NADA FUNCIONAR

### Opcao 1: Reiniciar Windows
O ultimo recurso para liberar todos os processos bloqueados.

### Opcao 2: Usar Debug ao inves de Release
```powershell
dotnet build --configuration Debug
dotnet bin\Debug\net8.0\IComanda.API.dll
```

---

**Desenvolvido por:** GitHub Copilot  
**Data:** 16/02/2026
