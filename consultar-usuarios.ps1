# Script para consultar usuários do banco Firebird
$connectionString = "Server=localhost;Database=C:\iComanda\Dados\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8"

Add-Type -Path "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.11\System.Data.Common.dll" -ErrorAction SilentlyContinue

try {
    # Carregar o assembly do Firebird
    $dllPath = Get-ChildItem -Path "$env:USERPROFILE\.nuget\packages\FirebirdSql.Data.FirebirdClient" -Recurse -Filter "FirebirdSql.Data.FirebirdClient.dll" | Select-Object -First 1 -ExpandProperty FullName
    
    if (-not $dllPath) {
        Write-Host "❌ FirebirdSql.Data.FirebirdClient.dll não encontrado" -ForegroundColor Red
        Write-Host "Tentando usar dotnet para consultar..." -ForegroundColor Yellow
        exit 1
    }
    
    Add-Type -Path $dllPath
    
    $connection = New-Object FirebirdSql.Data.FirebirdClient.FbConnection($connectionString)
    $connection.Open()
    
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT ID, NOME, SENHA, ATIVO, BLOQUEIO FROM USUARIOS WHERE ATIVO='1' ORDER BY ID FETCH FIRST 10 ROWS ONLY"
    
    $reader = $command.ExecuteReader()
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Usuários Ativos no Banco de Dados" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    while ($reader.Read()) {
        $id = $reader["ID"]
        $nome = $reader["NOME"]
        $senha = $reader["SENHA"]
        $ativo = $reader["ATIVO"]
        $bloqueio = if ($reader["BLOQUEIO"] -eq [DBNull]::Value) { "0" } else { $reader["BLOQUEIO"] }
        
        Write-Host "ID: $id" -ForegroundColor Yellow
        Write-Host "Nome: $nome" -ForegroundColor Green
        Write-Host "Senha: $senha" -ForegroundColor White
        Write-Host "Ativo: $ativo, Bloqueio: $bloqueio"
        Write-Host "----------------------------------------"
    }
    
    $reader.Close()
    $connection.Close()
    
} catch {
    Write-Host "❌ Erro: $_" -ForegroundColor Red
    Write-Host "`nTentando método alternativo...`n" -ForegroundColor Yellow
    
    # Método alternativo usando dotnet e Dapper
    $code = @"
using System;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using Dapper;

var connectionString = "Server=localhost;Database=C:\\iComanda\\Dados\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8";
using var connection = new FbConnection(connectionString);

var usuarios = connection.Query("SELECT ID, NOME, SENHA, ATIVO FROM USUARIOS WHERE ATIVO='1' ORDER BY ID FETCH FIRST 10 ROWS ONLY");
Console.WriteLine("ID | Nome | Senha | Ativo");
foreach (var u in usuarios)
{
    Console.WriteLine($"{u.ID} | {u.NOME} | {u.SENHA} | {u.ATIVO}");
}
"@
    
    Set-Content -Path "temp-query.csx" -Value $code
    dotnet script temp-query.csx
    Remove-Item temp-query.csx -ErrorAction SilentlyContinue
}
