@echo off
echo ====================================
echo  Iniciando IComanda API (HTTP only)
echo ====================================
echo.

set ASPNETCORE_URLS=http://localhost:65375
set ASPNETCORE_ENVIRONMENT=Development

cd /d "%~dp0"
dotnet run --no-launch-profile

pause

