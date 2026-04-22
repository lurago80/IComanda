@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ========================================
echo   Iniciando IComanda - Backend e Frontend
echo ========================================
echo.

REM Obter o diretório do script
set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

REM Verificar se estamos no diretório correto
if not exist "IComanda.API.csproj" (
    echo [ERRO] Este script deve ser executado da pasta IComanda.API
    echo Diretorio atual: %CD%
    echo Por favor, navegue ate a pasta IComanda.API e execute novamente.
    pause
    exit /b 1
)

echo [INFO] Diretorio do projeto: %CD%
echo.

REM Parar processos anteriores se existirem
echo [INFO] Verificando processos anteriores...
taskkill /F /IM "IComanda.API.exe" >nul 2>&1
taskkill /F /IM "node.exe" /FI "WINDOWTITLE eq *react-scripts*" >nul 2>&1
timeout /t 2 /nobreak >nul

REM Verificar se dotnet esta instalado
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERRO] .NET SDK nao encontrado. Por favor, instale o .NET 8.0 SDK.
    pause
    exit /b 1
)

REM Verificar se npm esta instalado
where npm >nul 2>&1
if errorlevel 1 (
    echo [ERRO] npm nao encontrado. Por favor, instale o Node.js.
    pause
    exit /b 1
)

REM Iniciar Backend
echo [INFO] Iniciando Backend na porta 65375...
set "BACKEND_DIR=%~dp0"
start "IComanda API - Backend" /D "%BACKEND_DIR%" cmd /k "dotnet run --configuration Release"
timeout /t 5 /nobreak >nul

REM Verificar se o frontend existe
if not exist "icomanda-frontend\package.json" (
    echo [AVISO] Frontend nao encontrado em icomanda-frontend\
    echo Backend iniciado. Pressione qualquer tecla para fechar...
    pause
    exit /b 0
)

REM Verificar se node_modules existe, se nao, instalar dependencias
if not exist "icomanda-frontend\node_modules" (
    echo [INFO] Instalando dependencias do frontend...
    pushd "icomanda-frontend"
    call npm install
    if errorlevel 1 (
        echo [ERRO] Falha ao instalar dependencias do frontend.
        popd
        pause
        exit /b 1
    )
    popd
)

REM Iniciar Frontend
echo [INFO] Iniciando Frontend na porta 3000...
set "FRONTEND_DIR=%~dp0icomanda-frontend"
start "IComanda - Frontend" /D "%FRONTEND_DIR%" cmd /k "npm start"

echo.
echo ========================================
echo   Aplicacao iniciada com sucesso!
echo ========================================
echo.
echo Backend: http://localhost:65375
echo Frontend: http://localhost:3000
echo Swagger: http://localhost:65375/swagger
echo.
echo Pressione qualquer tecla para fechar esta janela...
echo (As janelas do Backend e Frontend continuarao abertas)
pause >nul

