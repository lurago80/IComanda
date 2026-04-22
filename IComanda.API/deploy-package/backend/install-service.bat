@echo off
echo ========================================
echo   Instalacao Servico Windows IComandaAPI
echo ========================================
echo.

REM Verificar se esta rodando como administrador
net session >nul 2>&1
if errorlevel 1 (
    echo [ERRO] Este script precisa ser executado como Administrador!
    echo Clique com botao direito e selecione "Executar como administrador"
    pause
    exit /b 1
)

REM Obter caminho completo do executavel
set "SERVICE_DIR=%~dp0"
set "SERVICE_DLL=%SERVICE_DIR%IComanda.API.dll"

REM Verificar se o DLL existe
if not exist "%SERVICE_DLL%" (
    echo [ERRO] Arquivo IComanda.API.dll nao encontrado em: %SERVICE_DLL%
    echo Certifique-se de que a aplicacao foi publicada corretamente.
    pause
    exit /b 1
)

REM Verificar se dotnet esta instalado
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERRO] .NET Runtime nao encontrado!
    echo Por favor, instale o .NET 8.0 Runtime primeiro.
    pause
    exit /b 1
)

echo [OK] Arquivo encontrado: %SERVICE_DLL%
echo [OK] .NET Runtime encontrado
echo.

REM Verificar se o servico ja existe
sc query IComandaAPI >nul 2>&1
if not errorlevel 1 (
    echo [AVISO] Servico IComandaAPI ja existe!
    echo.
    set /p RESPOSTA="Deseja remover o servico existente? (S/N): "
    if /i "%RESPOSTA%"=="S" (
        echo Parando servico...
        net stop IComandaAPI >nul 2>&1
        echo Removendo servico...
        sc delete IComandaAPI
        timeout /t 2 >nul
    ) else (
        echo Instalacao cancelada.
        pause
        exit /b 0
    )
)

echo Criando servico Windows IComandaAPI...

REM Criar o servico usando dotnet (j? que n?o ? self-contained)
REM O caminho do dotnet precisa estar no PATH do sistema
set "DOTNET_PATH=dotnet"
set "SERVICE_BINPATH=%DOTNET_PATH% \"%SERVICE_DLL%\""

sc create IComandaAPI binPath= "%SERVICE_BINPATH%" start= auto DisplayName= "IComanda API Service"
if errorlevel 1 (
    echo [ERRO] Falha ao criar servico!
    echo.
    echo Tente criar manualmente com:
    echo   sc create IComandaAPI binPath= "dotnet \"%SERVICE_DLL%\""
    pause
    exit /b 1
)

sc description IComandaAPI "IComanda API Service - Sistema de Comandas e Vendas"

REM Configurar dependencia do Firebird (se existir)
sc query FirebirdServerDefaultInstance >nul 2>&1
if not errorlevel 1 (
    echo [INFO] Configurando dependencia do Firebird...
    sc config IComandaAPI depend= FirebirdServerDefaultInstance
)

REM Configurar para reiniciar automaticamente em caso de falha
sc failure IComandaAPI reset= 86400 actions= restart/5000/restart/5000/restart/5000

echo.
echo ========================================
echo   Servico instalado com sucesso!
echo ========================================
echo.
echo Para iniciar o servico:
echo   net start IComandaAPI
echo.
echo Para parar o servico:
echo   net stop IComandaAPI
echo.
echo Para remover o servico:
echo   net stop IComandaAPI
echo   sc delete IComandaAPI
echo.
echo Para verificar o status:
echo   sc query IComandaAPI
echo.
pause
