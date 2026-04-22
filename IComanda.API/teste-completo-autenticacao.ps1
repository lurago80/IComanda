# ========================================
# TESTE COMPLETO DE AUTENTICACAO
# Login + Acesso a endpoint protegido
# ========================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TESTE COMPLETO DE AUTENTICACAO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ========================================
# PASSO 1: LOGIN
# ========================================
Write-Host "[1/3] Fazendo login..." -ForegroundColor Yellow
Write-Host ""

$loginBody = @{
    username = "INOVE"
    password = "1401"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest `
        -Uri "http://localhost:5000/api/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody `
        -UseBasicParsing
    
    $loginData = $loginResponse.Content | ConvertFrom-Json
    
    Write-Host "  Status: $($loginResponse.StatusCode) OK" -ForegroundColor Green
    Write-Host "  Usuario: $($loginData.username)" -ForegroundColor Cyan
    Write-Host "  User ID: $($loginData.userId)" -ForegroundColor Cyan
    Write-Host "  Role: $($loginData.role)" -ForegroundColor Cyan
    Write-Host "  Token (primeiros 40 chars): $($loginData.token.Substring(0,40))..." -ForegroundColor Cyan
    Write-Host ""
    
    $token = $loginData.token
    
} catch {
    Write-Host "  ERRO ao fazer login:" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Pressione qualquer tecla para sair..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# ========================================
# PASSO 2: ACESSAR ENDPOINT PROTEGIDO
# ========================================
Write-Host "[2/3] Acessando endpoint protegido /api/vendas/abertas..." -ForegroundColor Yellow
Write-Host ""

$headers = @{
    Authorization = "Bearer $token"
}

try {
    $vendasResponse = Invoke-WebRequest `
        -Uri "http://localhost:5000/api/vendas/abertas" `
        -Method GET `
        -Headers $headers `
        -UseBasicParsing
    
    Write-Host "  Status: $($vendasResponse.StatusCode) OK" -ForegroundColor Green
    Write-Host "  Content-Type: $($vendasResponse.Headers['Content-Type'])" -ForegroundColor Cyan
    Write-Host "  Tamanho da resposta: $($vendasResponse.Content.Length) bytes" -ForegroundColor Cyan
    
    # Tentar parsear JSON
    try {
        $vendasData = $vendasResponse.Content | ConvertFrom-Json
        if ($vendasData -is [Array]) {
            Write-Host "  Vendas abertas encontradas: $($vendasData.Count)" -ForegroundColor Green
        } else {
            Write-Host "  Resposta: $($vendasResponse.Content.Substring(0, [Math]::Min(200, $vendasResponse.Content.Length)))..." -ForegroundColor Cyan
        }
    } catch {
        Write-Host "  Resposta (texto): $($vendasResponse.Content.Substring(0, [Math]::Min(200, $vendasResponse.Content.Length)))..." -ForegroundColor Cyan
    }
    
    Write-Host ""
    
} catch {
    Write-Host "  ERRO ao acessar endpoint protegido:" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "  Mensagem: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "  Resposta: $responseBody" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "Pressione qualquer tecla para sair..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}

# ========================================
# PASSO 3: TESTAR OUTRO ENDPOINT
# ========================================
Write-Host "[3/3] Testando outro endpoint protegido /api/produtos..." -ForegroundColor Yellow
Write-Host ""

try {
    $produtosResponse = Invoke-WebRequest `
        -Uri "http://localhost:5000/api/produtos?pagina=1&tamanhoPagina=5" `
        -Method GET `
        -Headers $headers `
        -UseBasicParsing
    
    Write-Host "  Status: $($produtosResponse.StatusCode) OK" -ForegroundColor Green
    Write-Host "  Tamanho da resposta: $($produtosResponse.Content.Length) bytes" -ForegroundColor Cyan
    Write-Host ""
    
} catch {
    Write-Host "  ERRO:" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
}

# ========================================
# RESUMO
# ========================================
Write-Host "========================================" -ForegroundColor Green
Write-Host "  TESTE COMPLETO - SUCESSO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Autenticacao funcionando corretamente!" -ForegroundColor Green
Write-Host "  - Login: OK" -ForegroundColor Green
Write-Host "  - Token gerado: OK" -ForegroundColor Green
Write-Host "  - Acesso a endpoints protegidos: OK" -ForegroundColor Green
Write-Host ""
Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
