# PORTAS UTILIZADAS PELO SISTEMA ICOMANDA

## Portas do Sistema

| Servico | Porta | Protocolo | Descricao |
|---------|-------|-----------|-----------|
| **Backend API** | 5000 | HTTP | API principal do sistema |
| **Frontend React** | 3000 | HTTP | Interface web (dev mode) |
| **WhatsApp Baileys** | 3001 | HTTP | Servico de mensagens WhatsApp |
| **Firebird Database** | 3050 | TCP | Banco de dados Firebird |

---

## Verificacao Manual de Portas

### Verificar todas as portas:
```powershell
netstat -ano | findstr ":5000 :3000 :3001 :3050"
```

### Verificar porta especifica:
```powershell
netstat -ano | findstr ":5000"
```

---

## Liberar Porta Manualmente

### 1. Identificar processo na porta:
```powershell
netstat -ano | findstr ":5000"
```

### 2. Anotar o PID (ultima coluna)

### 3. Encerrar processo:
```powershell
Stop-Process -Id <PID> -Force
```

Exemplo:
```powershell
# Se o PID for 12345
Stop-Process -Id 12345 -Force
```

---

## Scripts Automaticos

### Iniciar Sistema (RECOMENDADO)
```batch
cd C:\VS_Code\icomanda\IComanda.API
iniciar-sistema.bat
```

O script automaticamente:
1. Verifica quais portas estao ocupadas
2. Encerra processos que estejam usando as portas
3. Limpa arquivos de build bloqueados
4. Compila o backend
5. Inicia o backend
6. Verifica se o backend esta saudavel

### Parar Sistema
```batch
cd C:\VS_Code\icomanda\IComanda.API
parar-sistema.bat
```

O script encerra:
- Todos os processos dotnet (Backend)
- Todos os processos node (Frontend/WhatsApp)
- Verifica se as portas foram liberadas

---

## Conflitos Comuns

### Porta 5000 (Backend)
**Sintoma:** Backend nao inicia ou retorna erro de porta em uso

**Causa:** Outro processo dotnet ou aplicacao ASP.NET rodando

**Solucao:**
```powershell
Stop-Process -Name "dotnet" -Force
```

### Porta 3000 (Frontend)
**Sintoma:** `npm start` falha com erro de porta ocupada

**Causa:** Outro servidor React/Node rodando

**Solucao:**
```powershell
Stop-Process -Name "node" -Force
```

### Porta 3001 (WhatsApp)
**Sintoma:** Servico WhatsApp nao inicia

**Causa:** Instancia anterior do servidor WhatsApp ainda rodando

**Solucao:**
```powershell
Stop-Process -Name "node" -Force
```

### Porta 3050 (Firebird)
**Sintoma:** Erro de conexao com banco de dados

**Causa:** Firebird nao esta rodando OU firewall bloqueando

**Solucao:**
```powershell
# Verificar se Firebird esta rodando
Get-Service | Where-Object {$_.Name -like "*Firebird*"}

# Iniciar servico Firebird (se parado)
Start-Service FirebirdServerDefaultInstance
```

---

## Firewall

Se tiver problemas de acesso externo (de outros computadores da rede):

### Liberar porta no Firewall do Windows:
```powershell
New-NetFirewallRule -DisplayName "IComanda Backend" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
New-NetFirewallRule -DisplayName "IComanda Frontend" -Direction Inbound -Protocol TCP -LocalPort 3000 -Action Allow
New-NetFirewallRule -DisplayName "IComanda WhatsApp" -Direction Inbound -Protocol TCP -LocalPort 3001 -Action Allow
```

---

## Diagnostico Completo

Execute este comando para ver TUDO que esta rodando nas portas do sistema:

```powershell
Write-Host "=== PORTAS DO SISTEMA ===" -ForegroundColor Cyan
Write-Host ""

$portas = @{
    5000 = "Backend API"
    3000 = "Frontend React"
    3001 = "WhatsApp Baileys"
    3050 = "Firebird Database"
}

foreach ($porta in $portas.Keys) {
    Write-Host "Porta $porta ($($portas[$porta])):" -ForegroundColor Yellow
    $result = netstat -ano | Select-String ":$porta\s" | Select-String "LISTENING"
    
    if ($result) {
        $result | ForEach-Object {
            if ($_ -match '\s+(\d+)\s*$') {
                $pid = $matches[1]
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                if ($process) {
                    Write-Host "  PID: $pid - Processo: $($process.ProcessName)" -ForegroundColor Green
                }
            }
        }
    } else {
        Write-Host "  LIVRE" -ForegroundColor Gray
    }
    Write-Host ""
}
```

---

**Desenvolvido por:** GitHub Copilot  
**Data:** 16/02/2026
