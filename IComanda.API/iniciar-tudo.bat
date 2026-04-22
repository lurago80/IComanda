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
taskkill /F /IM "dotnet.exe" /FI "WINDOWTITLE eq *IComanda*" >nul 2>&1
for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq node.exe" /FO LIST ^| findstr /C:"PID:"') do (
    taskkill /F /PID %%a >nul 2>&1
)
timeout /t 2 /nobreak >nul

echo [✅] Processos anteriores finalizados
echo.

REM Verificar se o frontend existe
if not exist "icomanda-frontend\package.json" (
    echo [⚠️ AVISO] Frontend não encontrado em icomanda-frontend\
    echo Iniciando apenas o backend...
    goto :START_BACKEND_ONLY
)

REM Verificar se node_modules existe, se não, instalar dependências
if not exist "icomanda-frontend\node_modules" (
    echo [INFO] Instalando dependências do frontend...
    echo Isso pode levar alguns minutos na primeira vez...
    echo.
    pushd "icomanda-frontend"
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

REM Iniciar Chrome com WhatsApp Web (para envio automático)
echo [INFO] Iniciando Chrome com WhatsApp Web...
if exist "iniciar-chrome-whatsapp.bat" (
    echo [✅] Executando iniciar-chrome-whatsapp.bat...
    start "🌐 Chrome WhatsApp" /MIN cmd /c "iniciar-chrome-whatsapp.bat"
    timeout /t 3 /nobreak >nul
    echo [✅] Chrome iniciado com remote debugging
    echo [INFO] Abra o WhatsApp Web manualmente: https://web.whatsapp.com
    echo.
) else (
    echo [⚠️ AVISO] Arquivo iniciar-chrome-whatsapp.bat não encontrado
    echo [INFO] Para envio automático de WhatsApp, execute iniciar-chrome-whatsapp.bat manualmente
    echo.
)

start "🚀 IComanda - Backend" /D "%CD%" cmd /k "echo Restaurando pacotes NuGet... && dotnet restore && echo Iniciando Backend... && dotnet run --configuration Release"

REM Aguardar backend iniciar
echo [INFO] Aguardando backend iniciar...
timeout /t 8 /nobreak >nul

REM Verificar se o backend está respondendo
echo [INFO] Verificando se o backend está respondendo...
curl -s http://localhost:65375/health >nul 2>&1
if errorlevel 1 (
    echo [⚠️ AVISO] Backend ainda não está respondendo, mas continuando...
) else (
    echo [✅] Backend está respondendo!
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

set "FRONTEND_DIR=%CD%\icomanda-frontend"
start "🎨 IComanda - Frontend" /D "%FRONTEND_DIR%" cmd /k "echo Iniciando Frontend... && npm start"

echo [INFO] Aguardando frontend iniciar...
timeout /t 5 /nobreak >nul
echo.

goto :SUCCESS

:START_BACKEND_ONLY
echo ========================================
echo   📦 Iniciando APENAS BACKEND
echo ========================================
echo.

REM Iniciar Chrome com WhatsApp Web (para envio automático)
echo [INFO] Iniciando Chrome com WhatsApp Web...
if exist "iniciar-chrome-whatsapp.bat" (
    echo [✅] Executando iniciar-chrome-whatsapp.bat...
    start "🌐 Chrome WhatsApp" /MIN cmd /c "iniciar-chrome-whatsapp.bat"
    timeout /t 3 /nobreak >nul
    echo [✅] Chrome iniciado com remote debugging
    echo [INFO] Abra o WhatsApp Web manualmente: https://web.whatsapp.com
    echo.
) else (
    echo [⚠️ AVISO] Arquivo iniciar-chrome-whatsapp.bat não encontrado
    echo [INFO] Para envio automático de WhatsApp, execute iniciar-chrome-whatsapp.bat manualmente
    echo.
)

start "🚀 IComanda - Backend" /D "%CD%" cmd /k "echo Restaurando pacotes NuGet... && dotnet restore && echo Iniciando Backend... && dotnet run --configuration Release"
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
if exist "icomanda-frontend\package.json" (
    echo   Frontend:        http://localhost:3000
)
echo.
echo 📝 Notas:
echo   - As janelas do Backend e Frontend continuarão abertas
echo   - Para parar, feche as janelas ou pressione Ctrl+C em cada uma
echo   - Os logs aparecerão nas respectivas janelas
echo   - Chrome com WhatsApp Web foi iniciado para envio automático
echo   - Abra o WhatsApp Web: https://web.whatsapp.com
echo.
echo ========================================
echo.
echo Pressione qualquer tecla para fechar esta janela...
echo (As aplicações continuarão rodando)
pause >nul

