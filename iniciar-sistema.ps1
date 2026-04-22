# Script para iniciar IComanda - Metodo robusto
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$AllowedRoots = @("C:\VS_Code\icomanda", "C:\iCOMANDA")
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$CurrentRoot = [System.IO.Path]::GetFullPath($ScriptDir).TrimEnd('\\')
$ApiDir = Join-Path $CurrentRoot "IComanda.API"
$FrontendDir = Join-Path $ApiDir "icomanda-frontend"

if (-not ($AllowedRoots | Where-Object { $CurrentRoot.Equals($_, [System.StringComparison]::OrdinalIgnoreCase) })) {
    Write-Host "[ERRO] Script executado na pasta errada." -ForegroundColor Red
    Write-Host "[ERRO] Pasta detectada: $CurrentRoot" -ForegroundColor Red
    Write-Host "[ERRO] Pastas corretas: $($AllowedRoots -join '  OU  ')" -ForegroundColor Yellow
    Write-Host "[ACAO] Execute este script somente na pasta oficial do projeto." -ForegroundColor Yellow
    exit 1
}

# Ler a porta do backend do appsettings.json
$BackendPort = 65375
try {
    $appSettings = Get-Content (Join-Path $ApiDir "appsettings.json") -Raw | ConvertFrom-Json
    if ($appSettings.Kestrel.Port) {
        $BackendPort = $appSettings.Kestrel.Port
    }
} catch { }

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "INICIANDO SISTEMA ICOMANDA" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Passo 1: Parar processos
Write-Host "`n[1/5] Parando processos anteriores..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
    $_.MainWindowTitle -like "*icomanda*" -or (Get-NetTCPConnection -OwningProcess $_.Id -ErrorAction SilentlyContinue | Where-Object LocalPort -eq 3000)
} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3
Write-Host "Processos parados" -ForegroundColor Green

# Passo 2: Limpar arquivos bloqueados
Write-Host "`n[2/5] Limpando arquivos de build..." -ForegroundColor Yellow
Set-Location $ApiDir
if (Test-Path "bin\Release\net8.0\IComanda.API.dll") {
    Remove-Item "bin\Release\net8.0\IComanda.API.dll" -Force -ErrorAction SilentlyContinue
}
Write-Host "Arquivos limpos" -ForegroundColor Green

# Passo 3: Compilar backend
Write-Host "`n[3/5] Compilando backend..." -ForegroundColor Yellow
$buildOutput = dotnet build --configuration Release --no-incremental --verbosity quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO ao compilar!" -ForegroundColor Red
    Write-Host $buildOutput
    pause
    exit 1
}
Write-Host "Build concluido com sucesso" -ForegroundColor Green

# Passo 4: Iniciar backend
Write-Host "`n[4/5] Iniciando backend (porta $BackendPort)..." -ForegroundColor Yellow
Start-Process -FilePath "dotnet" -ArgumentList "bin\Release\net8.0\IComanda.API.dll" -WorkingDirectory $ApiDir -WindowStyle Normal
Start-Sleep -Seconds 8

# Verificar se backend esta rodando
$backendOk = $false
for ($i = 1; $i -le 3; $i++) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$BackendPort/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "Backend iniciado com sucesso na porta $BackendPort!" -ForegroundColor Green
            $backendOk = $true
            break
        }
    } catch {
        if ($i -lt 3) {
            Write-Host "  Aguardando backend iniciar... (tentativa $i/3)" -ForegroundColor Yellow
            Start-Sleep -Seconds 5
        }
    }
}
if (-not $backendOk) {
    Write-Host "AVISO: Backend nao respondeu na porta $BackendPort. Verifique a janela do servidor." -ForegroundColor Yellow
}

# Passo 5: Iniciar frontend
Write-Host "`n[5/5] Iniciando frontend React (porta 3000)..." -ForegroundColor Yellow
if (Test-Path $FrontendDir) {
    Start-Process -FilePath "cmd" -ArgumentList "/k", "cd /d `"$FrontendDir`" && npm start" -WindowStyle Normal
    Start-Sleep -Seconds 5
    Write-Host "Frontend iniciado! Aguarde o browser abrir automaticamente..." -ForegroundColor Green
} else {
    Write-Host "AVISO: Diretorio do frontend nao encontrado em: $FrontendDir" -ForegroundColor Yellow
}

Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "SISTEMA INICIADO!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan

Write-Host "`nURLs:" -ForegroundColor White
Write-Host "  Backend:  http://localhost:$BackendPort" -ForegroundColor Gray
Write-Host "  Frontend: http://localhost:3000" -ForegroundColor Gray
Write-Host "  Swagger:  http://localhost:$BackendPort/swagger" -ForegroundColor Gray

Write-Host "`nCredenciais de teste:" -ForegroundColor White
Write-Host "  Username: INOVE" -ForegroundColor Gray
Write-Host "  Senha:    1401" -ForegroundColor Gray

Write-Host "`nPara parar: Execute parar-tudo.bat" -ForegroundColor Yellow
Write-Host ""
