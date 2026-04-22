@echo off
echo ========================================
echo Iniciando Chrome com Remote Debugging
echo ========================================
echo.
echo Este script inicia o Chrome com remote debugging habilitado
echo para que o sistema possa conectar ao WhatsApp Web ja aberto.
echo.
echo IMPORTANTE: Feche todas as janelas do Chrome antes de executar!
echo.
pause

REM Encontrar o caminho do Chrome
set CHROME_PATH=
if exist "C:\Program Files\Google\Chrome\Application\chrome.exe" (
    set CHROME_PATH=C:\Program Files\Google\Chrome\Application\chrome.exe
) else if exist "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" (
    set CHROME_PATH=C:\Program Files (x86)\Google\Chrome\Application\chrome.exe
) else (
    echo ERRO: Chrome nao encontrado!
    echo Por favor, instale o Google Chrome.
    pause
    exit /b 1
)

echo Chrome encontrado em: %CHROME_PATH%
echo.
echo Iniciando Chrome com remote debugging na porta 9222...
echo.

REM Fechar Chrome existente (se estiver aberto)
taskkill /F /IM chrome.exe 2>nul
timeout /t 2 /nobreak >nul

REM Iniciar Chrome com remote debugging
REM IMPORTANTE: Não usar --user-data-dir para não interferir com o perfil existente
echo Iniciando Chrome na porta 9222...
start "" "%CHROME_PATH%" --remote-debugging-port=9222 --disable-web-security --disable-features=IsolateOrigins,site-per-process

REM Aguardar Chrome iniciar
timeout /t 3 /nobreak >nul

REM Verificar se a porta está aberta
echo Verificando se a porta 9222 está aberta...
netstat -ano | findstr "9222" >nul
if errorlevel 1 (
    echo [AVISO] Porta 9222 nao foi aberta. Verifique se o Chrome iniciou corretamente.
) else (
    echo [OK] Porta 9222 esta aberta!
)

echo.
echo ========================================
echo Chrome iniciado com sucesso!
echo ========================================
echo.
echo PROXIMOS PASSOS:
echo.
echo 1. Abra o WhatsApp Web manualmente:
echo    https://web.whatsapp.com
echo.
echo 2. Faca o login (escaneie o QR Code se necessario)
echo.
echo 3. Mantenha o WhatsApp Web aberto
echo.
echo 4. Verifique se esta funcionando:
echo    http://localhost:65375/api/whatsapp/status
echo    (deve retornar: {"conectado":true})
echo.
echo ========================================
echo.
pause
