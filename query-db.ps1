$fbDll = Get-ChildItem -Path "$env:USERPROFILE\.nuget\packages\FirebirdSql.Data.FirebirdClient" -Recurse -Filter "FirebirdSql.Data.FirebirdClient.dll" | Select-Object -First 1 -ExpandProperty FullName
Add-Type -Path $fbDll -ErrorAction SilentlyContinue

$cs = "Server=localhost;Database=C:\Users\usuario\Desktop\Base\Eduardo\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8"
$conn = New-Object FirebirdSql.Data.FirebirdClient.FbConnection($cs)
$conn.Open()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT LANCADO, COUNT(*) as QTD, MIN(DATA_SAIDA) as MIN_DATA, MAX(DATA_SAIDA) as MAX_DATA FROM VENDAS GROUP BY LANCADO ORDER BY LANCADO"
$r = $cmd.ExecuteReader()
Write-Host "`n=== VENDAS por LANCADO ===" -ForegroundColor Cyan
while($r.Read()){
    Write-Host "LANCADO='$($r['LANCADO'])' QTD=$($r['QTD']) DATA_MIN=$($r['MIN_DATA']) DATA_MAX=$($r['MAX_DATA'])"
}
$r.Close()

$cmd2 = $conn.CreateCommand()
$cmd2.CommandText = "SELECT COUNT(*) FROM VENDAS"
Write-Host "`nTotal VENDAS: $($cmd2.ExecuteScalar())"

$conn.Close()
