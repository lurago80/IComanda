@echo off
chcp 65001 >nul
title Sistema iComanda - Diagnostico de Portas

echo ========================================
echo   DIAGNOSTICO DE PORTAS - ICOMANDA
echo ========================================
echo.

REM Executar script PowerShell
powershell.exe -ExecutionPolicy Bypass -File "%~dp0diagnostico-portas.ps1"

exit /b 0
