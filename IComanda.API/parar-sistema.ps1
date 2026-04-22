# ========================================
# SCRIPT PARA PARAR O SISTEMA ICOMANDA
# Encerra todos os processos do sistema
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PARANDO SISTEMA ICOMANDA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ========================================
# PARAR PROCESSOS DOTNET (BACKEND)
# ========================================
Write-Host "[1/3] Parando backend (dotnet)..." -ForegroundColor Yellow

$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    Write-Host "  Encontrados $($dotnetProcesses.Count) processo(s) dotnet" -ForegroundColor Yellow
    
    foreach ($proc in $dotnetProcesses) {
        Write-Host "  Encerrando PID: $($proc.Id) - $($proc.ProcessName)" -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    
    Start-Sleep -Seconds 2
    
    $remaining = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    if ($remaining) {
        Write-Host "  AVISO: Ainda existem processos dotnet rodando" -ForegroundColor Red
    } else {
        Write-Host "  Backend parado com sucesso" -ForegroundColor Green
    }
} else {
    Write-Host "  Nenhum processo dotnet encontrado" -ForegroundColor Green
}

Write-Host ""

# ========================================
# PARAR PROCESSOS NODE (FRONTEND/WHATSAPP)
# ========================================
Write-Host "[2/3] Parando frontend/WhatsApp (node)..." -ForegroundColor Yellow

$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue

if ($nodeProcesses) {
    Write-Host "  Encontrados $($nodeProcesses.Count) processo(s) node" -ForegroundColor Yellow
    
    foreach ($proc in $nodeProcesses) {
        Write-Host "  Encerrando PID: $($proc.Id) - $($proc.ProcessName)" -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    
    Start-Sleep -Seconds 2
    
    $remaining = Get-Process -Name "node" -ErrorAction SilentlyContinue
    if ($remaining) {
        Write-Host "  AVISO: Ainda existem processos node rodando" -ForegroundColor Red
    } else {
        Write-Host "  Frontend/WhatsApp parados com sucesso" -ForegroundColor Green
    }
} else {
    Write-Host "  Nenhum processo node encontrado" -ForegroundColor Green
}

Write-Host ""

# ========================================
# VERIFICAR PORTAS
# ========================================
Write-Host "[3/3] Verificando portas..." -ForegroundColor Yellow

$portas = @(5000, 3000, 3001)

foreach ($porta in $portas) {
    $connections = netstat -ano | Select-String ":$porta\s" | Select-String "LISTENING"
    
    if ($connections) {
        Write-Host "  Porta $porta ainda esta em uso!" -ForegroundColor Red
    } else {
        Write-Host "  Porta $porta liberada" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SISTEMA PARADO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
