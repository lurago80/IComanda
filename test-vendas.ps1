$body = '{"username":"LOJA","password":"1234"}'
$token = (Invoke-RestMethod -Uri "http://localhost:65375/api/auth/login" -Method Post -ContentType "application/json" -Body $body).token
$headers = @{Authorization="Bearer $token"}

# Check vendas abertas
Write-Host "=== Vendas Abertas ===" -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "http://localhost:65375/api/vendas/abertas" -Headers $headers
    Write-Host "Count: $($r.Count)"
    if ($r.Count -gt 0) { $r | Select-Object -First 2 | ConvertTo-Json -Depth 2 }
} catch { Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red }

# Check relatorios/vendas with wide range
Write-Host "`n=== Relatorio Vendas (2020-2026) ===" -ForegroundColor Cyan
try {
    $r2 = Invoke-RestMethod -Uri "http://localhost:65375/api/relatorios/vendas?dataInicio=2020-01-01&dataFim=2026-12-31" -Headers $headers
    Write-Host "Count: $($r2.Count)"
    if ($r2.Count -gt 0) {
        Write-Host "Primeira: LANCADO=$($r2[0].lancado) DATA=$($r2[0].dataSaida)"
        Write-Host "Ultima: LANCADO=$($r2[-1].lancado) DATA=$($r2[-1].dataSaida)"
    }
} catch { Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red }

# Check dashboard with absolute wide range
Write-Host "`n=== Dashboard (2000-2030) ===" -ForegroundColor Cyan
try {
    $r3 = Invoke-RestMethod -Uri "http://localhost:65375/api/relatorios/dashboard?dataInicio=2000-01-01&dataFim=2030-12-31" -Headers $headers
    Write-Host "TotalVendas: $($r3.totalVendas)"
    Write-Host "ValorTotal: $($r3.valorTotal)"
    Write-Host "TicketPorDia.Count: $($r3.ticketPorDia.Count)"
    if ($r3.ticketPorDia.Count -gt 0) {
        $r3.ticketPorDia | Select-Object -First 3 | ConvertTo-Json
    }
} catch { 
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
    $stream = $_.Exception.Response.GetResponseStream()
    if ($stream) { Write-Host ([System.IO.StreamReader]::new($stream).ReadToEnd()) -ForegroundColor Yellow }
}
