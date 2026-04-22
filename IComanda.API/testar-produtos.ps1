# Script para testar se os produtos estão sendo retornados pela API

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Teste de Produtos - Diagnóstico" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:65375"

Write-Host "1. Testando endpoint de diagnóstico..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/produtos/diagnostico" -Method Get
    Write-Host "✅ Diagnóstico obtido com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Total de produtos: $($response.totalProdutos)" -ForegroundColor Cyan
    Write-Host "Produtos ativos: $($response.produtosAtivos)" -ForegroundColor Green
    Write-Host "Produtos inativos: $($response.produtosInativos)" -ForegroundColor Yellow
    Write-Host "Produtos sem grupo: $($response.produtosSemGrupo)" -ForegroundColor Red
    Write-Host "Produtos com grupo: $($response.produtosComGrupo)" -ForegroundColor Green
    Write-Host "Grupos com produtos: $($response.gruposComProdutos)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Primeiros 10 produtos:" -ForegroundColor Cyan
    $response.primeirosProdutos | ForEach-Object {
        Write-Host "  - ID: $($_.id), Descrição: $($_.descricao), Grupo: $($_.grupo), Ativo: $($_.ativo)" -ForegroundColor White
    }
} catch {
    Write-Host "❌ Erro ao obter diagnóstico: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "2. Testando busca de produtos por grupo..." -ForegroundColor Yellow
try {
    # Testar com grupo 1 (geralmente existe)
    $response = Invoke-RestMethod -Uri "$baseUrl/api/produtos/grupo/1?ativo=true" -Method Get
    Write-Host "✅ Produtos do grupo 1: $($response.Count)" -ForegroundColor Green
    if ($response.Count -gt 0) {
        Write-Host "   Primeiros produtos:" -ForegroundColor Cyan
        $response | Select-Object -First 3 | ForEach-Object {
            Write-Host "     - $($_.id): $($_.descricao)" -ForegroundColor White
        }
    }
} catch {
    Write-Host "❌ Erro ao buscar produtos do grupo 1: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "3. Testando busca geral de produtos..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/produtos/buscar?ativo=true" -Method Get
    Write-Host "✅ Produtos ativos encontrados: $($response.Count)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao buscar produtos: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Teste concluído!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
