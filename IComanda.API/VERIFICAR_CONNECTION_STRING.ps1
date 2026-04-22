# Script para verificar qual connection string está sendo usada
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VERIFICAÇÃO DE CONNECTION STRING" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar appsettings.json no diretório atual
$appsettingsPath = Join-Path $PSScriptRoot "appsettings.json"
if (Test-Path $appsettingsPath) {
    Write-Host "✅ appsettings.json encontrado em:" -ForegroundColor Green
    Write-Host "   $appsettingsPath" -ForegroundColor Yellow
    Write-Host ""
    
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    if ($appsettings.ConnectionStrings.Firebird) {
        $connString = $appsettings.ConnectionStrings.Firebird
        # Ocultar senha
        $connStringParaLog = $connString -replace 'Password=[^;]+', 'Password=***'
        Write-Host "📋 Connection String no appsettings.json:" -ForegroundColor Cyan
        Write-Host "   $connStringParaLog" -ForegroundColor White
        Write-Host ""
        
        # Extrair caminho do banco
        if ($connString -match 'Database=([^;]+)') {
            $dbPath = $matches[1]
            Write-Host "📁 Caminho do banco de dados:" -ForegroundColor Cyan
            Write-Host "   $dbPath" -ForegroundColor White
            Write-Host ""
            
            # Verificar se o arquivo existe
            if (Test-Path $dbPath) {
                Write-Host "✅ Arquivo de banco encontrado!" -ForegroundColor Green
                $fileInfo = Get-Item $dbPath
                Write-Host "   Tamanho: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Gray
                Write-Host "   Última modificação: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
            } else {
                Write-Host "❌ Arquivo de banco NÃO encontrado no caminho especificado!" -ForegroundColor Red
                Write-Host "   Verifique se o caminho está correto." -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "❌ Connection String 'Firebird' não encontrada no appsettings.json!" -ForegroundColor Red
    }
} else {
    Write-Host "❌ appsettings.json NÃO encontrado em:" -ForegroundColor Red
    Write-Host "   $appsettingsPath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificando outros arquivos appsettings..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar appsettings.Development.json
$appsettingsDevPath = Join-Path $PSScriptRoot "appsettings.Development.json"
if (Test-Path $appsettingsDevPath) {
    Write-Host "⚠️ appsettings.Development.json encontrado!" -ForegroundColor Yellow
    Write-Host "   Este arquivo pode sobrescrever o appsettings.json" -ForegroundColor Yellow
    Write-Host "   $appsettingsDevPath" -ForegroundColor Yellow
    Write-Host ""
}

# Verificar appsettings.Production.json
$appsettingsProdPath = Join-Path $PSScriptRoot "appsettings.Production.json"
if (Test-Path $appsettingsProdPath) {
    Write-Host "⚠️ appsettings.Production.json encontrado!" -ForegroundColor Yellow
    Write-Host "   Este arquivo pode sobrescrever o appsettings.json" -ForegroundColor Yellow
    Write-Host "   $appsettingsProdPath" -ForegroundColor Yellow
    Write-Host ""
}

# Verificar variáveis de ambiente
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificando variáveis de ambiente..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$envConnString = [Environment]::GetEnvironmentVariable("ConnectionStrings__Firebird")
if ($envConnString) {
    Write-Host "⚠️ Variável de ambiente ConnectionStrings__Firebird encontrada!" -ForegroundColor Yellow
    $envConnStringParaLog = $envConnString -replace 'Password=[^;]+', 'Password=***'
    Write-Host "   $envConnStringParaLog" -ForegroundColor White
    Write-Host "   Esta variável SOBRESCREVE o appsettings.json!" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "✅ Nenhuma variável de ambiente ConnectionStrings__Firebird encontrada" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RECOMENDAÇÕES:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Verifique se o appsettings.json está no diretório onde a aplicação roda" -ForegroundColor White
Write-Host "2. Verifique se não há appsettings.{Environment}.json sobrescrevendo" -ForegroundColor White
Write-Host "3. Verifique se não há variáveis de ambiente sobrescrevendo" -ForegroundColor White
Write-Host "4. Após alterar appsettings.json, REINICIE a aplicação" -ForegroundColor White
Write-Host ""
