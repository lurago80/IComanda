Add-Type -Path "C:\Users\usuario\.nuget\packages\firebirdsql.data.firebirdclient\10.3.1\lib\net48\FirebirdSql.Data.FirebirdClient.dll"
$cs="Server=localhost;Database=C:\Users\usuario\Desktop\Base\Eduardo\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8"
$conn=New-Object FirebirdSql.Data.FirebirdClient.FbConnection($cs);$conn.Open()
$cmd=$conn.CreateCommand();$cmd.CommandText="SELECT FIRST 10 ID, NOME, TIPO, ATIVO FROM USUARIO ORDER BY ID"
$r=$cmd.ExecuteReader();while($r.Read()){Write-Host "ID=$($r[0]) NOME=$($r[1]) TIPO=$($r[2]) ATIVO=$($r[3])"}
$r.Close();$conn.Close()
