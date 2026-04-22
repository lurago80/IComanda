# Apaga a sessão do WhatsApp para começar do zero (novo QR).
# Rode quando o serviço ficar em loop "connection errored" / "not logged in".
$authDir = Join-Path $PSScriptRoot "auth_info_baileys"
if (Test-Path $authDir) {
    Remove-Item -Recurse -Force $authDir
    Write-Host "Pasta auth_info_baileys apagada. Rode: npm run dev" -ForegroundColor Green
} else {
    Write-Host "Pasta auth_info_baileys nao existe." -ForegroundColor Yellow
}
