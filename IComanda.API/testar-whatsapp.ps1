# Script para testar conexão do WhatsApp Web

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Teste de Conexão WhatsApp Web" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se porta 9222 está aberta
Write-Host "1. Verificando porta 9222..." -ForegroundColor Yellow
$porta9222 = netstat -ano | findstr "9222"
if ($porta9222) {
    Write-Host "   ✅ Porta 9222 está aberta" -ForegroundColor Green
    Write-Host "   $porta9222" -ForegroundColor Gray
} else {
    Write-Host "   ❌ Porta 9222 NÃO está aberta" -ForegroundColor Red
    Write-Host "   Execute: iniciar-chrome-whatsapp.bat" -ForegroundColor Yellow
}
Write-Host ""

# Verificar status do backend
Write-Host "2. Verificando status do backend..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/status" -Method Get -ErrorAction Stop
    if ($response.conectado) {
        Write-Host "   ✅ WhatsApp Web está CONECTADO" -ForegroundColor Green
    } else {
        Write-Host "   ❌ WhatsApp Web NÃO está conectado" -ForegroundColor Red
        Write-Host "   Verifique se:" -ForegroundColor Yellow
        Write-Host "   - Chrome está rodando com remote debugging (porta 9222)" -ForegroundColor Yellow
        Write-Host "   - WhatsApp Web está aberto: https://web.whatsapp.com" -ForegroundColor Yellow
        Write-Host "   - Você fez login no WhatsApp Web" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Erro ao verificar status: $_" -ForegroundColor Red
    Write-Host "   Verifique se o backend está rodando" -ForegroundColor Yellow
}
Write-Host ""

# Tentar inicializar
Write-Host "3. Tentando inicializar WhatsApp Web..." -ForegroundColor Yellow
try {
    $initResponse = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/inicializar" -Method Post -ErrorAction Stop
    Write-Host "   ✅ Inicialização: $($initResponse.mensagem)" -ForegroundColor Green
    Start-Sleep -Seconds 5
    
    # Verificar status novamente
    $response = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/status" -Method Get -ErrorAction Stop
    if ($response.conectado) {
        Write-Host "   ✅ Agora está CONECTADO!" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️ Ainda não conectado. Verifique os logs do backend" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ❌ Erro ao inicializar: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Fim do Teste" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Se ainda não estiver conectado:" -ForegroundColor Yellow
Write-Host "1. Feche todas as janelas do Chrome" -ForegroundColor White
Write-Host "2. Execute: iniciar-chrome-whatsapp.bat" -ForegroundColor White
Write-Host "3. Abra: https://web.whatsapp.com" -ForegroundColor White
Write-Host "4. Faça o login" -ForegroundColor White
Write-Host "5. Execute este script novamente" -ForegroundColor White
Write-Host ""
