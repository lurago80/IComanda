# =============================================================================
# gerar-pacote-atualizacao.ps1
# Gera pacote de atualização com os arquivos modificados em 15/04/2026
# Inclui: segurança backend, tela de carrinho fullscreen, correção de login
# =============================================================================
# Uso: powershell -ExecutionPolicy Bypass -File gerar-pacote-atualizacao.ps1
# =============================================================================

$ErrorActionPreference = 'Stop'

$data        = Get-Date -Format 'yyyyMMdd_HHmm'
$nomePacote  = "ATUALIZACAO-iCOMANDA-$data"
$pastaTemp   = Join-Path $env:TEMP $nomePacote
$arquivoZip  = Join-Path $PSScriptRoot "$nomePacote.zip"
$raiz        = $PSScriptRoot

Write-Host ""
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "  iComanda - Gerador de Pacote de Atualização" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host ""

# Limpar pasta temp anterior
if (Test-Path $pastaTemp) { Remove-Item $pastaTemp -Recurse -Force }
New-Item -ItemType Directory -Path $pastaTemp | Out-Null

# -----------------------------------------------------------------------
# Arquivos modificados nesta atualização
# NÃO inclui appsettings.json (cada cliente tem sua connection string)
# -----------------------------------------------------------------------
$arquivos = @(
    "IComanda.API\Controllers\AuthController.cs",
    "IComanda.API\icomanda-frontend\src\App.tsx",
    "IComanda.API\icomanda-frontend\src\pages\Login.tsx",
    "IComanda.API\icomanda-frontend\src\pages\GruposPage.tsx",
    "IComanda.API\icomanda-frontend\src\pages\ReceberContasReceberPage.tsx",
    "IComanda.API\icomanda-frontend\src\vite-env.d.ts",
    "IComanda.API\icomanda-frontend\src\components\cart\CartDrawer.tsx"
)

Write-Host "[1/4] Copiando arquivos modificados..." -ForegroundColor Yellow
$pastaArquivos = Join-Path $pastaTemp "arquivos"
New-Item -ItemType Directory -Path $pastaArquivos | Out-Null

$copiados = 0
foreach ($arquivo in $arquivos) {
    $origem = Join-Path $raiz $arquivo
    if (-not (Test-Path $origem)) {
        Write-Warning "  ⚠  Não encontrado: $arquivo"
        continue
    }
    $destino      = Join-Path $pastaArquivos $arquivo
    $pastaDestino = Split-Path $destino -Parent
    if (-not (Test-Path $pastaDestino)) {
        New-Item -ItemType Directory -Path $pastaDestino -Force | Out-Null
    }
    Copy-Item $origem $destino
    $copiados++
    Write-Host "  ✅ $arquivo" -ForegroundColor Green
}

# -----------------------------------------------------------------------
# Criar INSTALAR.ps1
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "[2/4] Criando instalador..." -ForegroundColor Yellow

$instalador = @'
# =============================================================
# INSTALAR.ps1 - Atualização iComanda
# Execute com duplo clique em INSTALAR.bat
# =============================================================
$ErrorActionPreference = 'Continue'
$pastaScript = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "   iComanda - Instalando Atualização  " -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# --- Detectar pasta de instalação ---
$candidatos = @("C:\IComanda", "C:\VS_Code\icomanda")
$pastaInstalacao = $null
foreach ($c in $candidatos) {
    if (Test-Path (Join-Path $c "IComanda.API\IComanda.API.csproj")) {
        $pastaInstalacao = $c
        break
    }
}

if (-not $pastaInstalacao) {
    Write-Host "Pasta do iComanda não detectada automaticamente." -ForegroundColor Yellow
    $pastaInstalacao = Read-Host "Digite o caminho completo de instalação (ex: C:\iCOMANDA)"
    if (-not (Test-Path (Join-Path $pastaInstalacao "IComanda.API\IComanda.API.csproj"))) {
        Write-Host "❌ Caminho inválido. Instalação cancelada." -ForegroundColor Red
        Read-Host "Pressione ENTER para sair"
        exit 1
    }
}

Write-Host "📂 Instalando em: $pastaInstalacao" -ForegroundColor Green
Write-Host ""

# --- Parar processos ---
Write-Host "⏹  Parando processos do iComanda..." -ForegroundColor Yellow
@("IComanda.API", "dotnet") | ForEach-Object {
    Stop-Process -Name $_ -Force -ErrorAction SilentlyContinue
}
$portas = @(65375, 3000, 3001)
Get-NetTCPConnection -LocalPort $portas -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
# Aguardar processos encerrarem
$limite = 10
$tentativas = 0
do {
    Start-Sleep -Seconds 1
    $tentativas++
    $ocupadas = (Get-NetTCPConnection -LocalPort $portas -State Listen -ErrorAction SilentlyContinue).Count
} while ($ocupadas -gt 0 -and $tentativas -lt $limite)

Write-Host "  ✅ Processos encerrados." -ForegroundColor Green
Write-Host ""

# --- Copiar arquivos ---
Write-Host "📋 Copiando arquivos..." -ForegroundColor Yellow
$pastaArquivos = Join-Path $pastaScript "arquivos"
$erros = 0

Get-ChildItem -Path $pastaArquivos -Recurse -File | ForEach-Object {
    $relativo = $_.FullName.Substring($pastaArquivos.Length + 1)
    $destino  = Join-Path $pastaInstalacao $relativo
    $dir      = Split-Path $destino -Parent
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    try {
        Copy-Item $_.FullName $destino -Force
        Write-Host "  ✅ $relativo" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ ERRO: $relativo" -ForegroundColor Red
        Write-Host "     $_" -ForegroundColor DarkRed
        $erros++
    }
}

Write-Host ""
if ($erros -gt 0) {
    Write-Host "⚠️  $erros arquivo(s) com erro. Verifique as permissões e tente como Administrador." -ForegroundColor Yellow
} else {
    Write-Host "✅ Todos os arquivos copiados com sucesso!" -ForegroundColor Green
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "   Atualização concluída!" -ForegroundColor Green
Write-Host ""
Write-Host "   Próximo passo:" -ForegroundColor White
Write-Host "   Execute o iniciar-tudo.bat normalmente" -ForegroundColor White
Write-Host "   (o sistema irá recompilar automaticamente)" -ForegroundColor Gray
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Read-Host "Pressione ENTER para sair"
'@

$instalador | Out-File -FilePath (Join-Path $pastaTemp "INSTALAR.ps1") -Encoding UTF8

# Criar INSTALAR.bat (launcher para o PS1)
@'
@echo off
chcp 65001 >nul
echo.
echo Iniciando instalador de atualização iComanda...
echo.
powershell -ExecutionPolicy Bypass -File "%~dp0INSTALAR.ps1"
'@ | Out-File -FilePath (Join-Path $pastaTemp "INSTALAR.bat") -Encoding ASCII

# -----------------------------------------------------------------------
# Criar LEIA-ME.txt
# -----------------------------------------------------------------------
$listaArquivos = ($arquivos | ForEach-Object { "  - $_" }) -join "`r`n"
@"
ATUALIZAÇÃO iCOMANDA - $data
========================================

COMO INSTALAR:
  1. Descompacte este ZIP em qualquer pasta
  2. Dê duplo clique em INSTALAR.bat
  3. Aguarde a conclusão
  4. Inicie o sistema normalmente (iniciar-tudo.bat)

ARQUIVOS ATUALIZADOS NESTE PACOTE:
$listaArquivos

O QUE MUDOU:
  - Carrinho (Comanda e PDV Rápido) agora ocupa a tela toda
  - Campo de observação dos itens maior
  - Correção da tela branca ao abrir o sistema
  - Erro de login exibe mensagem inline (sem popup)
  - Melhorias de segurança no backend

IMPORTANTE:
  - NÃO substitui appsettings.json (sua configuração de banco fica intacta)
  - Em caso de dúvida, execute INSTALAR.bat como Administrador
"@ | Out-File -FilePath (Join-Path $pastaTemp "LEIA-ME.txt") -Encoding UTF8

# -----------------------------------------------------------------------
# Gerar ZIP
# -----------------------------------------------------------------------
Write-Host ""
Write-Host "[3/4] Compactando pacote..." -ForegroundColor Yellow

if (Test-Path $arquivoZip) { Remove-Item $arquivoZip -Force }
Compress-Archive -Path "$pastaTemp\*" -DestinationPath $arquivoZip

Remove-Item $pastaTemp -Recurse -Force

# -----------------------------------------------------------------------
# Resultado
# -----------------------------------------------------------------------
$tamanho = [math]::Round((Get-Item $arquivoZip).Length / 1KB, 1)

Write-Host ""
Write-Host "======================================================" -ForegroundColor Green
Write-Host "  Pacote gerado com sucesso!" -ForegroundColor Green
Write-Host "  $copiados arquivo(s) incluidos" -ForegroundColor White
Write-Host "  Tamanho: $tamanho KB" -ForegroundColor White
Write-Host ""
Write-Host "  ZIP salvo em:" -ForegroundColor White
Write-Host "  $arquivoZip" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Para instalar no cliente:" -ForegroundColor Gray
Write-Host "  1. Copie o ZIP para a maquina do cliente" -ForegroundColor Gray
Write-Host "  2. Descompacte e execute INSTALAR.bat" -ForegroundColor Gray
Write-Host ""
