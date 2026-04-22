# Script para testar autenticacao completa
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TESTE DE AUTENTICACAO COMPLETA" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Passo 1: Fazer login e capturar token
Write-Host "1. FAZENDO LOGIN..." -ForegroundColor Yellow
$loginResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"username":"INOVE","password":"1401"}' `
    -UseBasicParsing

$loginData = $loginResponse.Content | ConvertFrom-Json

Write-Host "Status: $($loginResponse.StatusCode)" -ForegroundColor Green
Write-Host "Token recebido (primeiros 30 chars): $($loginData.token.Substring(0,30))..." -ForegroundColor Green
Write-Host "Username: $($loginData.username)" -ForegroundColor Green
Write-Host "Role: $($loginData.role)" -ForegroundColor Green
Write-Host "Expira em: $($loginData.expiresIn) horas" -ForegroundColor Green
Write-Host "`n" 

# Passo 2: Usar o token para acessar rota protegida
Write-Host "2. TESTANDO ROTA PROTEGIDA (/api/vendas/abertas)..." -ForegroundColor Yellow

$headers = @{
    "Authorization" = "Bearer $($loginData.token)"
    "Content-Type" = "application/json"
}

try {
    $vendasResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/vendas/abertas" `
        -Method GET `
        -Headers $headers `
        -UseBasicParsing
    
    $vendas = $vendasResponse.Content | ConvertFrom-Json
    
    Write-Host "Status: $($vendasResponse.StatusCode)" -ForegroundColor Green
    Write-Host "Vendas abertas: $($vendas.Count)" -ForegroundColor Green
    Write-Host "`n" 
    
    # Passo 3: Explicar o que é o token
    Write-Host "3  O QUE E O TOKEN JWT?" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Gray
    Write-Host "Token JWT (JSON Web Token) e como um cracha digital" -ForegroundColor White
    Write-Host "   - Gerado apos login bem-sucedido" -ForegroundColor Gray
    Write-Host "   - Contem informacoes do usuario (ID, nome, permissoes)" -ForegroundColor Gray
    Write-Host "   - Expira apos 2 horas (configuravel)" -ForegroundColor Gray
    Write-Host "   - Deve ser enviado em TODAS as requisicoes protegidas" -ForegroundColor Gray
    Write-Host "`n" 
    Write-Host "Como funciona:" -ForegroundColor White
    Write-Host "   1. Usuario faz login -> Backend valida senha" -ForegroundColor Gray
    Write-Host "   2. Backend gera Token JWT + RefreshToken" -ForegroundColor Gray
    Write-Host "   3. Frontend salva no localStorage" -ForegroundColor Gray
    Write-Host "   4. Frontend envia em cada request: Authorization: Bearer <token>" -ForegroundColor Gray
    Write-Host "   5. Backend valida token e permite acesso" -ForegroundColor Gray
    Write-Host "`n" 
    Write-Host "Expiracao:" -ForegroundColor White
    Write-Host "   - JWT Token: 2 horas (apos isso, precisa renovar)" -ForegroundColor Gray
    Write-Host "   - RefreshToken: 7 dias (para renovar JWT sem fazer login novamente)" -ForegroundColor Gray
    Write-Host "`n" 
    Write-Host "SE O TOKEN NAO FOR ENVIADO:" -ForegroundColor Red
    Write-Host "   - Backend retorna 401 Unauthorized" -ForegroundColor Gray
    Write-Host "   - Mensagem: Requires an authenticated user" -ForegroundColor Gray
    Write-Host "   - Frontend deve redirecionar para login" -ForegroundColor Gray
    Write-Host "`n" 
    
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "AUTENTICACAO FUNCIONANDO PERFEITAMENTE!" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Cyan
    
} catch {
    Write-Host "ERRO AO ACESSAR ROTA PROTEGIDA!" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "Mensagem: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`n" 
    
    if ($_.Exception.Response.StatusCode.value__ -eq 401) {
        Write-Host "DIAGNOSTICO:" -ForegroundColor Yellow
        Write-Host "   - O token NAO esta sendo enviado corretamente" -ForegroundColor Gray
        Write-Host "   - OU o token esta invalido/expirado" -ForegroundColor Gray
        Write-Host "   - Verifique se o frontend esta salvando no localStorage" -ForegroundColor Gray
        Write-Host "   - Verifique se o axios interceptor esta funcionando" -ForegroundColor Gray
    }
}
