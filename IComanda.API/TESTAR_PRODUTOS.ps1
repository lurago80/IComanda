# Script de Teste para Consulta de Produtos
# Este script testa a consulta de produtos diretamente

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TESTE DE CONSULTA DE PRODUTOS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar se a API está rodando
$apiUrl = "http://localhost:5000"
if ($env:ASPNETCORE_URLS) {
    $apiUrl = $env:ASPNETCORE_URLS -replace ".*://", "http://" -replace ".*://", ""
    $apiUrl = $apiUrl -split "," | Select-Object -First 1
    if (-not $apiUrl.StartsWith("http")) {
        $apiUrl = "http://$apiUrl"
    }
}

Write-Host "🔍 Testando endpoint: $apiUrl/api/produtos/teste" -ForegroundColor Yellow
Write-Host ""

try {
    # Teste 1: Endpoint de teste
    Write-Host "📋 Teste 1: Endpoint /api/produtos/teste" -ForegroundColor Green
    $response = Invoke-RestMethod -Uri "$apiUrl/api/produtos/teste" -Method Get -ErrorAction Stop
    Write-Host "✅ Sucesso!" -ForegroundColor Green
    Write-Host "   Produtos Ativos: $($response.produtosAtivos)" -ForegroundColor White
    Write-Host "   Todos Produtos: $($response.todosProdutos)" -ForegroundColor White
    if ($response.produtosCompletos) {
        Write-Host "   Produtos Completos: $($response.produtosCompletos)" -ForegroundColor White
    }
    Write-Host ""
} catch {
    Write-Host "❌ Erro no teste 1:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "   Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

try {
    # Teste 2: Endpoint de produtos completos (o que estava falhando)
    Write-Host "📋 Teste 2: Endpoint /api/produtos/completos?ativo=true" -ForegroundColor Green
    $response = Invoke-RestMethod -Uri "$apiUrl/api/produtos/completos?ativo=true&pagina=1&itensPorPagina=5" -Method Get -ErrorAction Stop
    Write-Host "✅ Sucesso! Produtos retornados: $($response.Count)" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "   Primeiro produto: $($response[0].descricao) (ID: $($response[0].id))" -ForegroundColor White
    }
    Write-Host ""
} catch {
    Write-Host "❌ Erro no teste 2:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json -ErrorAction SilentlyContinue
        if ($errorDetails) {
            Write-Host "   Mensagem: $($errorDetails.message)" -ForegroundColor Red
            if ($errorDetails.innerException) {
                Write-Host "   Inner Exception: $($errorDetails.innerException)" -ForegroundColor Red
            }
        } else {
            Write-Host "   Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
    }
    Write-Host ""
}

try {
    # Teste 3: Endpoint de busca simples
    Write-Host "📋 Teste 3: Endpoint /api/produtos/buscar?ativo=true" -ForegroundColor Green
    $response = Invoke-RestMethod -Uri "$apiUrl/api/produtos/buscar?ativo=true&pagina=1&itensPorPagina=5" -Method Get -ErrorAction Stop
    Write-Host "✅ Sucesso! Produtos retornados: $($response.Count)" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "   Primeiro produto: $($response[0].descricao) (ID: $($response[0].id))" -ForegroundColor White
    }
    Write-Host ""
} catch {
    Write-Host "❌ Erro no teste 3:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "   Detalhes: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TESTE CONCLUÍDO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
