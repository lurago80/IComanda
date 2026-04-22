# Script de Deploy - Prepara arquivos para instalação no cliente
# Este script cria um pacote completo para deploy

param(
    [string]$OutputPath = ".\deploy-package",
    [switch]$IncludeFrontend = $true
)

Write-Host "📦 Preparando pacote de deploy para cliente..." -ForegroundColor Cyan
Write-Host ""

# Limpar pasta de deploy anterior
if (Test-Path $OutputPath) {
    Write-Host "🗑️  Removendo pacote anterior..." -ForegroundColor Yellow
    Remove-Item $OutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputPath | Out-Null
New-Item -ItemType Directory -Path "$OutputPath\backend" | Out-Null

# 1. Publicar Backend
Write-Host "📦 Publicando Backend..." -ForegroundColor Yellow
# Já estamos no diretório IComanda.API, não precisa mudar

$publishDir = ".\publish"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

dotnet publish -c Release -o $publishDir --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao publicar backend!" -ForegroundColor Red
    exit 1
}

# Copiar arquivos do backend
Copy-Item -Path "$publishDir\*" -Destination "$OutputPath\backend\" -Recurse -Force
Write-Host "✅ Backend publicado" -ForegroundColor Green

# 2. Build Frontend (se solicitado)
if ($IncludeFrontend) {
    Write-Host ""
    Write-Host "📦 Fazendo build do Frontend..." -ForegroundColor Yellow
    Set-Location icomanda-frontend

    if (-not (Test-Path "node_modules")) {
        Write-Host "📥 Instalando dependências..." -ForegroundColor Yellow
        npm install --silent
    }

    npm run build

    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Erro ao fazer build do frontend!" -ForegroundColor Red
        exit 1
    }

    # Copiar frontend
    New-Item -ItemType Directory -Path "$OutputPath\frontend" -Force | Out-Null
    Copy-Item -Path "build\*" -Destination "$OutputPath\frontend\" -Recurse -Force
    Write-Host "✅ Frontend buildado" -ForegroundColor Green
    Set-Location ..
}

# 3. Copiar arquivos de configuração e documentação
Write-Host ""
Write-Host "📄 Copiando documentação..." -ForegroundColor Yellow

Copy-Item -Path "GUIA_INSTALACAO.md" -Destination "$OutputPath\" -Force -ErrorAction SilentlyContinue
Copy-Item -Path "appsettings.json.example" -Destination "$OutputPath\backend\appsettings.json.example" -ErrorAction SilentlyContinue

# Copiar scripts de iniciar frontend (se existirem)
if (Test-Path ".\deploy-package\iniciar-frontend.ps1") {
    Copy-Item -Path ".\deploy-package\iniciar-frontend.ps1" -Destination "$OutputPath\iniciar-frontend.ps1" -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Script iniciar-frontend.ps1 copiado" -ForegroundColor Green
}
if (Test-Path ".\deploy-package\iniciar-frontend.bat") {
    Copy-Item -Path ".\deploy-package\iniciar-frontend.bat" -Destination "$OutputPath\iniciar-frontend.bat" -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Script iniciar-frontend.bat copiado" -ForegroundColor Green
}

# Copiar scripts de iniciar backend (se existirem)
if (Test-Path ".\deploy-package\iniciar-backend.ps1") {
    Copy-Item -Path ".\deploy-package\iniciar-backend.ps1" -Destination "$OutputPath\iniciar-backend.ps1" -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Script iniciar-backend.ps1 copiado" -ForegroundColor Green
}
if (Test-Path ".\deploy-package\iniciar-backend.bat") {
    Copy-Item -Path ".\deploy-package\iniciar-backend.bat" -Destination "$OutputPath\iniciar-backend.bat" -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Script iniciar-backend.bat copiado" -ForegroundColor Green
}

# 4. Criar script de instalação
Write-Host "📝 Criando scripts de instalação..." -ForegroundColor Yellow

$installScript = @"
@echo off
echo ========================================
echo   Instalacao IComanda API
echo ========================================
echo.

REM Verificar .NET
echo Verificando .NET Runtime...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERRO] .NET 8.0 Runtime nao encontrado!
    echo Por favor, instale o .NET 8.0 Runtime primeiro.
    echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [OK] .NET encontrado
echo.

REM Verificar appsettings.json
if not exist "appsettings.json" (
    echo [AVISO] appsettings.json nao encontrado!
    echo Por favor, configure o appsettings.json com a connection string do banco.
    echo.
    copy appsettings.json.example appsettings.json
    echo Arquivo appsettings.json.example copiado para appsettings.json
    echo EDITE O ARQUIVO appsettings.json ANTES DE CONTINUAR!
    pause
    exit /b 1
)

echo [OK] Configuracao encontrada
echo.

REM Criar diretorio de logs
if not exist "logs" mkdir logs

echo ========================================
echo   Instalacao concluida!
echo ========================================
echo.
echo Para executar a aplicacao:
echo   dotnet IComanda.API.dll
echo.
echo Ou execute: start-api.bat
echo.
pause
"@

$installScript | Out-File -FilePath "$OutputPath\backend\INSTALAR.bat" -Encoding ASCII

# Script para iniciar a API
$startScript = @"
@echo off
echo Iniciando IComanda API...
echo.
cd /d "%~dp0"
dotnet IComanda.API.dll
pause
"@

$startScript | Out-File -FilePath "$OutputPath\backend\start-api.bat" -Encoding ASCII

# Script para criar serviço Windows
$serviceScript = @"
@echo off
echo ========================================
echo   Instalacao Servico Windows IComandaAPI
echo ========================================
echo.

REM Verificar se esta rodando como administrador
net session >nul 2>&1
if errorlevel 1 (
    echo [ERRO] Este script precisa ser executado como Administrador!
    echo Clique com botao direito e selecione "Executar como administrador"
    pause
    exit /b 1
)

REM Obter caminho completo do executavel
set "SERVICE_DIR=%~dp0"
set "SERVICE_DLL=%SERVICE_DIR%IComanda.API.dll"

REM Verificar se o DLL existe
if not exist "%SERVICE_DLL%" (
    echo [ERRO] Arquivo IComanda.API.dll nao encontrado em: %SERVICE_DLL%
    echo Certifique-se de que a aplicacao foi publicada corretamente.
    pause
    exit /b 1
)

REM Verificar se dotnet esta instalado
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERRO] .NET Runtime nao encontrado!
    echo Por favor, instale o .NET 8.0 Runtime primeiro.
    pause
    exit /b 1
)

echo [OK] Arquivo encontrado: %SERVICE_DLL%
echo [OK] .NET Runtime encontrado
echo.

REM Verificar se o servico ja existe
sc query IComandaAPI >nul 2>&1
if not errorlevel 1 (
    echo [AVISO] Servico IComandaAPI ja existe!
    echo.
    set /p RESPOSTA="Deseja remover o servico existente? (S/N): "
    if /i "%RESPOSTA%"=="S" (
        echo Parando servico...
        net stop IComandaAPI >nul 2>&1
        echo Removendo servico...
        sc delete IComandaAPI
        timeout /t 2 >nul
    ) else (
        echo Instalacao cancelada.
        pause
        exit /b 0
    )
)

echo Criando servico Windows IComandaAPI...

REM Criar o servico usando dotnet (já que não é self-contained)
REM O caminho do dotnet precisa estar no PATH do sistema
set "DOTNET_PATH=dotnet"
set "SERVICE_BINPATH=%DOTNET_PATH% \"%SERVICE_DLL%\""

sc create IComandaAPI binPath= "%SERVICE_BINPATH%" start= auto DisplayName= "IComanda API Service"
if errorlevel 1 (
    echo [ERRO] Falha ao criar servico!
    echo.
    echo Tente criar manualmente com:
    echo   sc create IComandaAPI binPath= "dotnet \"%SERVICE_DLL%\""
    pause
    exit /b 1
)

sc description IComandaAPI "IComanda API Service - Sistema de Comandas e Vendas"

REM Configurar dependencia do Firebird (se existir)
sc query FirebirdServerDefaultInstance >nul 2>&1
if not errorlevel 1 (
    echo [INFO] Configurando dependencia do Firebird...
    sc config IComandaAPI depend= FirebirdServerDefaultInstance
)

REM Configurar para reiniciar automaticamente em caso de falha
sc failure IComandaAPI reset= 86400 actions= restart/5000/restart/5000/restart/5000

echo.
echo ========================================
echo   Servico instalado com sucesso!
echo ========================================
echo.
echo Para iniciar o servico:
echo   net start IComandaAPI
echo.
echo Para parar o servico:
echo   net stop IComandaAPI
echo.
echo Para remover o servico:
echo   net stop IComandaAPI
echo   sc delete IComandaAPI
echo.
echo Para verificar o status:
echo   sc query IComandaAPI
echo.
pause
"@

$serviceScript | Out-File -FilePath "$OutputPath\backend\install-service.bat" -Encoding ASCII

# Script para remover serviço
$uninstallScript = @"
@echo off
echo ========================================
echo   Remocao Servico Windows IComandaAPI
echo ========================================
echo.

REM Verificar se esta rodando como administrador
net session >nul 2>&1
if errorlevel 1 (
    echo [ERRO] Este script precisa ser executado como Administrador!
    pause
    exit /b 1
)

REM Verificar se o servico existe
sc query IComandaAPI >nul 2>&1
if errorlevel 1 (
    echo [AVISO] Servico IComandaAPI nao encontrado!
    pause
    exit /b 0
)

echo Parando servico...
net stop IComandaAPI
if errorlevel 1 (
    echo [AVISO] Nao foi possivel parar o servico (pode ja estar parado)
)

timeout /t 2 >nul

echo Removendo servico...
sc delete IComandaAPI
if errorlevel 1 (
    echo [ERRO] Falha ao remover o servico!
    pause
    exit /b 1
)

echo.
echo [OK] Servico removido com sucesso!
echo.
pause
"@

$uninstallScript | Out-File -FilePath "$OutputPath\backend\uninstall-service.bat" -Encoding ASCII

# Já estamos no diretório correto

Write-Host ""
Write-Host "🎉 Pacote de deploy criado com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "📁 Localizacao: $OutputPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "📦 Conteudo do pacote:" -ForegroundColor Yellow
Write-Host "   - backend\          (Backend publicado)" -ForegroundColor White
if ($IncludeFrontend) {
    Write-Host "   - frontend\         (Frontend buildado)" -ForegroundColor White
}
Write-Host "   - GUIA_INSTALACAO.md (Documentacao)" -ForegroundColor White
Write-Host ""
Write-Host "💡 Proximos passos:" -ForegroundColor Cyan
Write-Host "   1. Copie a pasta deploy-package para o PC do cliente" -ForegroundColor White
Write-Host "   2. Siga as instrucoes no GUIA_INSTALACAO.md" -ForegroundColor White
Write-Host "   3. Execute INSTALAR.bat na pasta backend" -ForegroundColor White
Write-Host ""

