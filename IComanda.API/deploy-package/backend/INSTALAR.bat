@echo off
echo ========================================
echo   Instalacao IComanda API
echo ========================================
echo.

REM Verificar .NET
echo Verificando .NET Runtime...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERRO] .NET 8.0 Runtime nao encontrado!
    echo Por favor, instale o .NET 8.0 Runtime primeiro.
    echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo [OK] .NET encontrado
echo.

REM Verificar appsettings.json
if not exist "appsettings.json" (
    echo [AVISO] appsettings.json nao encontrado!
    echo Por favor, configure o appsettings.json com a connection string do banco.
    echo.
    copy appsettings.json.example appsettings.json
    echo Arquivo appsettings.json.example copiado para appsettings.json
    echo EDITE O ARQUIVO appsettings.json ANTES DE CONTINUAR!
    pause
    exit /b 1
)

echo [OK] Configuracao encontrada
echo.

REM Criar diretorio de logs
if not exist "logs" mkdir logs

echo ========================================
echo   Instalacao concluida!
echo ========================================
echo.
echo Para executar a aplicacao:
echo   dotnet IComanda.API.dll
echo.
echo Ou execute: start-api.bat
echo.
pause
