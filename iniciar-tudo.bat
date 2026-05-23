@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ========================================
echo   🚀 IComanda - Iniciando Tudo
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

echo [❌ ERRO] Script executado na pasta errada.
echo [ERRO] Pasta detectada: %CURRENT_ROOT%
echo [ERRO] Pastas corretas: %EXPECTED_ROOT_A%  OU  %EXPECTED_ROOT_B%
echo [AÇÃO] Execute este script somente na pasta oficial do projeto.
echo.
pause
exit /b 1

:ROOT_OK

echo [INFO] Diretório do projeto: %CD%
echo.

REM Verificar se dotnet está instalado
where dotnet >nul 2>&1
if errorlevel 1 (
    echo [❌ ERRO] .NET SDK não encontrado!
    echo Por favor, instale o .NET 8.0 SDK.
    echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

REM Verificar se npm está instalado
where npm >nul 2>&1
if errorlevel 1 (
    echo [❌ ERRO] npm não encontrado!
    echo Por favor, instale o Node.js 18+.
    echo Download: https://nodejs.org/
    echo.
    pause
    exit /b 1
)

REM Parar processos anteriores se existirem
echo [INFO] Verificando processos anteriores...
taskkill /F /IM "IComanda.API.exe" >nul 2>&1
REM Liberar portas 65375, 3000 e 3001 pelo PID dono da porta (mais seguro que matar todos os dotnet.exe)
powershell -Command "Get-NetTCPConnection -LocalPort 65375,3000,3001 -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }" 2>nul
timeout /t 2 /nobreak >nul

REM Aguardar porta 65375 ficar realmente livre (max 15s)
echo [INFO] Aguardando porta 65375 ficar livre...
set "_tries=0"
:WAIT_PORT_FREE
powershell -Command "exit (Get-NetTCPConnection -LocalPort 65375 -ErrorAction SilentlyContinue).Count" 2>nul
if %errorlevel% EQU 0 goto :PORT_IS_FREE
set /a _tries+=1
if %_tries% GEQ 15 (
    echo [AVISO] Porta 65375 ainda ocupada apos 15s. Continuando mesmo assim...
    goto :PORT_IS_FREE
)
timeout /t 1 /nobreak >nul
goto :WAIT_PORT_FREE
:PORT_IS_FREE

echo [OK] Processos anteriores finalizados. Portas liberadas.
echo.

REM Verificar se o backend existe
if not exist "IComanda.API\IComanda.API.csproj" (
    echo [❌ ERRO] Projeto backend não encontrado!
    echo Certifique-se de estar na raiz do projeto.
    pause
    exit /b 1
)

REM Verificar se o frontend existe
if not exist "IComanda.API\icomanda-frontend\package.json" (
    echo [⚠️ AVISO] Frontend não encontrado em IComanda.API\icomanda-frontend\
    echo Iniciando apenas o backend...
    goto :START_BACKEND_ONLY
)

REM Verificar se node_modules existe, se não, instalar dependências
if not exist "IComanda.API\icomanda-frontend\node_modules" (
    echo [INFO] Instalando dependências do frontend...
    echo Isso pode levar alguns minutos na primeira vez...
    echo.
    pushd "IComanda.API\icomanda-frontend"
    call npm install
    if errorlevel 1 (
        echo [❌ ERRO] Falha ao instalar dependências do frontend.
        popd
        pause
        exit /b 1
    )
    popd
    echo [✅] Dependências instaladas
    echo.
)

:START_BACKEND
echo ========================================
echo   📦 Iniciando BACKEND
echo ========================================
echo.
echo [INFO] Porta: 65375
echo [INFO] Swagger: http://localhost:65375/swagger
echo [INFO] Health Check: http://localhost:65375/health
echo.

set "BACKEND_DIR=%CD%\IComanda.API"
start "🚀 IComanda - Backend" /D "%BACKEND_DIR%" cmd /k "echo Compilando backend... && dotnet build --configuration Release -v minimal && echo. && echo Iniciando Backend... && dotnet bin\Release\net8.0\IComanda.API.dll"

REM Aguardar backend iniciar (build pode levar 30-60s; verificar health em loop)
echo [INFO] Aguardando backend compilar e iniciar (pode levar até 60s)...
set BACKEND_READY=0
for /L %%i in (1,1,12) do (
    if "!BACKEND_READY!"=="0" (
        timeout /t 5 /nobreak >nul
        curl -s http://localhost:65375/health >nul 2>&1
        if not errorlevel 1 (
            set BACKEND_READY=1
            echo [✅] Backend está respondendo!
        ) else (
            echo [INFO] Aguardando... ^(tentativa %%i/12^)
        )
    )
)
if "!BACKEND_READY!"=="0" (
    echo [⚠️ AVISO] Backend demorou mais que o esperado, mas continuando...
)
echo.

:START_FRONTEND
echo ========================================
echo   💻 Iniciando FRONTEND
echo ========================================
echo.
echo [INFO] Porta: 3000
echo [INFO] URL: http://localhost:3000
echo.

set "FRONTEND_DIR=%CD%\IComanda.API\icomanda-frontend"
start "🎨 IComanda - Frontend" /D "%FRONTEND_DIR%" cmd /k "echo Iniciando Frontend... && set DISABLE_ESLINT_PLUGIN=true && set PORT=3000 && set BROWSER=none && npm start"

echo [INFO] Aguardando frontend iniciar...
timeout /t 5 /nobreak >nul
echo.

REM Iniciar WhatsApp Baileys (sem Docker) se a pasta existir
set "BAILEYS_DIR=%CD%\icomanda-whatsapp-baileys"
if exist "%BAILEYS_DIR%\package.json" (
    echo ========================================
    echo   📱 Iniciando WhatsApp ^(Baileys^)
    echo ========================================
    echo.
    echo [INFO] Porta: 3001
    echo [INFO] Escaneie o QR no modal "Conectar WhatsApp" do iComanda
    echo.
    if not exist "%BAILEYS_DIR%\node_modules" (
        echo [INFO] Instalando dependencias do Baileys...
        pushd "%BAILEYS_DIR%"
        call npm install
        popd
        echo [OK] Dependencias instaladas
        echo.
    )
    start "📱 IComanda - WhatsApp Baileys" /D "%BAILEYS_DIR%" cmd /k "echo Iniciando WhatsApp Baileys... && npm run dev"
    echo [OK] WhatsApp Baileys iniciado em nova janela
    echo.
    timeout /t 2 /nobreak >nul
) else (
    echo [AVISO] Pasta icomanda-whatsapp-baileys nao encontrada. WhatsApp ^(Baileys^) nao sera iniciado.
    echo.
)

REM Abrir navegador uma unica vez na porta 3000 ^(iComanda^)
echo [INFO] O navegador sera aberto em 20 segundos em http://localhost:3000
start /B cmd /c "timeout /t 20 /nobreak >nul && start http://localhost:3000"

goto :SUCCESS

:START_BACKEND_ONLY
echo ========================================
echo   📦 Iniciando APENAS BACKEND
echo ========================================
echo.
set "BACKEND_DIR=%CD%\IComanda.API"
start "🚀 IComanda - Backend" /D "%BACKEND_DIR%" cmd /k "echo Restaurando pacotes NuGet... && dotnet restore && echo Compilando backend... && dotnet build --configuration Release -v minimal && echo. && echo Iniciando Backend... && dotnet bin\Release\net8.0\IComanda.API.dll"
timeout /t 3 /nobreak >nul
goto :SUCCESS

:SUCCESS
echo.
echo ========================================
echo   ✅ Aplicação Iniciada com Sucesso!
echo ========================================
echo.
echo 📍 Acessos:
echo.
echo   Backend API:    http://localhost:65375
echo   Swagger:        http://localhost:65375/swagger
echo   Health Check:   http://localhost:65375/health
if exist "IComanda.API\icomanda-frontend\package.json" (
    echo   Frontend:        http://localhost:3000
)
if exist "icomanda-whatsapp-baileys\package.json" (
    echo   WhatsApp Baileys: http://localhost:3001 ^(envio direto, sem Docker^)
)
echo.
echo 📝 Notas:
echo   - As janelas do Backend, Frontend e WhatsApp Baileys continuarao abertas
echo   - Para parar, feche as janelas ou pressione Ctrl+C em cada uma
echo   - Os logs aparecerao nas respectivas janelas
echo   - WhatsApp Baileys: escaneie o QR no modal "Conectar WhatsApp" do iComanda
echo.
echo ========================================
echo.
echo Pressione qualquer tecla para fechar esta janela...
echo As aplicacoes continuarao rodando
pause >nul

