# ========================================
# SCRIPT DE INICIALIZACAO DO SISTEMA ICOMANDA
# Verifica e libera portas antes de iniciar
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  INICIANDO SISTEMA ICOMANDA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validar pasta correta de execução
$AllowedRoots = @("C:\VS_Code\icomanda\IComanda.API", "C:\iCOMANDA\IComanda.API")
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$CurrentRoot = [System.IO.Path]::GetFullPath($ScriptDir).TrimEnd('\\')

if (-not ($AllowedRoots | Where-Object { $CurrentRoot.Equals($_, [System.StringComparison]::OrdinalIgnoreCase) })) {
    Write-Host "[ERRO] Script executado na pasta errada." -ForegroundColor Red
    Write-Host "[ERRO] Pasta detectada: $CurrentRoot" -ForegroundColor Red
    Write-Host "[ERRO] Pastas corretas: $($AllowedRoots -join '  OU  ')" -ForegroundColor Yellow
    Write-Host "[ACAO] Execute este script somente na pasta oficial do projeto." -ForegroundColor Yellow
    exit 1
}

# ========================================
# CONFIGURACAO DE PORTAS
# ========================================
$PORTAS = @{
    Backend = 5000
    Frontend = 3000
    WhatsApp = 3001
    Firebird = 3050
}

Write-Host "[1/6] Verificando portas em uso..." -ForegroundColor Yellow
Write-Host ""

# ========================================
# FUNCAO: Verificar e matar processo em porta
# ========================================
function Stop-ProcessOnPort {
    param(
        [int]$Port,
        [string]$Name,
        [bool]$IsDatabase = $false
    )
    
    Write-Host "  Verificando porta $Port ($Name)..." -NoNewline
    
    $connections = netstat -ano | Select-String ":$Port\s" | Select-String "LISTENING"
    
    if ($connections) {
        $processIds = @()
        foreach ($line in $connections) {
            if ($line -match '\s+(\d+)\s*$') {
                $processIds += $matches[1]
            }
        }
        
        $processIds = $processIds | Select-Object -Unique
        
        if ($processIds.Count -gt 0) {
            Write-Host " OCUPADA (PID: $($processIds -join ', '))" -ForegroundColor Red
            
            if ($IsDatabase) {
                Write-Host "    [FIREBIRD] Porta do banco de dados - NAO sera encerrado" -ForegroundColor Yellow
                return $false
            }
            
            foreach ($processId in $processIds) {
                try {
                    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
                    if ($process) {
                        Write-Host "    Encerrando processo: $($process.ProcessName) (PID: $processId)..." -ForegroundColor Yellow
                        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                        Write-Host "    Processo encerrado com sucesso" -ForegroundColor Green
                    }
                } catch {
                    Write-Host "    Erro ao encerrar PID $processId : $_" -ForegroundColor Red
                }
            }
            return $true
        }
    }
    
    Write-Host " LIVRE" -ForegroundColor Green
    return $false
}

# ========================================
# VERIFICAR E LIBERAR PORTAS
# ========================================
$portasLiberadas = 0

if (Stop-ProcessOnPort -Port $PORTAS.Backend -Name "Backend API") {
    $portasLiberadas++
}

if (Stop-ProcessOnPort -Port $PORTAS.Frontend -Name "Frontend React") {
    $portasLiberadas++
}

if (Stop-ProcessOnPort -Port $PORTAS.WhatsApp -Name "WhatsApp Baileys") {
    $portasLiberadas++
}

Stop-ProcessOnPort -Port $PORTAS.Firebird -Name "Firebird Database" -IsDatabase $true

if ($portasLiberadas -gt 0) {
    Write-Host ""
    Write-Host "  Aguardando liberacao de portas..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
}

Write-Host ""

# ========================================
# MATAR PROCESSOS DOTNET E NODE RESTANTES
# ========================================
Write-Host "[2/6] Encerrando processos dotnet e node..." -ForegroundColor Yellow

$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "  Encontrados $($dotnetProcesses.Count) processo(s) dotnet - encerrando..." -ForegroundColor Yellow
    Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "  Processos dotnet encerrados" -ForegroundColor Green
} else {
    Write-Host "  Nenhum processo dotnet encontrado" -ForegroundColor Green
}

$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    Write-Host "  Encontrados $($nodeProcesses.Count) processo(s) node - encerrando..." -ForegroundColor Yellow
    Stop-Process -Name "node" -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "  Processos node encerrados" -ForegroundColor Green
} else {
    Write-Host "  Nenhum processo node encontrado" -ForegroundColor Green
}

Write-Host ""

# ========================================
# LIMPAR ARQUIVOS DE BUILD
# ========================================
Write-Host "[3/6] Limpando arquivos de build..." -ForegroundColor Yellow

Set-Location -Path $CurrentRoot

if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin" -ErrorAction SilentlyContinue
    Write-Host "  Pasta bin removida" -ForegroundColor Green
}

if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj" -ErrorAction SilentlyContinue
    Write-Host "  Pasta obj removida" -ForegroundColor Green
}

Write-Host ""

# ========================================
# COMPILAR BACKEND
# ========================================
Write-Host "[4/6] Compilando backend..." -ForegroundColor Yellow

$buildOutput = dotnet build --configuration Release --no-incremental 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "  Build concluido com sucesso" -ForegroundColor Green
} else {
    Write-Host "  ERRO ao compilar backend:" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Red
    Write-Host ""
    Write-Host "Pressione qualquer tecla para sair..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

Write-Host ""

# ========================================
# INICIAR BACKEND
# ========================================
Write-Host "[5/6] Iniciando backend..." -ForegroundColor Yellow

$backendProcess = Start-Process -FilePath "dotnet" `
    -ArgumentList "bin\Release\net8.0\IComanda.API.dll" `
    -WorkingDirectory $CurrentRoot `
    -WindowStyle Normal `
    -PassThru

if ($backendProcess) {
    Write-Host "  Backend iniciado (PID: $($backendProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "  ERRO ao iniciar backend" -ForegroundColor Red
    exit 1
}

Write-Host ""

# ========================================
# VERIFICAR SAUDE DO BACKEND
# ========================================
Write-Host "[6/6] Verificando saude do backend..." -ForegroundColor Yellow

Start-Sleep -Seconds 5

try {
    $healthCheck = Invoke-WebRequest -Uri "http://localhost:$($PORTAS.Backend)/health" -UseBasicParsing -TimeoutSec 10
    
    if ($healthCheck.StatusCode -eq 200) {
        Write-Host "  Backend ONLINE e respondendo" -ForegroundColor Green
        Write-Host "  Status: $($healthCheck.StatusCode)" -ForegroundColor Green
        Write-Host "  Resposta: $($healthCheck.Content)" -ForegroundColor Green
    } else {
        Write-Host "  Backend respondeu com status inesperado: $($healthCheck.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  AVISO: Nao foi possivel verificar saude do backend" -ForegroundColor Yellow
    Write-Host "  Erro: $_" -ForegroundColor Yellow
    Write-Host "  O backend pode estar iniciando ainda..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  SISTEMA ICOMANDA INICIADO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "URLs do Sistema:" -ForegroundColor White
Write-Host "  Backend API: http://localhost:$($PORTAS.Backend)" -ForegroundColor Cyan
Write-Host "  Swagger: http://localhost:$($PORTAS.Backend)/swagger" -ForegroundColor Cyan
Write-Host "  Health Check: http://localhost:$($PORTAS.Backend)/health" -ForegroundColor Cyan
Write-Host ""
Write-Host "Comandos Uteis:" -ForegroundColor White
Write-Host "  Parar sistema: Stop-Process -Name 'dotnet' -Force" -ForegroundColor Yellow
Write-Host "  Ver logs: Get-Content $CurrentRoot\logs\*.log -Tail 50 -Wait" -ForegroundColor Yellow
Write-Host "  Testar autenticacao: .\testar-autenticacao-completa.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "Pressione qualquer tecla para fechar esta janela..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
