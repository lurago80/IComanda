# ========================================
# CORRIGINDO TUDO - WhatsApp Web
# ========================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CORRIGINDO TUDO - WhatsApp Web" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Mudar para o diretório do script
Set-Location $PSScriptRoot

# PASSO 1: Fechar Chrome
Write-Host "[PASSO 1] Fechando TODAS as janelas do Chrome..." -ForegroundColor Yellow
$chromeProcesses = Get-Process -Name "chrome" -ErrorAction SilentlyContinue
if ($chromeProcesses) {
    Stop-Process -Name "chrome" -Force -ErrorAction SilentlyContinue
    Write-Host "[OK] Chrome fechado com sucesso" -ForegroundColor Green
    Start-Sleep -Seconds 2
} else {
    Write-Host "[INFO] Chrome nao estava rodando" -ForegroundColor Gray
}
Write-Host ""

# PASSO 2: Verificar caminho do Chrome
Write-Host "[PASSO 2] Verificando caminho do Chrome..." -ForegroundColor Yellow
$chromePath = $null

$paths = @(
    "C:\Program Files\Google\Chrome\Application\chrome.exe",
    "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
)

foreach ($path in $paths) {
    if (Test-Path $path) {
        $chromePath = $path
        Write-Host "[OK] Chrome encontrado em: $chromePath" -ForegroundColor Green
        break
    }
}

if (-not $chromePath) {
    Write-Host "[ERRO] Chrome nao encontrado!" -ForegroundColor Red
    Write-Host "Por favor, instale o Google Chrome." -ForegroundColor Yellow
    Read-Host "Pressione Enter para sair"
    exit 1
}
Write-Host ""

# PASSO 3: Iniciar Chrome com remote debugging
Write-Host "[PASSO 3] Iniciando Chrome com remote debugging na porta 9222..." -ForegroundColor Yellow
$chromeArgs = @(
    "--remote-debugging-port=9222",
    "--disable-web-security",
    "--disable-features=IsolateOrigins,site-per-process"
)

Start-Process -FilePath $chromePath -ArgumentList $chromeArgs
Write-Host "[OK] Chrome iniciado" -ForegroundColor Green
Start-Sleep -Seconds 3
Write-Host ""

# PASSO 4: Verificar porta 9222
Write-Host "[PASSO 4] Verificando se a porta 9222 esta aberta..." -ForegroundColor Yellow
$portaAberta = $false
$tentativas = 0
$maxTentativas = 5

while (-not $portaAberta -and $tentativas -lt $maxTentativas) {
    $connection = Test-NetConnection -ComputerName localhost -Port 9222 -WarningAction SilentlyContinue -InformationLevel Quiet
    if ($connection) {
        Write-Host "[OK] Porta 9222 esta ABERTA!" -ForegroundColor Green
        $portaAberta = $true
    } else {
        $tentativas++
        if ($tentativas -lt $maxTentativas) {
            Write-Host "[AVISO] Porta 9222 ainda nao esta aberta. Aguardando... ($tentativas/$maxTentativas)" -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        } else {
            Write-Host "[ERRO] Porta 9222 nao foi aberta. Verifique se o Chrome iniciou corretamente." -ForegroundColor Red
        }
    }
}
Write-Host ""

# PASSO 5: Abrir WhatsApp Web
Write-Host "[PASSO 5] Abrindo WhatsApp Web..." -ForegroundColor Yellow
Start-Process -FilePath $chromePath -ArgumentList "https://web.whatsapp.com"
Write-Host "[OK] WhatsApp Web sera aberto em uma nova aba" -ForegroundColor Green
Start-Sleep -Seconds 2
Write-Host ""

# Resumo
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  PROXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Faca o login no WhatsApp Web (escaneie o QR Code se necessario)" -ForegroundColor White
Write-Host ""
Write-Host "2. Mantenha o WhatsApp Web aberto e conectado" -ForegroundColor White
Write-Host ""
Write-Host "3. Inicie o backend (se ainda nao estiver rodando):" -ForegroundColor White
Write-Host "   cd IComanda.API" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "4. Verifique o status:" -ForegroundColor White
Write-Host "   http://localhost:65375/api/whatsapp/status" -ForegroundColor Gray
Write-Host "   (deve retornar: {`"conectado`":true})" -ForegroundColor Gray
Write-Host ""
Write-Host "5. Execute o diagnostico novamente:" -ForegroundColor White
Write-Host "   .\DIAGNOSTICO_COMPLETO.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Read-Host "Pressione Enter para sair"
