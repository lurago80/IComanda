@echo off
REM Script para commit e push automático ao salvar arquivos no projeto

:loop
REM Aguarda 10 segundos entre verificações
timeout /t 10 >nul

git add .
git diff --cached --quiet
if %errorlevel%==1 (
    git commit -m "Atualização automática: alterações detectadas"
    git push origin HEAD
)
goto loop
