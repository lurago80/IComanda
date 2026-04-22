@echo off
REM ============================================
REM VERIFICADOR DE PRÉ-REQUISITOS ICOMANDA
REM ============================================

echo.
echo ========================================
echo   VERIFICADOR DE PRE-REQUISITOS
echo ========================================
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0VERIFICAR_PRE_REQUISITOS.ps1"

pause
