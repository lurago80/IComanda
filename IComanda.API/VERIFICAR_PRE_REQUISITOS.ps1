# ============================================
# VERIFICADOR DE PRÉ-REQUISITOS ICOMANDA
# ============================================
# Este script apenas verifica o que está instalado
# sem fazer instalação
# ============================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "🔍 VERIFICANDO PRÉ-REQUISITOS ICOMANDA" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$faltando = @()
$instalado = @()
$avisos = @()

# 1. Verificar .NET SDK 8.0
Write-Host "📦 .NET SDK 8.0..." -NoNewline
try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $version = [version]$dotnetVersion
        if ($version.Major -ge 8) {
            Write-Host " ✅ $dotnetVersion" -ForegroundColor Green
            $instalado += ".NET SDK $dotnetVersion"
        } else {
            Write-Host " ❌ Versão $dotnetVersion (necessário 8.0+)" -ForegroundColor Red
            $faltando += ".NET SDK 8.0 ou superior (atual: $dotnetVersion)"
        }
    } else {
        throw "dotnet não encontrado"
    }
} catch {
    Write-Host " ❌ Não encontrado" -ForegroundColor Red
    $faltando += ".NET SDK 8.0 ou superior"
    Write-Host "   📥 Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
}

# 2. Verificar Node.js
Write-Host "📦 Node.js 16+..." -NoNewline
try {
    $nodeVersion = node --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $version = $nodeVersion -replace 'v', ''
        $majorVersion = [int]($version.Split('.')[0])
        if ($majorVersion -ge 16) {
            Write-Host " ✅ $nodeVersion" -ForegroundColor Green
            $instalado += "Node.js $nodeVersion"
        } else {
            Write-Host " ❌ Versão $nodeVersion (necessário 16+)" -ForegroundColor Red
            $faltando += "Node.js 16 ou superior (atual: $nodeVersion)"
        }
    } else {
        throw "node não encontrado"
    }
} catch {
    Write-Host " ❌ Não encontrado" -ForegroundColor Red
    $faltando += "Node.js 16 ou superior"
    Write-Host "   📥 Download: https://nodejs.org/" -ForegroundColor Yellow
}

# 3. Verificar npm
Write-Host "📦 npm..." -NoNewline
try {
    $npmVersion = npm --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host " ✅ $npmVersion" -ForegroundColor Green
        $instalado += "npm $npmVersion"
    } else {
        throw "npm não encontrado"
    }
} catch {
    Write-Host " ❌ Não encontrado" -ForegroundColor Red
    $faltando += "npm (geralmente vem com Node.js)"
}

# 4. Verificar estrutura do projeto
Write-Host "📁 Estrutura do projeto..." -NoNewline
$backendPath = "IComanda.API.csproj"
$frontendPath = "icomanda-frontend\package.json"

$backendOk = Test-Path $backendPath
$frontendOk = Test-Path $frontendPath

if ($backendOk -and $frontendOk) {
    Write-Host " ✅ OK" -ForegroundColor Green
} elseif ($backendOk) {
    Write-Host " ⚠️  Backend OK, Frontend não encontrado" -ForegroundColor Yellow
    $avisos += "Pasta icomanda-frontend não encontrada"
} elseif ($frontendOk) {
    Write-Host " ⚠️  Frontend OK, Backend não encontrado" -ForegroundColor Yellow
    $avisos += "Arquivo IComanda.API.csproj não encontrado"
} else {
    Write-Host " ❌ Não encontrado" -ForegroundColor Red
    $faltando += "Estrutura do projeto (execute na pasta raiz)"
}

# 5. Verificar se dependências já estão instaladas
if ($backendOk) {
    Write-Host "📦 Dependências do Backend..." -NoNewline
    $objPath = "obj\project.assets.json"
    if (Test-Path $objPath) {
        $assetsTime = (Get-Item $objPath).LastWriteTime
        $projTime = (Get-Item $backendPath).LastWriteTime
        if ($assetsTime -gt $projTime) {
            Write-Host " ✅ Instaladas" -ForegroundColor Green
        } else {
            Write-Host " ⚠️  Podem estar desatualizadas" -ForegroundColor Yellow
            $avisos += "Execute 'dotnet restore' para atualizar dependências do backend"
        }
    } else {
        Write-Host " ❌ Não instaladas" -ForegroundColor Red
        $avisos += "Execute 'dotnet restore' para instalar dependências do backend"
    }
}

if ($frontendOk) {
    Write-Host "📦 Dependências do Frontend..." -NoNewline
    $nodeModulesPath = "icomanda-frontend\node_modules"
    if (Test-Path $nodeModulesPath) {
        $nodeModulesTime = (Get-Item $nodeModulesPath).LastWriteTime
        $packageJsonTime = (Get-Item $frontendPath).LastWriteTime
        if ($nodeModulesTime -gt $packageJsonTime) {
            Write-Host " ✅ Instaladas" -ForegroundColor Green
        } else {
            Write-Host " ⚠️  Podem estar desatualizadas" -ForegroundColor Yellow
            $avisos += "Execute 'npm install' na pasta icomanda-frontend"
        }
    } else {
        Write-Host " ❌ Não instaladas" -ForegroundColor Red
        $avisos += "Execute 'npm install' na pasta icomanda-frontend"
    }
}

# ============================================
# RESUMO
# ============================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "📊 RESUMO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($instalado.Count -gt 0) {
    Write-Host "✅ INSTALADO:" -ForegroundColor Green
    foreach ($item in $instalado) {
        Write-Host "   • $item" -ForegroundColor Green
    }
    Write-Host ""
}

if ($faltando.Count -gt 0) {
    Write-Host "❌ FALTANDO:" -ForegroundColor Red
    foreach ($item in $faltando) {
        Write-Host "   • $item" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Execute INSTALAR.bat para instalar automaticamente" -ForegroundColor Yellow
    Write-Host ""
}

if ($avisos.Count -gt 0) {
    Write-Host "⚠️  AVISOS:" -ForegroundColor Yellow
    foreach ($aviso in $avisos) {
        Write-Host "   • $aviso" -ForegroundColor Yellow
    }
    Write-Host ""
}

if ($faltando.Count -eq 0 -and $avisos.Count -eq 0) {
    Write-Host "✅ Tudo pronto! Você pode executar o sistema." -ForegroundColor Green
    Write-Host ""
}

Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
