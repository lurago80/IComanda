# Script para verificar status e diagnosticar problemas

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  VERIFICACAO DE STATUS - WhatsApp Web" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se backend está rodando
Write-Host "[1] Verificando se backend esta rodando..." -ForegroundColor Yellow
try {
    $status = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/status" -Method Get -ErrorAction Stop
    Write-Host "   [OK] Backend esta respondendo" -ForegroundColor Green
    Write-Host "   Status: $($status.conectado)" -ForegroundColor $(if ($status.conectado) { "Green" } else { "Red" })
} catch {
    Write-Host "   [ERRO] Backend nao esta respondendo: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "   SOLUCAO: Inicie o backend:" -ForegroundColor Yellow
    Write-Host "   cd IComanda.API" -ForegroundColor Gray
    Write-Host "   dotnet run" -ForegroundColor Gray
    exit 1
}
Write-Host ""

# Verificar diagnóstico completo
Write-Host "[2] Obtendo diagnostico completo..." -ForegroundColor Yellow
try {
    $diagnostico = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/diagnostico" -Method Get -ErrorAction Stop
    
    Write-Host "   Inicializado: $($diagnostico.inicializado)" -ForegroundColor $(if ($diagnostico.inicializado) { "Green" } else { "Red" })
    Write-Host "   Driver Ativo: $($diagnostico.driverAtivo)" -ForegroundColor $(if ($diagnostico.driverAtivo) { "Green" } else { "Red" })
    Write-Host "   Status Final: $($diagnostico.statusFinal)" -ForegroundColor Cyan
    
    if ($diagnostico.urlAtual) {
        Write-Host "   URL Atual: $($diagnostico.urlAtual)" -ForegroundColor Gray
    }
    
    if ($diagnostico.erro) {
        Write-Host "   [ERRO] $($diagnostico.erro)" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "   Portas testadas:" -ForegroundColor Cyan
    foreach ($porta in $diagnostico.portasTestadas) {
        $statusPorta = if ($porta.aberta) { "[ABERTA]" } else { "[FECHADA]" }
        $cor = if ($porta.aberta) { "Green" } else { "Red" }
        Write-Host "      Porta $($porta.porta): $statusPorta" -ForegroundColor $cor
        
        if ($porta.abas -and $porta.abas.Count -gt 0) {
            Write-Host "         Abas encontradas: $($porta.abas.Count)" -ForegroundColor Gray
            foreach ($aba in $porta.abas) {
                if ($aba.url -like "*web.whatsapp.com*") {
                    Write-Host "         [OK] WhatsApp Web encontrado!" -ForegroundColor Green
                    Write-Host "            Titulo: $($aba.title)" -ForegroundColor Gray
                }
            }
        }
    }
    
} catch {
    Write-Host "   [ERRO] Nao foi possivel obter diagnostico: $_" -ForegroundColor Red
}
Write-Host ""

# Verificar Chrome
Write-Host "[3] Verificando processos do Chrome..." -ForegroundColor Yellow
$chromeProcesses = Get-Process -Name "chrome" -ErrorAction SilentlyContinue
if ($chromeProcesses) {
    Write-Host "   [OK] Chrome esta rodando ($($chromeProcesses.Count) processo(s))" -ForegroundColor Green
    
    $temRemoteDebugging = $false
    foreach ($proc in $chromeProcesses) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)").CommandLine
            if ($cmdLine -like "*remote-debugging-port*") {
                Write-Host "   [OK] Processo $($proc.Id) tem remote-debugging-port" -ForegroundColor Green
                $temRemoteDebugging = $true
            }
        } catch { }
    }
    
    if (-not $temRemoteDebugging) {
        Write-Host "   [ERRO] Nenhum processo Chrome com remote-debugging-port!" -ForegroundColor Red
        Write-Host ""
        Write-Host "   SOLUCAO:" -ForegroundColor Yellow
        Write-Host "   1. Execute: .\CORRIGIR_TUDO.ps1" -ForegroundColor Gray
        Write-Host "   2. Ou feche o Chrome e execute: .\iniciar-chrome-whatsapp.bat" -ForegroundColor Gray
    }
} else {
    Write-Host "   [ERRO] Chrome NAO esta rodando!" -ForegroundColor Red
    Write-Host ""
    Write-Host "   SOLUCAO: Execute: .\CORRIGIR_TUDO.ps1" -ForegroundColor Yellow
}
Write-Host ""

# Verificar porta 9222
Write-Host "[4] Verificando porta 9222..." -ForegroundColor Yellow
$connection = Test-NetConnection -ComputerName localhost -Port 9222 -WarningAction SilentlyContinue -InformationLevel Quiet
if ($connection) {
    Write-Host "   [OK] Porta 9222 esta ABERTA" -ForegroundColor Green
} else {
    Write-Host "   [ERRO] Porta 9222 esta FECHADA" -ForegroundColor Red
    
    # Verificar se Chrome está usando outra porta
    $chromeProcesses = Get-Process -Name "chrome" -ErrorAction SilentlyContinue
    $outrasPortas = @()
    foreach ($proc in $chromeProcesses) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)").CommandLine
            if ($cmdLine -like "*remote-debugging-port*" -and $cmdLine -match "remote-debugging-port=(\d+)") {
                $porta = [int]$matches[1]
                if ($porta -ne 9222 -and -not $outrasPortas.Contains($porta)) {
                    $outrasPortas += $porta
                }
            }
        } catch { }
    }
    
    if ($outrasPortas.Count -gt 0) {
        Write-Host "   [AVISO] Chrome esta usando outras portas: $($outrasPortas -join ', ')" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "   SOLUCAO: Execute: .\CORRIGIR_PORTA.ps1" -ForegroundColor Yellow
        Write-Host "   (Este script fecha o Chrome e reinicia na porta 9222)" -ForegroundColor Gray
    } else {
        Write-Host ""
        Write-Host "   SOLUCAO: Execute: .\CORRIGIR_TUDO.ps1" -ForegroundColor Yellow
    }
}
Write-Host ""

# Resumo
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESUMO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($status.conectado) {
    Write-Host "[SUCESSO] WhatsApp Web esta CONECTADO!" -ForegroundColor Green
} else {
    Write-Host "[PROBLEMA] WhatsApp Web NAO esta conectado" -ForegroundColor Red
    Write-Host ""
    Write-Host "PASSO A PASSO PARA CORRIGIR:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Execute: .\CORRIGIR_TUDO.ps1" -ForegroundColor White
    Write-Host "2. Faca login no WhatsApp Web" -ForegroundColor White
    Write-Host "3. Execute este script novamente: .\VERIFICAR_STATUS.ps1" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
