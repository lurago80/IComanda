<#
.SYNOPSIS
    Script de migração de senhas plaintext para BCrypt hash

.DESCRIPTION
    Este script migra todas as senhas de usuários do banco de dados Firebird
    de texto plano (plaintext) para hash BCrypt seguro.
    
    ATENÇÃO: Execute este script UMA VEZ após implementar password hashing
    
.NOTES
    - Requer .NET 8.0 SDK instalado
    - Requer acesso ao banco de dados Firebird
    - Backup do banco antes de executar!
    
.EXAMPLE
    .\migrar-senhas-bcrypt.ps1
#>

param(
    [string]$ConnectionString = "User=SYSDBA;Password=masterkey;Database=C:\iComanda\Dados\DADOSG5.FDB;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;",
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"

Write-Host "`n🔐 MIGRAÇÃO DE SENHAS PARA BCRYPT HASH" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# Verificar se é dry run
if ($DryRun) {
    Write-Host "⚠️  MODO DRY-RUN: Nenhuma alteração será feita no banco" -ForegroundColor Yellow
}

# Criar script C# temporário para fazer a migração
$scriptContent = @"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using Dapper;
using BCrypt.Net;

public class PasswordMigrator
{
    private readonly string _connectionString;
    private readonly bool _dryRun;
    private readonly bool _verbose;

    public PasswordMigrator(string connectionString, bool dryRun, bool verbose)
    {
        _connectionString = connectionString;
        _dryRun = dryRun;
        _verbose = verbose;
    }

    public async Task MigrateAsync()
    {
        Console.WriteLine($"\n📊 Conectando ao banco de dados...");
        
        using var connection = new FbConnection(_connectionString);
        await connection.OpenAsync();
        
        Console.WriteLine("✅ Conexão estabelecida");
        
        // Buscar todos os usuários
        var usuarios = await connection.QueryAsync<Usuario>(
            "SELECT ID, NOME, SENHA FROM USUARIO WHERE ATIVO = '1'");
        
        var usuariosLista = new List<Usuario>(usuarios);
        Console.WriteLine($"\n📋 Total de usuários ativos: {usuariosLista.Count}");
        
        int migrados = 0;
        int jaHasheados = 0;
        int erros = 0;
        int vazios = 0;
        
        foreach (var usuario in usuariosLista)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario.Senha))
                {
                    Console.WriteLine($"⚠️  Usuário {usuario.ID} ({usuario.Nome}) - Senha vazia, pulando...");
                    vazios++;
                    continue;
                }
                
                // Verificar se já está em hash BCrypt
                if (IsPasswordHashed(usuario.Senha))
                {
                    if (_verbose)
                    {
                        Console.WriteLine($"✓ Usuário {usuario.ID} ({usuario.Nome}) - Já em BCrypt hash");
                    }
                    jaHasheados++;
                    continue;
                }
                
                // Senha está em plaintext - migrar
                var senhaOriginal = usuario.Senha;
                var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaOriginal, 12);
                
                Console.WriteLine($"🔄 Usuário {usuario.ID} ({usuario.Nome})");
                if (_verbose)
                {
                    Console.WriteLine($"   Plaintext: {senhaOriginal.Substring(0, Math.Min(3, senhaOriginal.Length))}*** ({senhaOriginal.Length} chars)");
                    Console.WriteLine($"   Hash:      {senhaHash.Substring(0, 20)}... (60 chars)");
                }
                
                if (!_dryRun)
                {
                    await connection.ExecuteAsync(
                        "UPDATE USUARIO SET SENHA = @Senha WHERE ID = @Id",
                        new { Id = usuario.ID, Senha = senhaHash });
                    
                    Console.WriteLine($"   ✅ Migrado com sucesso");
                }
                else
                {
                    Console.WriteLine($"   [DRY-RUN] Seria migrado");
                }
                
                migrados++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao migrar usuário {usuario.ID}: {ex.Message}");
                erros++;
            }
        }
        
        // Resumo
        Console.WriteLine($"\n📊 RESUMO DA MIGRAÇÃO");
        Console.WriteLine($"=" . Repeat(60));
        Console.WriteLine($"Total de usuários:        {usuariosLista.Count}");
        Console.WriteLine($"Já em BCrypt hash:        {jaHasheados}");
        Console.WriteLine($"Migrados agora:           {migrados}");
        Console.WriteLine($"Senhas vazias:            {vazios}");
        Console.WriteLine($"Erros:                    {erros}");
        Console.WriteLine($"=" . Repeat(60));
        
        if (_dryRun)
        {
            Console.WriteLine($"\n⚠️  DRY-RUN completado - Nenhuma alteração foi feita");
            Console.WriteLine($"Execute sem -DryRun para aplicar as mudanças");
        }
        else if (migrados > 0)
        {
            Console.WriteLine($"\n✅ Migração concluída com sucesso!");
            Console.WriteLine($"⚠️  IMPORTANTE: Teste o login de todos os usuários!");
        }
    }
    
    private bool IsPasswordHashed(string password)
    {
        return password.StartsWith("$2") && password.Length == 60;
    }
    
    private class Usuario
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string Senha { get; set; }
    }
}

public static class StringExtensions
{
    public static string Repeat(this string text, int count)
    {
        return new string(text[0], count);
    }
}

// Entry point
var migrator = new PasswordMigrator(
    args[0], 
    bool.Parse(args[1]), 
    bool.Parse(args[2]));

await migrator.MigrateAsync();
"@

# Salvar script temporário
$tempFile = [System.IO.Path]::GetTempFileName() + ".cs"
$tempProject = [System.IO.Path]::GetTempFileName() + ".csproj"

$scriptContent | Out-File -FilePath $tempFile -Encoding UTF8

# Criar projeto temporário
$projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FirebirdSql.Data.FirebirdClient" Version="8.5.0" />
    <PackageReference Include="Dapper" Version="2.1.15" />
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  </ItemGroup>
</Project>
"@

$projectContent | Out-File -FilePath $tempProject -Encoding UTF8

try {
    Write-Host "`n🔨 Compilando script de migração..." -ForegroundColor Yellow
    
    # Restaurar pacotes e compilar
    $tempDir = Split-Path $tempProject
    Copy-Item $tempFile "$tempDir\Program.cs" -Force
    
    dotnet build $tempProject --configuration Release --verbosity quiet | Out-Null
    
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao compilar script de migração"
    }
    
    Write-Host "✅ Compilação concluída`n" -ForegroundColor Green
    
    # Executar migração
    $exePath = Join-Path $tempDir "bin\Release\net8.0\*.exe"
    $exe = Get-Item $exePath | Select-Object -First 1
    
    & $exe.FullName $ConnectionString $DryRun.ToString() $Verbose.ToString()
    
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao executar migração"
    }
    
} catch {
    Write-Host "`n❌ ERRO: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Limpar arquivos temporários
    if (Test-Path $tempFile) { Remove-Item $tempFile -Force }
    if (Test-Path $tempProject) { Remove-Item $tempProject -Force }
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
}

Write-Host "`n✅ Script concluído" -ForegroundColor Green

<#
INSTRUÇÕES DE USO:

1. BACKUP DO BANCO (OBRIGATÓRIO):
   Copy-Item "C:\iComanda\Dados\DADOSG5.FDB" "C:\iComanda\Dados\BACKUP_DADOSG5.FDB"

2. EXECUTAR EM MODO DRY-RUN (recomendado primeiro):
   .\migrar-senhas-bcrypt.ps1 -DryRun -Verbose

3. EXECUTAR MIGRAÇÃO REAL:
   .\migrar-senhas-bcrypt.ps1

4. TESTAR LOGIN DE TODOS OS USUÁRIOS

5. SE HOUVER PROBLEMAS, RESTAURAR BACKUP:
   Copy-Item "C:\iComanda\Dados\BACKUP_DADOSG5.FDB" "C:\iComanda\Dados\DADOSG5.FDB"

OBSERVAÇÕES:
- A migração automática no AuthController já migra senhas no primeiro login
- Este script é útil para migrar TODAS as senhas de uma vez
- Usuários que não fizeram login ainda terão senhas migradas no próximo login
#>
