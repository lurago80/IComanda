@echo off
echo ========================================
echo   Remocao Servico Windows IComandaAPI
echo ========================================
echo.

REM Verificar se esta rodando como administrador
net session >nul 2>&1
if errorlevel 1 (
    echo [ERRO] Este script precisa ser executado como Administrador!
    pause
    exit /b 1
)

REM Verificar se o servico existe
sc query IComandaAPI >nul 2>&1
if errorlevel 1 (
    echo [AVISO] Servico IComandaAPI nao encontrado!
    pause
    exit /b 0
)

echo Parando servico...
net stop IComandaAPI
if errorlevel 1 (
    echo [AVISO] Nao foi possivel parar o servico (pode ja estar parado)
)

timeout /t 2 >nul

echo Removendo servico...
sc delete IComandaAPI
if errorlevel 1 (
    echo [ERRO] Falha ao remover o servico!
    pause
    exit /b 1
)

echo.
echo [OK] Servico removido com sucesso!
echo.
pause
