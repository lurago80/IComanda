@echo off
chcp 65001 >nul
echo.
echo ========================================
echo   Iniciando Frontend IComanda
echo ========================================
echo.

REM Obter o diretório do script
cd /d "%~dp0"

REM Executar o script PowerShell
powershell.exe -ExecutionPolicy Bypass -File "%~dp0iniciar-frontend.ps1"

if errorlevel 1 (
    echo.
    echo [ERRO] Falha ao executar o script PowerShell
    echo.
    pause
    exit /b 1
)

pause

