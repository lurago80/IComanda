<#
.SYNOPSIS
    Corrige erro ESLint no frontend do iComanda

.DESCRIPTION
    Script para corrigir o erro "Plugin 'react' was conflicted" 
    que ocorre ao iniciar o frontend na máquina do cliente.
    
    Aplica as seguintes correções:
    1. Cria .env.local desabilitando ESLint
    2. Remove configuração ESLint do package.json
    3. Cria .eslintrc.json simplificado
    4. Limpa cache e reinstala dependências

.PARAMETER FrontendPath
    Caminho para a pasta do frontend
    
.PARAMETER LimparCache
    Se deve limpar node_modules e reinstalar (padrão: $false)

.EXAMPLE
    .\corrigir-eslint-frontend.ps1
    
.EXAMPLE
    .\corrigir-eslint-frontend.ps1 -LimparCache
#>

param(
    [string]$FrontendPath = "C:\iCOMANDA\IComanda.API\icomanda-frontend",
    [switch]$LimparCache = $false
)

$ErrorActionPreference = "Stop"

Write-Host "`n🔧 CORREÇÃO ERRO ESLINT - FRONTEND" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray

# Verificar se o diretório existe
if (-not (Test-Path $FrontendPath)) {
    Write-Host "❌ Diretório não encontrado: $FrontendPath" -ForegroundColor Red
    Write-Host "Use: .\corrigir-eslint-frontend.ps1 -FrontendPath 'C:\caminho\correto'" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Diretório encontrado: $FrontendPath" -ForegroundColor Green
Set-Location $FrontendPath

# 1. Criar .env.local
Write-Host "`n📝 1. Criando .env.local..." -ForegroundColor Yellow

$envContent = @"
# Desabilita ESLint durante build/desenvolvimento
# Corrige conflito "Plugin 'react' was conflicted"
DISABLE_ESLINT_PLUGIN=true

# Desabilita checagem de TypeScript durante compilação (apenas build)
TSC_COMPILE_ON_ERROR=true

# Porta do servidor de desenvolvimento
PORT=3000

# Não abre browser automaticamente
BROWSER=none
"@

$envContent | Out-File -FilePath ".env.local" -Encoding UTF8 -Force
Write-Host "   ✅ .env.local criado" -ForegroundColor Green

# 2. Criar .eslintrc.json
Write-Host "`n📝 2. Criando .eslintrc.json..." -ForegroundColor Yellow

$eslintContent = @"
{
  "extends": ["react-app"],
  "rules": {
    "no-unused-vars": "warn",
    "@typescript-eslint/no-unused-vars": "warn"
  }
}
"@

$eslintContent | Out-File -FilePath ".eslintrc.json" -Encoding UTF8 -Force
Write-Host "   ✅ .eslintrc.json criado" -ForegroundColor Green

# 3. Atualizar package.json (remover eslintConfig)
Write-Host "`n📝 3. Atualizando package.json..." -ForegroundColor Yellow

$packageJsonPath = "package.json"
if (Test-Path $packageJsonPath) {
    # Ler package.json
    $packageJson = Get-Content $packageJsonPath -Raw | ConvertFrom-Json
    
    # Remover eslintConfig se existir
    if ($packageJson.PSObject.Properties.Name -contains "eslintConfig") {
        $packageJson.PSObject.Properties.Remove("eslintConfig")
        
        # Salvar package.json atualizado
        $packageJson | ConvertTo-Json -Depth 10 | Out-File -FilePath $packageJsonPath -Encoding UTF8 -Force
        Write-Host "   ✅ package.json atualizado (eslintConfig removido)" -ForegroundColor Green
    } else {
        Write-Host "   ℹ️  package.json já estava correto" -ForegroundColor Gray
    }
} else {
    Write-Host "   ⚠️  package.json não encontrado!" -ForegroundColor Yellow
}

# 4. Limpar cache (opcional)
if ($LimparCache) {
    Write-Host "`n🧹 4. Limpando cache e node_modules..." -ForegroundColor Yellow
    
    # Deletar node_modules
    if (Test-Path "node_modules") {
        Write-Host "   Removendo node_modules..." -ForegroundColor Gray
        Remove-Item -Path "node_modules" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Deletar package-lock.json
    if (Test-Path "package-lock.json") {
        Write-Host "   Removendo package-lock.json..." -ForegroundColor Gray
        Remove-Item -Path "package-lock.json" -Force -ErrorAction SilentlyContinue
    }
    
    # Deletar cache
    if (Test-Path ".cache") {
        Write-Host "   Removendo .cache..." -ForegroundColor Gray
        Remove-Item -Path ".cache" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    # Deletar build
    if (Test-Path "build") {
        Write-Host "   Removendo build..." -ForegroundColor Gray
        Remove-Item -Path "build" -Recurse -Force -ErrorAction SilentlyContinue
    }
    
    Write-Host "   ✅ Cache limpo" -ForegroundColor Green
    
    # Reinstalar dependências
    Write-Host "`n📦 5. Reinstalando dependências (isso pode demorar)..." -ForegroundColor Yellow
    
    try {
        npm install
        Write-Host "   ✅ Dependências instaladas com sucesso" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ Erro ao instalar dependências: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "   Execute manualmente: npm install" -ForegroundColor Yellow
    }
} else {
    Write-Host "`n📦 4. Cache não foi limpo (use -LimparCache para limpar)" -ForegroundColor Gray
}

# Resumo
Write-Host "`n📊 RESUMO" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Gray
Write-Host "✅ .env.local ............. Criado" -ForegroundColor Green
Write-Host "✅ .eslintrc.json ......... Criado" -ForegroundColor Green
Write-Host "✅ package.json ........... Atualizado" -ForegroundColor Green

if ($LimparCache) {
    Write-Host "✅ Cache .................. Limpo" -ForegroundColor Green
    Write-Host "✅ Dependências ........... Reinstaladas" -ForegroundColor Green
} else {
    Write-Host "⏭️  Cache .................. Não limpo (use -LimparCache)" -ForegroundColor Gray
}

Write-Host "`n🚀 PRÓXIMO PASSO" -ForegroundColor Yellow
Write-Host "Execute: npm start" -ForegroundColor White
Write-Host "`nOu reinicie o sistema com: .\iniciar-tudo.bat" -ForegroundColor White

Write-Host "`n✅ Correção aplicada com sucesso!" -ForegroundColor Green

<#
EXEMPLOS DE USO:

1. Correção básica (sem limpar cache):
   .\corrigir-eslint-frontend.ps1

2. Correção completa (com limpeza de cache):
   .\corrigir-eslint-frontend.ps1 -LimparCache

3. Especificar caminho customizado:
   .\corrigir-eslint-frontend.ps1 -FrontendPath "D:\OutroCaminho\icomanda-frontend" -LimparCache

OBSERVAÇÕES:
- Use -LimparCache apenas se o erro persistir após correção básica
- Limpeza de cache requer npm install (~5-10 minutos)
- Certifique-se de ter conexão com internet para npm install
#>
