# ============================================
# INSTALADOR ICOMANDA - Backend e Frontend
# ============================================
# Este script verifica pré-requisitos e instala
# todas as dependências necessárias
# ============================================

param(
    [switch]$SkipChecks = $false,
    [switch]$BackendOnly = $false,
    [switch]$FrontendOnly = $false
)

$ErrorActionPreference = "Stop"

# Cores para output
function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Blue
}

# ============================================
# VERIFICAÇÃO DE PRÉ-REQUISITOS
# ============================================

Write-Header "🔍 VERIFICANDO PRÉ-REQUISITOS"

$faltando = @()
$instalado = @()

# 1. Verificar .NET SDK 8.0
Write-Info "Verificando .NET SDK 8.0..."
try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $version = [version]$dotnetVersion
        if ($version.Major -ge 8) {
            Write-Success ".NET SDK encontrado: $dotnetVersion"
            $instalado += ".NET SDK $dotnetVersion"
        } else {
            Write-Error ".NET SDK 8.0 ou superior é necessário. Versão encontrada: $dotnetVersion"
            $faltando += ".NET SDK 8.0 ou superior (atual: $dotnetVersion)"
        }
    } else {
        throw "dotnet não encontrado"
    }
} catch {
    Write-Error ".NET SDK não encontrado"
    $faltando += ".NET SDK 8.0 ou superior"
    Write-Info "  📥 Download: https://dotnet.microsoft.com/download/dotnet/8.0"
}

# 2. Verificar Node.js
if (-not $BackendOnly) {
    Write-Info "Verificando Node.js..."
    try {
        $nodeVersion = node --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            $version = $nodeVersion -replace 'v', ''
            $majorVersion = [int]($version.Split('.')[0])
            if ($majorVersion -ge 16) {
                Write-Success "Node.js encontrado: $nodeVersion"
                $instalado += "Node.js $nodeVersion"
            } else {
                Write-Error "Node.js 16 ou superior é necessário. Versão encontrada: $nodeVersion"
                $faltando += "Node.js 16 ou superior (atual: $nodeVersion)"
            }
        } else {
            throw "node não encontrado"
        }
    } catch {
        Write-Error "Node.js não encontrado"
        $faltando += "Node.js 16 ou superior"
        Write-Info "  📥 Download: https://nodejs.org/"
    }

    # 3. Verificar npm
    Write-Info "Verificando npm..."
    try {
        $npmVersion = npm --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "npm encontrado: $npmVersion"
            $instalado += "npm $npmVersion"
        } else {
            throw "npm não encontrado"
        }
    } catch {
        Write-Error "npm não encontrado"
        $faltando += "npm (geralmente vem com Node.js)"
    }
}

# 4. Verificar se está no diretório correto
Write-Info "Verificando estrutura do projeto..."
$backendPath = "IComanda.API.csproj"
$frontendPath = "icomanda-frontend\package.json"

if (-not (Test-Path $backendPath)) {
    Write-Error "Arquivo IComanda.API.csproj não encontrado!"
    Write-Info "  Execute este script na pasta raiz do projeto (onde está IComanda.API.csproj)"
    exit 1
}

if (-not $BackendOnly -and -not (Test-Path $frontendPath)) {
    Write-Error "Arquivo icomanda-frontend\package.json não encontrado!"
    Write-Info "  A pasta icomanda-frontend não foi encontrada"
    exit 1
}

Write-Success "Estrutura do projeto OK"

# ============================================
# RESUMO DE VERIFICAÇÃO
# ============================================

Write-Header "📊 RESUMO DA VERIFICAÇÃO"

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
    
    if (-not $SkipChecks) {
        Write-Error "Por favor, instale os itens faltantes antes de continuar."
        Write-Info "Ou execute com -SkipChecks para pular a verificação (não recomendado)"
        Write-Host ""
        Write-Host "Pressione qualquer tecla para sair..."
        $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        exit 1
    } else {
        Write-Warning "Continuando apesar de itens faltantes (SkipChecks ativado)..."
    }
} else {
    Write-Success "Todos os pré-requisitos estão instalados!"
}

# ============================================
# INSTALAÇÃO DO BACKEND
# ============================================

if (-not $FrontendOnly) {
    Write-Header "📦 INSTALANDO BACKEND (.NET)"

    $backendDir = Split-Path -Parent $backendPath
    if ($backendDir -eq "") {
        $backendDir = "."
    }

    Push-Location $backendDir

    try {
        Write-Info "Restaurando pacotes NuGet..."
        dotnet restore --verbosity quiet

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Falha ao restaurar pacotes NuGet"
            exit 1
        }

        Write-Success "Pacotes NuGet restaurados com sucesso"

        Write-Info "Verificando build..."
        dotnet build --no-incremental --verbosity quiet | Out-Null

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Falha ao compilar o projeto"
            Write-Info "Execute 'dotnet build' manualmente para ver os erros"
            exit 1
        }

        Write-Success "Backend compilado com sucesso"

    } catch {
        Write-Error "Erro ao instalar backend: $_"
        exit 1
    } finally {
        Pop-Location
    }
}

# ============================================
# INSTALAÇÃO DO FRONTEND
# ============================================

if (-not $BackendOnly) {
    Write-Header "📦 INSTALANDO FRONTEND (Node.js)"

    if (-not (Test-Path "icomanda-frontend")) {
        Write-Error "Pasta icomanda-frontend não encontrada!"
        exit 1
    }

    Push-Location "icomanda-frontend"

    try {
        Write-Info "Instalando dependências npm (isso pode demorar alguns minutos)..."
        
        # Verificar se node_modules já existe
        if (Test-Path "node_modules") {
            Write-Info "node_modules encontrado. Verificando se está atualizado..."
            $packageJsonTime = (Get-Item "package.json").LastWriteTime
            $nodeModulesTime = (Get-Item "node_modules" -ErrorAction SilentlyContinue).LastWriteTime
            
            if ($nodeModulesTime -lt $packageJsonTime) {
                Write-Warning "node_modules parece desatualizado. Reinstalando..."
                Remove-Item "node_modules" -Recurse -Force -ErrorAction SilentlyContinue
            } else {
                Write-Info "node_modules parece atualizado. Pulando instalação..."
                Write-Success "Dependências já instaladas"
                Pop-Location
                continue
            }
        }

        # Instalar dependências
        npm install --loglevel=error

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Falha ao instalar dependências npm"
            Write-Info "Tente executar 'npm install' manualmente na pasta icomanda-frontend"
            exit 1
        }

        Write-Success "Dependências npm instaladas com sucesso"

        # Verificar se build funciona
        Write-Info "Verificando se o projeto compila..."
        $env:CI = "true"  # Evitar prompts interativos
        npm run build --silent 2>&1 | Out-Null

        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Build do frontend falhou, mas dependências foram instaladas"
            Write-Info "Você pode tentar executar 'npm run build' manualmente depois"
        } else {
            Write-Success "Frontend compilado com sucesso"
        }

    } catch {
        Write-Error "Erro ao instalar frontend: $_"
        exit 1
    } finally {
        Pop-Location
    }
}

# ============================================
# CONCLUSÃO
# ============================================

Write-Header "✅ INSTALAÇÃO CONCLUÍDA"

Write-Success "Backend e Frontend instalados com sucesso!"
Write-Host ""

Write-Info "PRÓXIMOS PASSOS:"
Write-Host ""
Write-Host "1. BACKEND:" -ForegroundColor Cyan
Write-Host "   cd IComanda.API" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""

Write-Host "2. FRONTEND:" -ForegroundColor Cyan
Write-Host "   cd icomanda-frontend" -ForegroundColor Gray
Write-Host "   npm start" -ForegroundColor Gray
Write-Host ""

Write-Info "Ou use os scripts de inicialização:"
Write-Host "   .\INICIAR_TUDO.bat" -ForegroundColor Yellow
Write-Host ""

Write-Host "Pressione qualquer tecla para sair..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
