@echo off
REM ============================================
REM INSTALADOR ICOMANDA - Backend e Frontend
REM ============================================
REM Wrapper em batch para o script PowerShell
REM ============================================

echo.
echo ========================================
echo   INSTALADOR ICOMANDA
echo ========================================
echo.

REM Verificar se PowerShell está disponível
powershell -Command "Get-Command powershell" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERRO: PowerShell nao encontrado!
    echo Por favor, instale o PowerShell e tente novamente.
    pause
    exit /b 1
)

REM Executar script PowerShell
powershell -ExecutionPolicy Bypass -File "%~dp0INSTALAR.ps1" %*

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERRO: Instalacao falhou!
    pause
    exit /b 1
)

exit /b 0
