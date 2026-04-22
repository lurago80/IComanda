@echo off
chcp 65001 >nul
title Sistema iComanda - Inicializacao

echo ========================================
echo   SISTEMA ICOMANDA - INICIALIZACAO
echo ========================================
echo.

REM Executar script PowerShell
powershell.exe -ExecutionPolicy Bypass -File "%~dp0iniciar-sistema.ps1"

if errorlevel 1 (
    echo.
    echo ERRO ao iniciar o sistema!
    pause
    exit /b 1
)

exit /b 0
