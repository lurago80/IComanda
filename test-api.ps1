$body = '{"username":"LOJA","password":"1234"}'
$token = (Invoke-RestMethod -Uri "http://localhost:65375/api/auth/login" -Method Post -ContentType "application/json" -Body $body).token
$headers = @{Authorization="Bearer $token"}

Write-Host "=== Grupos todos-com-quantidade ===" -ForegroundColor Cyan
try {
    $r1 = Invoke-WebRequest -Uri "http://localhost:65375/api/grupos/todos-com-quantidade" -Headers $headers
    Write-Host "Status: $($r1.StatusCode)" -ForegroundColor Green
    $data = $r1.Content | ConvertFrom-Json
    Write-Host "Count: $($data.Count)" -ForegroundColor Green
    $data | Select-Object -First 3 | ConvertTo-Json
} catch {
    Write-Host "ERRO: $($_.Exception.Message)" -ForegroundColor Red
    $_.Exception.Response.GetResponseStream() | ForEach-Object { [System.IO.StreamReader]::new($_).ReadToEnd() }
}

Write-Host ""
Write-Host "=== Dashboard (2025-01-01 a 2025-12-31) ===" -ForegroundColor Cyan
try {
    $r2 = Invoke-WebRequest -Uri "http://localhost:65375/api/relatorios/dashboard?dataInicio=2025-01-01&dataFim=2025-12-31" -Headers $headers
    Write-Host "Status: $($r2.StatusCode)" -ForegroundColor Green
    $r2.Content | ConvertFrom-Json | ConvertTo-Json -Depth 2
} catch {
    Write-Host "ERRO: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Dashboard (2024-01-01 a 2026-12-31) ===" -ForegroundColor Cyan
try {
    $r3 = Invoke-WebRequest -Uri "http://localhost:65375/api/relatorios/dashboard?dataInicio=2024-01-01&dataFim=2026-12-31" -Headers $headers
    Write-Host "Status: $($r3.StatusCode)" -ForegroundColor Green
    $d3 = $r3.Content | ConvertFrom-Json
    Write-Host "TotalVendas: $($d3.totalVendas)" -ForegroundColor Green
    Write-Host "ValorTotal: $($d3.valorTotal)" -ForegroundColor Green
    Write-Host "TicketMedio: $($d3.ticketMedio)" -ForegroundColor Green
    Write-Host "TicketPorDia count: $($d3.ticketPorDia.Count)" -ForegroundColor Green
    Write-Host "VendasPorHora count: $($d3.vendasPorHora.Count)" -ForegroundColor Green
} catch {
    Write-Host "ERRO: $($_.Exception.Message)" -ForegroundColor Red
}
