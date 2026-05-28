$r = Invoke-RestMethod "http://localhost:65375/api/auth/login" -Method POST -Body '{"username":"LOJA","password":"1234"}' -ContentType "application/json"
$token = $r.token
$h = @{ Authorization = "Bearer $token" }
Write-Host "=== configuracoes/sistema ==="
try {
    $conf = Invoke-RestMethod "http://localhost:65375/api/configuracoes/sistema" -Headers $h
    $conf | ConvertTo-Json
} catch { Write-Host "ERRO $($_.Exception.Response.StatusCode): $($_.ErrorDetails.Message)" }
Write-Host ""
Write-Host "=== vendas/abertas ==="
try {
    $v = Invoke-RestMethod "http://localhost:65375/api/vendas/abertas" -Headers $h
    Write-Host "Count: $($v.Count)"
} catch { Write-Host "ERRO $($_.Exception.Response.StatusCode): $($_.ErrorDetails.Message)" }
