@echo off
echo ========================================
echo  DIAGNOSTICO COMPLETO - WhatsApp Web
echo ========================================
echo.
echo Executando diagnostico completo...
echo.

cd /d "%~dp0"

powershell -ExecutionPolicy Bypass -File "DIAGNOSTICO_COMPLETO.ps1"

echo.
echo ========================================
echo  Fim do Diagnostico
echo ========================================
echo.
pause
