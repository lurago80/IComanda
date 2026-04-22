<#
.SYNOPSIS
    Executa script SQL para criar tabela REFRESH_TOKEN no Firebird

.DESCRIPTION
    Este script executa o SQL de criação da tabela REFRESH_TOKEN
    no banco de dados Firebird do iComanda.
    
.NOTES
    Requer: isql.exe (Firebird ISQL Tool)
    
.EXAMPLE
    .\executar-sql-refresh-token.ps1
#>

param(
    [string]$DatabasePath = "C:\iComanda\Dados\DADOSG5.FDB",
    [string]$User = "SYSDBA",
    [string]$Password = "masterkey",
    [string]$SqlFile = "criar-tabela-refresh-token.sql"
)

$ErrorActionPreference = "Stop"

Write-Host "`n🔧 CRIAÇÃO DA TABELA REFRESH_TOKEN" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# Verificar se o banco existe
if (-not (Test-Path $DatabasePath)) {
    Write-Host "❌ Banco de dados não encontrado: $DatabasePath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Banco de dados encontrado: $DatabasePath" -ForegroundColor Green

# Verificar se o script SQL existe
$scriptPath = Join-Path $PSScriptRoot $SqlFile
if (-not (Test-Path $scriptPath)) {
    Write-Host "❌ Script SQL não encontrado: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Script SQL encontrado: $scriptPath" -ForegroundColor Green

# Procurar isql.exe
$isqlPaths = @(
    "C:\Program Files\Firebird\Firebird_2_5\bin\isql.exe",
    "C:\Program Files (x86)\Firebird\Firebird_2_5\bin\isql.exe",
    "C:\Firebird\bin\isql.exe",
    "isql.exe"  # Tentar no PATH
)

$isqlExe = $null
foreach ($path in $isqlPaths) {
    if (Test-Path $path -ErrorAction SilentlyContinue) {
        $isqlExe = $path
        break
    }
    
    # Tentar encontrar no PATH
    if ($path -eq "isql.exe") {
        $found = Get-Command isql.exe -ErrorAction SilentlyContinue
        if ($found) {
            $isqlExe = "isql.exe"
            break
        }
    }
}

if (-not $isqlExe) {
    Write-Host "❌ isql.exe não encontrado!" -ForegroundColor Red
    Write-Host "Instale o Firebird ou adicione ao PATH:" -ForegroundColor Yellow
    Write-Host "  C:\Program Files\Firebird\Firebird_2_5\bin" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ isql.exe encontrado: $isqlExe" -ForegroundColor Green

# Criar arquivo de comando temporário para ISQL
$tempCmd = [System.IO.Path]::GetTempFileName() + ".sql"

$commands = @"
-- Conectar ao banco
-- (já passamos via linha de comando)

-- Verificar se a tabela já existe
SELECT COUNT(*) FROM RDB`$RELATIONS WHERE RDB`$RELATION_NAME = 'REFRESH_TOKEN';

-- Executar script de criação
INPUT '$scriptPath';

-- Verificar se foi criada
SELECT COUNT(*) FROM RDB`$RELATIONS WHERE RDB`$RELATION_NAME = 'REFRESH_TOKEN';

-- Mostrar estrutura
SHOW TABLE REFRESH_TOKEN;

EXIT;
"@

$commands | Out-File -FilePath $tempCmd -Encoding ASCII

try {
    Write-Host "`n📝 Executando SQL..." -ForegroundColor Yellow
    
    # Executar ISQL
    $output = & $isqlExe -user $User -password $Password $DatabasePath -input $tempCmd 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ SQL executado com sucesso!" -ForegroundColor Green
        
        # Mostrar output
        Write-Host "`n📄 Output:" -ForegroundColor Cyan
        $output | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        
        Write-Host "`n🎉 Tabela REFRESH_TOKEN criada com sucesso!" -ForegroundColor Green
        Write-Host "`n📋 Próximos passos:" -ForegroundColor Yellow
        Write-Host "   1. Reinicie a aplicação para usar Firebird" -ForegroundColor White
        Write-Host "   2. Teste o login e refresh token" -ForegroundColor White
        Write-Host "   3. Verifique os logs para confirmar persistência" -ForegroundColor White
    } else {
        Write-Host "`n❌ Erro ao executar SQL (Exit Code: $LASTEXITCODE)" -ForegroundColor Red
        Write-Host "`n📄 Output:" -ForegroundColor Cyan
        $output | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        exit 1
    }
} catch {
    Write-Host "`n❌ Exceção ao executar ISQL: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    # Limpar arquivo temporário
    if (Test-Path $tempCmd) {
        Remove-Item $tempCmd -Force
    }
}

Write-Host "`n✅ Script concluído com sucesso!" -ForegroundColor Green

<#
INSTRUÇÕES MANUAIS (se preferir):

1. Abra o ISQL:
   C:\Program Files\Firebird\Firebird_2_5\bin\isql.exe

2. Conecte ao banco:
   connect "C:\iComanda\Dados\DADOSG5.FDB" user 'SYSDBA' password 'masterkey';

3. Execute o script:
   input 'C:\VS_Code\icomanda\IComanda.API\criar-tabela-refresh-token.sql';

4. Verifique:
   show table refresh_token;
   select count(*) from refresh_token;

5. Saia:
   exit;
#>
