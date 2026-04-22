<#
.SYNOPSIS
    Diagnostica problemas de autenticação JWT no iComanda

.DESCRIPTION
    Script para diagnosticar e corrigir problemas 401 Unauthorized
    relacionados a tokens JWT expirados ou inválidos.

.EXAMPLE
    .\diagnosticar-jwt-401.ps1
#>

param(
    [string]$ApiUrl = "http://localhost:65375",
    [string]$Username = "admin",
    [string]$Password = "123456"
)

$ErrorActionPreference = "Continue"

Write-Host "`n🔍 DIAGNÓSTICO DE AUTENTICAÇÃO JWT" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# Cores
function Write-Success { param($msg) Write-Host "✅ $msg" -ForegroundColor Green }
function Write-Error { param($msg) Write-Host "❌ $msg" -ForegroundColor Red }
function Write-Warning { param($msg) Write-Host "⚠️  $msg" -ForegroundColor Yellow }
function Write-Info { param($msg) Write-Host "ℹ️  $msg" -ForegroundColor Cyan }

# 1. Verificar se o backend está rodando
Write-Info "1. Verificando se o backend está rodando..."

try {
    $healthCheck = Invoke-WebRequest -Uri "$ApiUrl/health" -Method GET -TimeoutSec 5 -ErrorAction Stop
    Write-Success "Backend está rodando na porta 65375"
} catch {
    Write-Error "Backend NÃO está rodando!"
    Write-Warning "Inicie o backend primeiro: cd C:\icomanda\IComanda.API ; dotnet run --configuration Release"
    exit 1
}

# 2. Testar login
Write-Info "`n2. Testando login..."

try {
    $loginBody = @{
        username = $Username
        password = $Password
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" `
        -Method POST `
        -Body $loginBody `
        -ContentType "application/json" `
        -ErrorAction Stop

    Write-Success "Login bem-sucedido!"
    Write-Host "   Token: $($loginResponse.token.Substring(0, 30))..." -ForegroundColor Gray
    
    if ($loginResponse.refreshToken) {
        Write-Host "   RefreshToken: $($loginResponse.refreshToken.Substring(0, 30))..." -ForegroundColor Gray
    }
    
    Write-Host "   Expira em: $($loginResponse.expiresIn) horas" -ForegroundColor Gray
    Write-Host "   User ID: $($loginResponse.userId)" -ForegroundColor Gray
    Write-Host "   Role: $($loginResponse.role)" -ForegroundColor Gray

} catch {
    Write-Error "Falha no login!"
    Write-Host "   Erro: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Warning "Credenciais inválidas ou usuário bloqueado"
        Write-Info "Verifique username e password no banco de dados"
    }
    
    exit 1
}

# 3. Verificar configuração JWT
Write-Info "`n3. Verificando configuração JWT..."

$appsettingsPath = "C:\icomanda\IComanda.API\appsettings.json"

if (Test-Path $appsettingsPath) {
    try {
        $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
        
        $jwtExpiration = $appsettings.Jwt.ExpirationHours
        $jwtRefresh = $appsettings.Jwt.RefreshExpirationDays
        
        Write-Host "   Token Expiration: $jwtExpiration horas" -ForegroundColor Gray
        Write-Host "   Refresh Expiration: $jwtRefresh dias" -ForegroundColor Gray
        
        if ($jwtExpiration -eq 2) {
            Write-Success "Configuração JWT correta (2h)"
        } elseif ($jwtExpiration -gt 24) {
            Write-Warning "Token expira em $jwtExpiration horas (muito longo!)"
            Write-Info "Recomendado: 2 horas"
        }
        
        if (-not $appsettings.Jwt.SecretKey) {
            Write-Error "JWT SecretKey não está definido!"
            Write-Warning "Adicione SecretKey no appsettings.json"
        }
        
    } catch {
        Write-Warning "Não foi possível ler appsettings.json"
    }
} else {
    Write-Warning "appsettings.json não encontrado em C:\icomanda\IComanda.API\"
}

# 4. Testar endpoint protegido
Write-Info "`n4. Testando acesso a endpoint protegido..."

try {
    $headers = @{
        "Authorization" = "Bearer $($loginResponse.token)"
    }
    
    $vendas = Invoke-RestMethod -Uri "$ApiUrl/api/vendas/abertas" `
        -Method GET `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Success "Endpoint protegido acessível!"
    Write-Host "   Total de vendas abertas: $($vendas.Count)" -ForegroundColor Gray
    
} catch {
    Write-Error "Falha ao acessar endpoint protegido!"
    Write-Host "   Erro: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        Write-Host "   Status Code: $statusCode" -ForegroundColor Red
        
        if ($statusCode -eq 401) {
            Write-Warning "Token inválido ou expirado!"
            Write-Info "Possíveis causas:"
            Write-Host "   1. Chave JWT diferente entre front/back" -ForegroundColor Gray
            Write-Host "   2. Token corrompido" -ForegroundColor Gray
            Write-Host "   3. Servidor reiniciado (tokens invalidados)" -ForegroundColor Gray
        }
    }
}

# 5. Verificar LocalStorage do Frontend
Write-Info "`n5. Verificando localStorage do navegador..."
Write-Warning "AÇÃO MANUAL NECESSÁRIA:"
Write-Host "   1. Abra o navegador (F12 → Console)" -ForegroundColor White
Write-Host "   2. Execute: localStorage.getItem('token')" -ForegroundColor White
Write-Host "   3. Execute: localStorage.getItem('user')" -ForegroundColor White
Write-Host "   4. Se retornar null ou token antigo → LIMPAR:" -ForegroundColor White
Write-Host "      localStorage.clear()" -ForegroundColor Yellow

# 6. Verificar tabela REFRESH_TOKEN
Write-Info "`n6. Verificando tabela REFRESH_TOKEN..."

$dbPath = "C:\icomanda\Dados\DADOSG5.FDB"

if (Test-Path $dbPath) {
    Write-Info "Banco de dados encontrado: $dbPath"
    Write-Warning "VERIFICAÇÃO MANUAL (ISQL):"
    Write-Host "   isql -user SYSDBA -password masterkey ""$dbPath""" -ForegroundColor White
    Write-Host "   SQL> SHOW TABLE REFRESH_TOKEN;" -ForegroundColor White
    Write-Host "   SQL> SELECT COUNT(*) FROM REFRESH_TOKEN;" -ForegroundColor White
    Write-Host ""
    Write-Host "   Se retornar erro 'Table unknown':" -ForegroundColor Yellow
    Write-Host "   Execute: .\executar-sql-refresh-token.ps1" -ForegroundColor Yellow
} else {
    Write-Error "Banco de dados não encontrado!"
}

# Resumo
Write-Host "`n📊 RESUMO DO DIAGNÓSTICO" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

Write-Host "`n🔧 SOLUÇÕES PARA ERRO 401:" -ForegroundColor Yellow
Write-Host ""
Write-Host "SOLUÇÃO 1: Limpar localStorage e fazer login novamente" -ForegroundColor White
Write-Host "   1. Abra o navegador (F12 → Console)" -ForegroundColor Gray
Write-Host "   2. Execute: localStorage.clear()" -ForegroundColor Gray
Write-Host "   3. Recarregue a página (F5)" -ForegroundColor Gray
Write-Host "   4. Faça login novamente" -ForegroundColor Gray
Write-Host ""

Write-Host "SOLUÇÃO 2: Criar tabela REFRESH_TOKEN (se não existir)" -ForegroundColor White
Write-Host "   cd C:\icomanda\IComanda.API" -ForegroundColor Gray
Write-Host "   .\executar-sql-refresh-token.ps1" -ForegroundColor Gray
Write-Host "   Reinicie o backend" -ForegroundColor Gray
Write-Host ""

Write-Host "SOLUÇÃO 3: Verificar chave JWT no appsettings.json" -ForegroundColor White
Write-Host "   Abra: C:\icomanda\IComanda.API\appsettings.json" -ForegroundColor Gray
Write-Host "   Verifique: Jwt.SecretKey está definido" -ForegroundColor Gray
Write-Host "   Se não estiver, adicione uma chave de pelo menos 32 caracteres" -ForegroundColor Gray
Write-Host ""

Write-Host "SOLUÇÃO 4: Reiniciar tudo" -ForegroundColor White
Write-Host "   cd C:\icomanda" -ForegroundColor Gray
Write-Host "   .\parar-tudo.bat" -ForegroundColor Gray
Write-Host "   .\iniciar-tudo.bat" -ForegroundColor Gray

Write-Host "`n✅ Diagnóstico concluído!" -ForegroundColor Green

<#
EXEMPLOS DE OUTPUT:

✅ SUCESSO:
  - Backend rodando
  - Login OK
  - Token válido
  - Endpoint acessível
  
❌ ERRO 401:
  - Token expirado (2h)
  - localStorage com token antigo
  - Tabela REFRESH_TOKEN não existe
  - Chave JWT incorreta
  
COMO CORRIGIR:
  1. localStorage.clear() no navegador
  2. Fazer login novamente
  3. Verificar tabela REFRESH_TOKEN existe
  4. Verificar appsettings.json
#>
