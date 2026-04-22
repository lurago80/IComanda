# =============================================================================
# gerar-pacote-cliente.ps1
# Gera o pacote completo do IComanda pronto para instalar em C:\IComanda\
# =============================================================================
# Uso: Abra o PowerShell como Administrador e execute:
#   powershell -ExecutionPolicy Bypass -File gerar-pacote-cliente.ps1
# =============================================================================

$ErrorActionPreference = "Stop"
$HOST_ROOT    = $PSScriptRoot                                  # C:\VS_Code\icomanda
$API_DIR      = Join-Path $HOST_ROOT "IComanda.API"
$FRONTEND_DIR = Join-Path $API_DIR "icomanda-frontend"
$WWWROOT_DIR  = Join-Path $API_DIR "wwwroot"
$PUBLISH_DIR  = Join-Path $API_DIR "publish"
$DEPLOY_DEST  = "C:\IComanda"

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  IComanda - Gerando Pacote para Cliente" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# -----------------------------------------------------------------------
# PASSO 1 — Verificar pré-requisitos
# -----------------------------------------------------------------------
Write-Host "[1/6] Verificando pré-requisitos..." -ForegroundColor Yellow

try { $dotnetVersion = (dotnet --version 2>&1); Write-Host "       .NET: $dotnetVersion" -ForegroundColor Gray }
catch { Write-Host "ERRO: .NET SDK nao encontrado. Instale o .NET 8 SDK." -ForegroundColor Red; exit 1 }

try { $nodeVersion = (node --version 2>&1); Write-Host "       Node: $nodeVersion" -ForegroundColor Gray }
catch { Write-Host "ERRO: Node.js nao encontrado. Instale o Node.js 18+." -ForegroundColor Red; exit 1 }

Write-Host "       OK" -ForegroundColor Green
Write-Host ""

# -----------------------------------------------------------------------
# PASSO 2 — Build do Frontend React
# -----------------------------------------------------------------------
Write-Host "[2/6] Fazendo build do Frontend (React / npm run build)..." -ForegroundColor Yellow

Set-Location $FRONTEND_DIR

if (-not (Test-Path "node_modules")) {
    Write-Host "       Instalando dependencias npm (aguarde)..." -ForegroundColor Gray
    npm install --silent
    if ($LASTEXITCODE -ne 0) { Write-Host "ERRO no npm install!" -ForegroundColor Red; exit 1 }
}

$env:CI = "false"   # evita que warnings virem erros no CRA
npm run build
if ($LASTEXITCODE -ne 0) { Write-Host "ERRO no npm run build!" -ForegroundColor Red; exit 1 }
Write-Host "       Frontend buildado em: icomanda-frontend\build\" -ForegroundColor Green
Write-Host ""

# -----------------------------------------------------------------------
# PASSO 3 — Copiar build do React para wwwroot/
# -----------------------------------------------------------------------
Write-Host "[3/6] Copiando frontend para wwwroot/..." -ForegroundColor Yellow

$buildDir = Join-Path $FRONTEND_DIR "build"
if (-not (Test-Path $buildDir)) {
    Write-Host "ERRO: Pasta build\ nao encontrada!" -ForegroundColor Red; exit 1
}

# Limpar wwwroot (menos .gitkeep)
if (Test-Path $WWWROOT_DIR) {
    Get-ChildItem $WWWROOT_DIR -Exclude ".gitkeep" | Remove-Item -Recurse -Force
}
New-Item -ItemType Directory -Path $WWWROOT_DIR -Force | Out-Null

# Copiar tudo do build/ para wwwroot/
Copy-Item -Path "$buildDir\*" -Destination $WWWROOT_DIR -Recurse -Force
Write-Host "       wwwroot/ populado com $(( Get-ChildItem $WWWROOT_DIR -Recurse -File ).Count) arquivos" -ForegroundColor Green
Write-Host ""

# -----------------------------------------------------------------------
# PASSO 4 — Publicar o Backend .NET
# -----------------------------------------------------------------------
Write-Host "[4/6] Publicando Backend (.NET publish)..." -ForegroundColor Yellow

Set-Location $API_DIR

# Limpar pasta publish anterior
if (Test-Path $PUBLISH_DIR) { Remove-Item $PUBLISH_DIR -Recurse -Force }

dotnet publish -c Release -o $PUBLISH_DIR --self-contained false --nologo -v quiet
if ($LASTEXITCODE -ne 0) { Write-Host "ERRO no dotnet publish!" -ForegroundColor Red; exit 1 }
Write-Host "       Backend publicado em: publish\" -ForegroundColor Green
Write-Host ""

# -----------------------------------------------------------------------
# PASSO 5 — Montar pasta C:\IComanda\
# -----------------------------------------------------------------------
Write-Host "[5/6] Montando pasta de deploy: $DEPLOY_DEST ..." -ForegroundColor Yellow

# Criar estrutura de pastas
$dirs = @(
    "$DEPLOY_DEST\Backend",
    "$DEPLOY_DEST\Dados",
    "$DEPLOY_DEST\logs"
)
foreach ($d in $dirs) {
    New-Item -ItemType Directory -Path $d -Force | Out-Null
}

# Copiar publish/ → Backend/
Copy-Item -Path "$PUBLISH_DIR\*" -Destination "$DEPLOY_DEST\Backend" -Recurse -Force

# Marcador para a pasta Dados
if (-not (Test-Path "$DEPLOY_DEST\Dados\COLOQUE_O_BANCO_AQUI.txt")) {
    Set-Content -Path "$DEPLOY_DEST\Dados\COLOQUE_O_BANCO_AQUI.txt" -Encoding UTF8 -Value @"
Coloque aqui o arquivo DADOSG5.FDB (banco de dados Firebird).
O caminho configurado no appsettings.json e: C:\iComanda\Dados\DADOSG5.FDB
"@
}

Write-Host "       Estrutura criada!" -ForegroundColor Green
Write-Host ""

# -----------------------------------------------------------------------
# PASSO 6 — Criar scripts de inicialização no root do deploy
# -----------------------------------------------------------------------
Write-Host "[6/6] Criando scripts de inicializacao..." -ForegroundColor Yellow

# --- iniciar.bat ---
Set-Content -Path "$DEPLOY_DEST\iniciar.bat" -Encoding Default -Value @'
@echo off
chcp 65001 >nul
title IComanda - Sistema

echo.
echo ========================================
echo   IComanda - Iniciando Sistema
echo ========================================
echo.

REM Matar processos anteriores
taskkill /F /IM "dotnet.exe" >nul 2>&1
timeout /t 2 /nobreak >nul

REM Iniciar backend (que tambem serve o frontend)
echo [INFO] Iniciando servidor na porta 65375...
start "IComanda API" /D "%~dp0Backend" cmd /k "dotnet IComanda.API.dll"

echo.
echo [INFO] Aguardando iniciar...
timeout /t 6 /nobreak >nul

echo.
echo ========================================
echo   Sistema iniciado!
echo ========================================
echo.
echo   Acesse pelo navegador:
echo   http://localhost:65375
echo.
echo   Ou pelo celular/tablet na mesma rede:
echo   http://SEU_IP_LOCAL:65375
echo.
echo   Para parar: execute parar.bat
echo.
pause
'@

# --- parar.bat ---
Set-Content -Path "$DEPLOY_DEST\parar.bat" -Encoding Default -Value @'
@echo off
echo Parando IComanda...
taskkill /F /IM "dotnet.exe" >nul 2>&1
echo Sistema parado.
timeout /t 2 /nobreak >nul
'@

# --- LEIAME.txt ---
$dataBuild = Get-Date -Format "dd/MM/yyyy HH:mm"
Set-Content -Path "$DEPLOY_DEST\LEIAME.txt" -Encoding UTF8 -Value @"
===================================================
  IComanda - Guia de Uso
  Gerado em: $dataBuild
===================================================

PRE-REQUISITOS:
  - .NET 8 Runtime instalado
    (Download: https://dotnet.microsoft.com/download/dotnet/8.0)
  - Firebird Server 2.5 instalado e rodando
  - Banco de dados DADOSG5.FDB disponivel

ESTRUTURA:
  C:\IComanda\
  |-- Backend\          <- Sistema (nao alterar)
  |-- Dados\            <- Coloque o DADOSG5.FDB aqui
  |-- logs\             <- Logs de execucao (automatico)
  |-- iniciar.bat       <- INICIA o sistema
  |-- parar.bat         <- PARA o sistema
  |-- LEIAME.txt        <- Este arquivo

COMO USAR:
  1. Coloque o arquivo DADOSG5.FDB na pasta Dados\
  2. Verifique o appsettings.json se necessario (veja abaixo)
  3. Execute iniciar.bat (duplo-clique ou como Administrador)
  4. Abra o navegador em: http://localhost:65375

CONFIGURACOES (Backend\appsettings.json):
  - Banco de dados: ConnectionStrings.Firebird
    Caminho padrao: C:\iComanda\Dados\DADOSG5.FDB
  - Porta do sistema: Kestrel.Port (padrao: 65375)
  - Impressora: Impressao.ImpressoraLocal.Nome

ACESSO PELO CELULAR (mesma rede Wi-Fi):
  - Descubra o IP do computador servidor: ipconfig
  - No celular, acesse: http://IP_DO_SERVIDOR:65375
  - Exemplo: http://192.168.1.100:65375

PROBLEMAS COMUNS:
  - "Banco nao encontrado": verifique o caminho no appsettings.json
  - "Porta em uso": execute parar.bat e tente novamente
  - "Erro de autenticacao Firebird": verifique se o servico Firebird esta rodando
"@

Write-Host "       Scripts criados!" -ForegroundColor Green
Write-Host ""

# -----------------------------------------------------------------------
# CONCLUIDO
# -----------------------------------------------------------------------
Write-Host "============================================================" -ForegroundColor Green
Write-Host "  PACOTE GERADO COM SUCESSO!" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Local: $DEPLOY_DEST" -ForegroundColor White
Write-Host ""
Write-Host "  Para iniciar o sistema no cliente:" -ForegroundColor Cyan
Write-Host "    1. Copie a pasta C:\IComanda\ para o computador do cliente" -ForegroundColor White
Write-Host "    2. Execute C:\IComanda\iniciar.bat" -ForegroundColor White
Write-Host "    3. Acesse http://localhost:65375" -ForegroundColor White
Write-Host ""

Set-Location $HOST_ROOT
