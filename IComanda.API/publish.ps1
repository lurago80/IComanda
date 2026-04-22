# Script de publicacao para IComanda API
# Publica o backend .NET e faz build do frontend React

Write-Host "Iniciando publicacao do IComanda..." -ForegroundColor Cyan
Write-Host ""

# Obter o diretorio do script
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir

# Verificar se estamos no diretorio correto
if (Test-Path "$scriptDir\IComanda.API.csproj") {
    # Ja estamos no diretorio IComanda.API
    $apiDir = $scriptDir
} else {
    # Tentar encontrar o diretorio IComanda.API
    if (Test-Path "$rootDir\IComanda.API\IComanda.API.csproj") {
        $apiDir = "$rootDir\IComanda.API"
    } else {
        Write-Host "Erro: Nao foi possivel encontrar o diretorio IComanda.API!" -ForegroundColor Red
        exit 1
    }
}

Set-Location $apiDir

# 1. Publicar Backend (.NET)
Write-Host "Publicando Backend (.NET)..." -ForegroundColor Yellow

# Criar diretorio de publicacao se nao existir
$publishDir = ".\publish"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir | Out-Null

# Publicar backend
dotnet publish -c Release -o $publishDir --self-contained false

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao publicar backend!" -ForegroundColor Red
    exit 1
}

Write-Host "Backend publicado com sucesso em: $publishDir" -ForegroundColor Green
Write-Host ""

# 2. Build Frontend (React)
Write-Host "Fazendo build do Frontend (React)..." -ForegroundColor Yellow

$frontendDir = Join-Path $apiDir "icomanda-frontend"

if (-not (Test-Path $frontendDir)) {
    Write-Host "Erro: Diretorio do frontend nao encontrado: $frontendDir" -ForegroundColor Red
    exit 1
}

Set-Location $frontendDir

# Instalar dependencias se necessario
if (-not (Test-Path "node_modules")) {
    Write-Host "Instalando dependencias do frontend..." -ForegroundColor Yellow
    npm install
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Erro ao instalar dependencias do frontend!" -ForegroundColor Red
        exit 1
    }
}

# Build do frontend
npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao fazer build do frontend!" -ForegroundColor Red
    exit 1
}

Write-Host "Frontend buildado com sucesso em: build" -ForegroundColor Green
Write-Host ""

# Voltar para o diretorio da API
Set-Location $apiDir

Write-Host "Publicacao concluida com sucesso!" -ForegroundColor Green
Write-Host ""
Write-Host "Arquivos publicados:" -ForegroundColor Cyan
Write-Host "   Backend: $apiDir\publish\" -ForegroundColor White
Write-Host "   Frontend: $frontendDir\build\" -ForegroundColor White
Write-Host ""
Write-Host "Para executar o backend publicado:" -ForegroundColor Cyan
Write-Host "   cd $apiDir\publish" -ForegroundColor White
Write-Host "   dotnet IComanda.API.dll" -ForegroundColor White
Write-Host ""
