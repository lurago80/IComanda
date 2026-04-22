# Script para parar processo e compilar backend
Write-Host "Parando processos IComanda.API..." -ForegroundColor Yellow

# Parar todos os processos IComanda.API
Get-Process -Name "IComanda.API" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host "Parando processo PID: $($_.Id)" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force
}

Start-Sleep -Seconds 2

Write-Host "Compilando backend..." -ForegroundColor Cyan
Set-Location "IComanda.API"
dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Build concluído com sucesso!" -ForegroundColor Green
} else {
    Write-Host "`n❌ Erro no build. Verifique os erros acima." -ForegroundColor Red
}

Set-Location ..

