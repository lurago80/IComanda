@echo off
echo ========================================
echo   Parando IComanda - Backend e Frontend
echo ========================================
echo.

echo [INFO] Parando processos do Backend...
taskkill /F /IM "IComanda.API.exe" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq *IComanda API - Backend*" >nul 2>&1

echo [INFO] Parando processos do Frontend...
taskkill /F /FI "WINDOWTITLE eq *IComanda - Frontend*" >nul 2>&1
REM Também tentar parar processos node relacionados ao React
for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq node.exe" /FO LIST ^| findstr /I "PID"') do (
    taskkill /F /PID %%a >nul 2>&1
)

echo.
echo [OK] Processos parados com sucesso!
echo.
timeout /t 2 /nobreak >nul

