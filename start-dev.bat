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

REM Bloquear execução fora da pasta oficial deste workspace
set "EXPECTED_ROOT_A=C:\VS_Code\icomanda"
set "EXPECTED_ROOT_B=C:\iCOMANDA"
for %%I in ("%CD%") do set "CURRENT_ROOT=%%~fI"
if "%CURRENT_ROOT:~-1%"=="\" set "CURRENT_ROOT=%CURRENT_ROOT:~0,-1%"
if "%EXPECTED_ROOT_A:~-1%"=="\" set "EXPECTED_ROOT_A=%EXPECTED_ROOT_A:~0,-1%"
if "%EXPECTED_ROOT_B:~-1%"=="\" set "EXPECTED_ROOT_B=%EXPECTED_ROOT_B:~0,-1%"

if /I "%CURRENT_ROOT%"=="%EXPECTED_ROOT_A%" goto :ROOT_OK
if /I "%CURRENT_ROOT%"=="%EXPECTED_ROOT_B%" goto :ROOT_OK

echo [ERRO] Script executado na pasta errada.
echo [ERRO] Pasta detectada: %CURRENT_ROOT%
echo [ERRO] Pastas corretas: %EXPECTED_ROOT_A%  OU  %EXPECTED_ROOT_B%
echo [ACAO] Execute este script somente na pasta oficial do projeto.
pause
exit /b 1

:ROOT_OK

REM Verificar se estamos no diretório raiz do projeto
if not exist "IComanda.API\IComanda.API.csproj" (
    echo [ERRO] Este script deve ser executado da pasta raiz do projeto
    echo Diretorio atual: %CD%
    echo Por favor, navegue ate a pasta raiz (onde esta a pasta IComanda.API) e execute novamente.
    pause
    exit /b 1
)

echo [INFO] Diretorio do projeto: %CD%
echo.

REM Parar processos anteriores e liberar portas 3000 e 3001
echo [INFO] Verificando processos anteriores...
taskkill /F /IM "IComanda.API.exe" >nul 2>&1
for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq node.exe" /FO LIST 2^>nul ^| findstr /C:"PID:"') do taskkill /F /PID %%a >nul 2>&1
timeout /t 2 /nobreak >nul
powershell -Command "Get-NetTCPConnection -LocalPort 3000,3001 -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }" 2>nul
timeout /t 1 /nobreak >nul

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
set "BACKEND_DIR=%~dp0IComanda.API"
if not exist "%BACKEND_DIR%\IComanda.API.csproj" (
    echo [ERRO] Arquivo IComanda.API.csproj nao encontrado em IComanda.API\
    pause
    exit /b 1
)
start "IComanda API - Backend" /D "%BACKEND_DIR%" cmd /k "dotnet run --configuration Release"
timeout /t 5 /nobreak >nul

REM Verificar se o frontend existe
if not exist "IComanda.API\icomanda-frontend\package.json" (
    echo [AVISO] Frontend nao encontrado em IComanda.API\icomanda-frontend\
    echo Backend iniciado. Pressione qualquer tecla para fechar...
    pause
    exit /b 0
)

REM Verificar se node_modules existe, se nao, instalar dependencias
if not exist "IComanda.API\icomanda-frontend\node_modules" (
    echo [INFO] Instalando dependencias do frontend...
    pushd "IComanda.API\icomanda-frontend"
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
set "FRONTEND_DIR=%~dp0IComanda.API\icomanda-frontend"
start "IComanda - Frontend" /D "%FRONTEND_DIR%" cmd /k "set DISABLE_ESLINT_PLUGIN=true && npm start"

echo.
echo [INFO] Navegador sera aberto em 12 segundos em http://localhost:3000
start /B cmd /c "timeout /t 12 /nobreak >nul & start http://localhost:3000"
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

