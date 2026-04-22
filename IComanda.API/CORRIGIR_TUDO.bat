@echo off
chcp 65001 >nul
echo ========================================
echo  CORRIGINDO TUDO - WhatsApp Web
echo ========================================
echo.

cd /d "%~dp0"

echo [PASSO 1] Fechando TODAS as janelas do Chrome...
taskkill /F /IM chrome.exe 2>nul
if %errorlevel% equ 0 (
    echo [OK] Chrome fechado com sucesso
) else (
    echo [INFO] Chrome nao estava rodando
)
timeout /t 2 /nobreak >nul
echo.

echo [PASSO 2] Verificando caminho do Chrome...
set CHROME_PATH=
if exist "C:\Program Files\Google\Chrome\Application\chrome.exe" (
    set "CHROME_PATH=C:\Program Files\Google\Chrome\Application\chrome.exe"
    echo [OK] Chrome encontrado
) else (
    if exist "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" (
        set "CHROME_PATH=C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
        echo [OK] Chrome encontrado
    ) else (
        echo [ERRO] Chrome nao encontrado!
        echo Por favor, instale o Google Chrome.
        pause
        exit /b 1
    )
)
echo.

echo [PASSO 3] Iniciando Chrome com remote debugging na porta 9222...
if defined CHROME_PATH (
    start "" "%CHROME_PATH%" --remote-debugging-port=9222 --disable-web-security --disable-features=IsolateOrigins,site-per-process
    echo [OK] Chrome iniciado
    timeout /t 3 /nobreak >nul
) else (
    echo [ERRO] Caminho do Chrome nao definido!
    pause
    exit /b 1
)
echo.

echo [PASSO 4] Verificando se a porta 9222 esta aberta...
netstat -ano | findstr "9222" >nul
if %errorlevel% equ 0 (
    echo [OK] Porta 9222 esta ABERTA!
) else (
    echo [AVISO] Porta 9222 ainda nao esta aberta. Aguardando...
    timeout /t 3 /nobreak >nul
    netstat -ano | findstr "9222" >nul
    if %errorlevel% equ 0 (
        echo [OK] Porta 9222 agora esta aberta!
    ) else (
        echo [ERRO] Porta 9222 nao foi aberta. Verifique se o Chrome iniciou corretamente.
    )
)
echo.

echo [PASSO 5] Abrindo WhatsApp Web...
start "" "%CHROME_PATH%" "https://web.whatsapp.com"
echo [OK] WhatsApp Web sera aberto em uma nova aba
timeout /t 2 /nobreak >nul
echo.

echo ========================================
echo  PROXIMOS PASSOS:
echo ========================================
echo.
echo 1. Faca o login no WhatsApp Web (escaneie o QR Code se necessario)
echo.
echo 2. Mantenha o WhatsApp Web aberto e conectado
echo.
echo 3. Inicie o backend (se ainda nao estiver rodando):
echo    cd IComanda.API
echo    dotnet run
echo.
echo 4. Verifique o status:
echo    http://localhost:65375/api/whatsapp/status
echo    (deve retornar: {"conectado":true})
echo.
echo 5. Execute o diagnostico novamente:
echo    .\DIAGNOSTICO_COMPLETO.ps1
echo.
echo ========================================
echo.
pause
