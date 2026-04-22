@echo off
chcp 65001 >nul
echo.
echo ========================================
echo   🛑 Parando IComanda
echo ========================================
echo.

echo [INFO] Parando processos do Backend...
taskkill /F /IM "IComanda.API.exe" >nul 2>&1
taskkill /F /IM "dotnet.exe" /FI "WINDOWTITLE eq *IComanda*" >nul 2>&1

echo [INFO] Parando processos do Frontend...
for /f "tokens=2" %%a in ('tasklist /FI "IMAGENAME eq node.exe" /FO LIST ^| findstr /C:"PID:"') do (
    taskkill /F /PID %%a >nul 2>&1
)

timeout /t 2 /nobreak >nul

echo.
echo [✅] Processos finalizados
echo.
echo Pressione qualquer tecla para fechar...
pause >nul

