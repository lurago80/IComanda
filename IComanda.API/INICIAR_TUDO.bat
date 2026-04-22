@echo off
chcp 65001 >nul
echo ========================================
echo  INICIAR TUDO - Sistema Completo
echo ========================================
echo.

cd /d "%~dp0"

echo [PASSO 1] Fechando Chrome existente...
taskkill /F /IM chrome.exe 2>nul
timeout /t 2 /nobreak >nul
echo.

echo [PASSO 2] Iniciando Chrome com remote debugging...
call "%~dp0iniciar-chrome-whatsapp.bat"
timeout /t 3 /nobreak >nul
echo.

echo [PASSO 3] Verificando se backend esta rodando...
netstat -ano | findstr ":65375" >nul
if %errorlevel% equ 0 (
    echo [OK] Backend ja esta rodando na porta 65375
) else (
    echo [INFO] Backend nao esta rodando. Iniciando...
    echo.
    echo IMPORTANTE: O backend sera iniciado em uma nova janela.
    echo Mantenha essa janela aberta enquanto usa o sistema.
    echo.
    start "Backend IComanda" cmd /k "cd /d %~dp0 && dotnet run"
    echo [OK] Backend iniciado. Aguarde alguns segundos para inicializar...
    timeout /t 5 /nobreak >nul
)
echo.

echo [PASSO 4] Verificando frontend...
cd icomanda-frontend
if exist "package.json" (
    echo [INFO] Frontend encontrado
    echo [INFO] Para iniciar o frontend, execute em outra janela:
    echo        cd icomanda-frontend
    echo        npm start
) else (
    echo [AVISO] Frontend nao encontrado
)
cd ..
echo.

echo ========================================
echo  SISTEMA INICIADO!
echo ========================================
echo.
echo PROXIMOS PASSOS:
echo.
echo 1. Abra o WhatsApp Web (se nao abriu automaticamente):
echo    https://web.whatsapp.com
echo.
echo 2. Faca o login (escaneie o QR Code)
echo.
echo 3. Aguarde o backend terminar de inicializar
echo    (procure por "Now listening on: http://localhost:65375")
echo.
echo 4. Verifique o status:
echo    http://localhost:65375/api/whatsapp/status
echo.
echo 5. Execute o diagnostico:
echo    .\DIAGNOSTICO_COMPLETO.ps1
echo.
echo ========================================
echo.
pause
