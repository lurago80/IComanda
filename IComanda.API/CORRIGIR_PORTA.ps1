# Script para corrigir problema de porta do Chrome

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CORRIGINDO PORTA DO CHROME" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar qual porta o Chrome está usando
Write-Host "[1] Verificando processos do Chrome..." -ForegroundColor Yellow
$chromeProcesses = Get-Process -Name "chrome" -ErrorAction SilentlyContinue
$portasEncontradas = @()

if ($chromeProcesses) {
    Write-Host "   Chrome esta rodando ($($chromeProcesses.Count) processo(s))" -ForegroundColor Gray
    
    foreach ($proc in $chromeProcesses) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)").CommandLine
            if ($cmdLine -like "*remote-debugging-port*") {
                if ($cmdLine -match "remote-debugging-port=(\d+)") {
                    $porta = [int]$matches[1]
                    if (-not $portasEncontradas.Contains($porta)) {
                        $portasEncontradas += $porta
                        Write-Host "   [INFO] Processo $($proc.Id) usando porta: $porta" -ForegroundColor Cyan
                    }
                }
            }
        } catch { }
    }
    
    if ($portasEncontradas.Count -gt 0) {
        Write-Host ""
        Write-Host "   Portas encontradas: $($portasEncontradas -join ', ')" -ForegroundColor Yellow
        
        if (-not $portasEncontradas.Contains(9222)) {
            Write-Host "   [PROBLEMA] Chrome nao esta usando a porta 9222!" -ForegroundColor Red
            Write-Host "   Portas em uso: $($portasEncontradas -join ', ')" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "   [INFO] Chrome nao esta rodando" -ForegroundColor Gray
}
Write-Host ""

# Verificar se porta 9222 está aberta
Write-Host "[2] Verificando porta 9222..." -ForegroundColor Yellow
$porta9222Aberta = Test-NetConnection -ComputerName localhost -Port 9222 -WarningAction SilentlyContinue -InformationLevel Quiet

if ($porta9222Aberta) {
    Write-Host "   [OK] Porta 9222 esta ABERTA" -ForegroundColor Green
    Write-Host ""
    Write-Host "   O problema pode ser outro. Verifique:" -ForegroundColor Yellow
    Write-Host "   1. WhatsApp Web esta aberto e logado?" -ForegroundColor White
    Write-Host "   2. Backend esta rodando?" -ForegroundColor White
    Write-Host "   3. Execute: .\VERIFICAR_STATUS.ps1" -ForegroundColor White
    exit 0
} else {
    Write-Host "   [ERRO] Porta 9222 esta FECHADA" -ForegroundColor Red
}
Write-Host ""

# Fechar Chrome
Write-Host "[3] Fechando TODOS os processos do Chrome..." -ForegroundColor Yellow
if ($chromeProcesses) {
    Stop-Process -Name "chrome" -Force -ErrorAction SilentlyContinue
    Write-Host "   [OK] Chrome fechado" -ForegroundColor Green
    Start-Sleep -Seconds 3
} else {
    Write-Host "   [INFO] Chrome ja estava fechado" -ForegroundColor Gray
}
Write-Host ""

# Encontrar caminho do Chrome
Write-Host "[4] Encontrando Chrome..." -ForegroundColor Yellow
$chromePath = $null

$paths = @(
    "C:\Program Files\Google\Chrome\Application\chrome.exe",
    "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
)

foreach ($path in $paths) {
    if (Test-Path $path) {
        $chromePath = $path
        Write-Host "   [OK] Chrome encontrado: $chromePath" -ForegroundColor Green
        break
    }
}

if (-not $chromePath) {
    Write-Host "   [ERRO] Chrome nao encontrado!" -ForegroundColor Red
    Write-Host "   Instale o Google Chrome." -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# Iniciar Chrome na porta 9222
Write-Host "[5] Iniciando Chrome na porta 9222..." -ForegroundColor Yellow
$chromeArgs = @(
    "--remote-debugging-port=9222",
    "--disable-web-security",
    "--disable-features=IsolateOrigins,site-per-process"
)

Start-Process -FilePath $chromePath -ArgumentList $chromeArgs
Write-Host "   [OK] Chrome iniciado" -ForegroundColor Green
Start-Sleep -Seconds 5
Write-Host ""

# Verificar se porta 9222 está aberta agora
Write-Host "[6] Verificando se porta 9222 esta aberta..." -ForegroundColor Yellow
$tentativas = 0
$maxTentativas = 10
$portaAberta = $false

while (-not $portaAberta -and $tentativas -lt $maxTentativas) {
    $connection = Test-NetConnection -ComputerName localhost -Port 9222 -WarningAction SilentlyContinue -InformationLevel Quiet
    if ($connection) {
        Write-Host "   [OK] Porta 9222 esta ABERTA!" -ForegroundColor Green
        $portaAberta = $true
    } else {
        $tentativas++
        if ($tentativas -lt $maxTentativas) {
            Write-Host "   [AGUARDANDO] Tentativa $tentativas/$maxTentativas..." -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        } else {
            Write-Host "   [ERRO] Porta 9222 ainda nao esta aberta apos $maxTentativas tentativas" -ForegroundColor Red
            Write-Host "   Verifique se o Chrome iniciou corretamente" -ForegroundColor Yellow
        }
    }
}
Write-Host ""

# Abrir WhatsApp Web
if ($portaAberta) {
    Write-Host "[7] Abrindo WhatsApp Web..." -ForegroundColor Yellow
    Start-Process -FilePath $chromePath -ArgumentList "https://web.whatsapp.com"
    Write-Host "   [OK] WhatsApp Web sera aberto" -ForegroundColor Green
    Start-Sleep -Seconds 2
    Write-Host ""
}

# Resumo
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESUMO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($portaAberta) {
    Write-Host "[SUCESSO] Porta 9222 esta ABERTA!" -ForegroundColor Green
    Write-Host ""
    Write-Host "PROXIMOS PASSOS:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Faca login no WhatsApp Web (escaneie o QR Code)" -ForegroundColor White
    Write-Host "2. Aguarde fazer login completamente" -ForegroundColor White
    Write-Host "3. Execute: .\VERIFICAR_STATUS.ps1" -ForegroundColor White
} else {
    Write-Host "[ERRO] Nao foi possivel abrir a porta 9222" -ForegroundColor Red
    Write-Host ""
    Write-Host "TENTE:" -ForegroundColor Yellow
    Write-Host "1. Feche manualmente todas as janelas do Chrome" -ForegroundColor White
    Write-Host "2. Execute este script novamente: .\CORRIGIR_PORTA.ps1" -ForegroundColor White
    Write-Host "3. Verifique se ha outro programa usando a porta 9222" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
