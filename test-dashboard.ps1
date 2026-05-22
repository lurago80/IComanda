$body = '{"username":"LOJA","password":"1234"}'
$token = (Invoke-RestMethod -Uri "http://localhost:65375/api/auth/login" -Method Post -ContentType "application/json" -Body $body).token
$headers = @{Authorization="Bearer $token"}

# Test multiple date ranges to find what triggers 500
$ranges = @(
    @{inicio="2024-01-01"; fim="2024-12-31"},
    @{inicio="2024-01-01"; fim="2025-06-30"},
    @{inicio="2023-01-01"; fim="2023-12-31"},
    @{inicio="2022-01-01"; fim="2022-12-31"},
    @{inicio="2020-01-01"; fim="2024-12-31"}
)

foreach ($range in $ranges) {
    try {
        $url = "http://localhost:65375/api/relatorios/dashboard?dataInicio=$($range.inicio)&dataFim=$($range.fim)"
        $r = Invoke-WebRequest -Uri $url -Headers $headers
        $d = $r.Content | ConvertFrom-Json
        Write-Host "$($range.inicio) - $($range.fim): OK - TotalVendas=$($d.totalVendas), Valor=$($d.valorTotal), Dias=$($d.ticketPorDia.Count)" -ForegroundColor Green
    } catch {
        Write-Host "$($range.inicio) - $($range.fim): ERRO 500" -ForegroundColor Red
        # Try to get error body
        $stream = $_.Exception.Response.GetResponseStream()
        if ($stream) {
            $reader = New-Object System.IO.StreamReader($stream)
            $errorBody = $reader.ReadToEnd()
            Write-Host "  Body: $errorBody" -ForegroundColor Yellow
        }
    }
}
