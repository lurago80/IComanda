@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

REM Obter o diretório do script e ir para a raiz esperada
set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

REM Bloquear execução fora da pasta oficial deste workspace
set "EXPECTED_ROOT_A=C:\VS_Code\icomanda"
set "EXPECTED_ROOT_B=C:\iCOMANDA"
for %%I in ("%CD%") do set "CURRENT_ROOT=%%~fI"
if "%CURRENT_ROOT:~-1%"=="\" set "CURRENT_ROOT=%CURRENT_ROOT:~0,-1%"
if "%EXPECTED_ROOT_A:~-1%"=="\" set "EXPECTED_ROOT_A=%EXPECTED_ROOT_A:~0,-1%"
if "%EXPECTED_ROOT_B:~-1%"=="\" set "EXPECTED_ROOT_B=%EXPECTED_ROOT_B:~0,-1%"

if /I "%CURRENT_ROOT%"=="%EXPECTED_ROOT_A%" goto :ROOT_OK
if /I "%CURRENT_ROOT%"=="%EXPECTED_ROOT_B%" goto :ROOT_OK

echo [ERRO] Script executado na pasta errada.
echo [ERRO] Pasta detectada: %CURRENT_ROOT%
echo [ERRO] Pastas corretas: %EXPECTED_ROOT_A%  OU  %EXPECTED_ROOT_B%
echo [ACAO] Execute este script somente na pasta oficial do projeto.
echo.
pause
exit /b 1

:ROOT_OK

echo =====================================
echo INICIANDO SISTEMA ICOMANDA
echo =====================================
echo.

echo [1/4] Parando processos anteriores...
taskkill /F /IM dotnet.exe >nul 2>&1
timeout /t 3 >nul

echo [2/4] Limpando arquivos bloqueados...
cd /d "%SCRIPT_DIR%IComanda.API"
if exist "bin\Release\net8.0\IComanda.API.dll" (
    del /F /Q "bin\Release\net8.0\IComanda.API.dll" >nul 2>&1
)
timeout /t 1 >nul

echo [3/4] Compilando backend...
dotnet build --configuration Release --no-incremental >nul 2>&1
if errorlevel 1 (
    echo ERRO ao compilar! Tente novamente.
    pause
    exit /b 1
)

echo [4/4] Iniciando Backend...
start "IComanda API" cmd /k "dotnet bin\Release\net8.0\IComanda.API.dll"
timeout /t 5 >nul

echo.
echo =====================================
echo SISTEMA INICIADO COM SUCESSO!
echo =====================================
echo.
echo Backend: http://localhost:5000
echo Frontend: http://localhost:3000
echo Swagger: http://localhost:5000/swagger
echo.
echo Usuario de teste:
echo   - Username: INOVE
echo   - Senha: 1401
echo.
echo Para acessar: Abra http://localhost:3000
echo Para parar: Execute parar-tudo.bat
echo.
pause
