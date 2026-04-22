# Script de teste para verificar o endpoint de emitente
Write-Host "🧪 Testando endpoint de emitente..." -ForegroundColor Cyan

$baseUrl = "http://localhost:65375"

Write-Host "`n1. Testando GET /api/vendas/emitente" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/vendas/emitente" -Method GET -ContentType "application/json" -UseBasicParsing
    Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "📦 Response:" -ForegroundColor Cyan
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $resp = $_.Exception.Response
        if ($resp -is [System.Net.Http.HttpResponseMessage]) {
            $body = $resp.Content.ReadAsStringAsync().Result
            Write-Host "📦 Response Body: $body" -ForegroundColor Yellow
        } elseif ($resp -is [System.Net.WebResponse]) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "📦 Response Body: $responseBody" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n2. Testando GET /api/emitente" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/emitente" -Method GET -ContentType "application/json" -UseBasicParsing
    Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "📦 Response:" -ForegroundColor Cyan
    $response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $resp = $_.Exception.Response
        if ($resp -is [System.Net.Http.HttpResponseMessage]) {
            $body = $resp.Content.ReadAsStringAsync().Result
            Write-Host "📦 Response Body: $body" -ForegroundColor Yellow
        } elseif ($resp -is [System.Net.WebResponse]) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "📦 Response Body: $responseBody" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n✅ Teste concluído!" -ForegroundColor Green


