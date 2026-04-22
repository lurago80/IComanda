@echo off
chcp 65001 >nul
title Sistema iComanda - Parar

echo ========================================
echo   SISTEMA ICOMANDA - PARAR
echo ========================================
echo.

REM Executar script PowerShell
powershell.exe -ExecutionPolicy Bypass -File "%~dp0parar-sistema.ps1"

exit /b 0
