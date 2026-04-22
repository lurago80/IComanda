# Script para diagnosticar problemas de Connection String
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  🔍 DIAGNÓSTICO DE CONNECTION STRING" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar appsettings.json no diretório atual
$appsettingsPath = Join-Path $PSScriptRoot "appsettings.json"
Write-Host "📁 Verificando appsettings.json..." -ForegroundColor Yellow
Write-Host "   Caminho: $appsettingsPath" -ForegroundColor Gray

if (Test-Path $appsettingsPath) {
    Write-Host "   ✅ Arquivo existe" -ForegroundColor Green
    
    try {
        $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        
        if ($appsettings.ConnectionStrings.Firebird) {
            $connString = $appsettings.ConnectionStrings.Firebird
            $connStringParaLog = $connString -replace 'Password=[^;]+', 'Password=***'
            
            Write-Host ""
            Write-Host "📋 Connection String no appsettings.json:" -ForegroundColor Cyan
            Write-Host "   $connStringParaLog" -ForegroundColor White
            
            # Extrair caminho do banco
            if ($connString -match 'Database=([^;]+)') {
                $dbPath = $matches[1]
                Write-Host ""
                Write-Host "📁 Caminho do banco de dados:" -ForegroundColor Cyan
                Write-Host "   $dbPath" -ForegroundColor White
                
                # Verificar se o arquivo existe
                if (Test-Path $dbPath) {
                    Write-Host "   ✅ Arquivo de banco encontrado!" -ForegroundColor Green
                    $fileInfo = Get-Item $dbPath
                    Write-Host "   Tamanho: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Gray
                    Write-Host "   Última modificação: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
                } else {
                    Write-Host "   ❌ Arquivo de banco NÃO encontrado!" -ForegroundColor Red
                    Write-Host "   Verifique se o caminho está correto." -ForegroundColor Yellow
                }
            }
        } else {
            Write-Host "   ❌ Connection String 'Firebird' não encontrada no appsettings.json!" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ❌ Erro ao ler appsettings.json: $_" -ForegroundColor Red
    }
} else {
    Write-Host "   ❌ Arquivo NÃO encontrado!" -ForegroundColor Red
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
    try {
        $appsettingsDev = Get-Content $appsettingsDevPath -Raw | ConvertFrom-Json
        if ($appsettingsDev.ConnectionStrings.Firebird) {
            $connDev = $appsettingsDev.ConnectionStrings.Firebird -replace 'Password=[^;]+', 'Password=***'
            Write-Host "   Connection String: $connDev" -ForegroundColor White
        }
    } catch {
        Write-Host "   Erro ao ler: $_" -ForegroundColor Red
    }
    Write-Host ""
}

# Verificar appsettings.Production.json
$appsettingsProdPath = Join-Path $PSScriptRoot "appsettings.Production.json"
if (Test-Path $appsettingsProdPath) {
    Write-Host "⚠️ appsettings.Production.json encontrado!" -ForegroundColor Yellow
    Write-Host "   Este arquivo pode sobrescrever o appsettings.json" -ForegroundColor Yellow
    try {
        $appsettingsProd = Get-Content $appsettingsProdPath -Raw | ConvertFrom-Json
        if ($appsettingsProd.ConnectionStrings.Firebird) {
            $connProd = $appsettingsProd.ConnectionStrings.Firebird -replace 'Password=[^;]+', 'Password=***'
            Write-Host "   Connection String: $connProd" -ForegroundColor White
        }
    } catch {
        Write-Host "   Erro ao ler: $_" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificando variáveis de ambiente..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$envConnString = [Environment]::GetEnvironmentVariable("ConnectionStrings__Firebird")
if ($envConnString) {
    Write-Host "⚠️ Variável de ambiente ConnectionStrings__Firebird encontrada!" -ForegroundColor Yellow
    $envConnStringParaLog = $envConnString -replace 'Password=[^;]+', 'Password=***'
    Write-Host "   $envConnStringParaLog" -ForegroundColor White
    Write-Host "   ⚠️ Esta variável SOBRESCREVE o appsettings.json!" -ForegroundColor Red
    Write-Host ""
} else {
    Write-Host "✅ Nenhuma variável de ambiente ConnectionStrings__Firebird encontrada" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Verificando diretório de execução..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar onde a aplicação será executada
$publishPath = Join-Path $PSScriptRoot "bin\Release\net8.0"
if (Test-Path $publishPath) {
    Write-Host "📁 Diretório de publicação (bin\Release\net8.0):" -ForegroundColor Cyan
    Write-Host "   $publishPath" -ForegroundColor White
    
    $publishAppsettings = Join-Path $publishPath "appsettings.json"
    if (Test-Path $publishAppsettings) {
        Write-Host "   ✅ appsettings.json existe no diretório de publicação" -ForegroundColor Green
        try {
            $publishSettings = Get-Content $publishAppsettings -Raw | ConvertFrom-Json
            if ($publishSettings.ConnectionStrings.Firebird) {
                $publishConn = $publishSettings.ConnectionStrings.Firebird -replace 'Password=[^;]+', 'Password=***'
                Write-Host "   Connection String: $publishConn" -ForegroundColor White
            }
        } catch {
            Write-Host "   Erro ao ler: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "   ❌ appsettings.json NÃO existe no diretório de publicação!" -ForegroundColor Red
        Write-Host "   ⚠️ Isso pode causar problemas!" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ℹ️ Diretório de publicação ainda não existe (execute dotnet build primeiro)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RECOMENDAÇÕES:" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "1. Verifique se o appsettings.json está no diretório onde a aplicação roda" -ForegroundColor White
Write-Host "2. Verifique se não há appsettings.{Environment}.json sobrescrevendo" -ForegroundColor White
Write-Host "3. Verifique se não há variáveis de ambiente sobrescrevendo" -ForegroundColor White
Write-Host "4. Após alterar appsettings.json, REINICIE a aplicação" -ForegroundColor White
Write-Host "5. Verifique os logs na inicialização para ver qual connection string está sendo usada" -ForegroundColor White
Write-Host ""
