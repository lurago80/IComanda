# Script para preparar pacote de correção do erro 401 para envio ao cliente
# Execute este script no ambiente de desenvolvimento

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Preparando Pacote de Correção 401" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$packageDir = ".\correcao-401-package"
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$packageName = "correcao-401_$timestamp"
$finalPackage = ".\$packageName"

# Criar diretório temporário
if (Test-Path $packageDir) {
    Remove-Item -Path $packageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

Write-Host "📦 Copiando arquivos do frontend..." -ForegroundColor Yellow

# Frontend
$frontendDir = "$packageDir\icomanda-frontend\src"
New-Item -ItemType Directory -Path "$frontendDir\pages" -Force | Out-Null
New-Item -ItemType Directory -Path "$frontendDir\services" -Force | Out-Null

Copy-Item ".\IComanda.API\icomanda-frontend\src\pages\Login.tsx" -Destination "$frontendDir\pages\" -Force
Copy-Item ".\IComanda.API\icomanda-frontend\src\services\api.ts" -Destination "$frontendDir\services\" -Force

Write-Host "✅ Frontend copiado" -ForegroundColor Green

Write-Host "📦 Copiando arquivos do backend..." -ForegroundColor Yellow

# Backend - Repositories
$repoImplDir = "$packageDir\IComanda.API\Repositories\Implementations"
$repoIntfDir = "$packageDir\IComanda.API\Repositories\Interfaces"
New-Item -ItemType Directory -Path $repoImplDir -Force | Out-Null
New-Item -ItemType Directory -Path $repoIntfDir -Force | Out-Null

# Copiar repositórios atualizados
$reposImpl = @(
    "OperadorComissaoRepository.cs",
    "RecebimentoRepository.cs"
)

$reposIntf = @(
    "IGrupoProdutoRepository.cs",
    "IOperadorComissaoRepository.cs",
    "IRecebimentoRepository.cs"
)

foreach ($file in $reposImpl) {
    $source = ".\IComanda.API\Repositories\Implementations\$file"
    if (Test-Path $source) {
        Copy-Item $source -Destination $repoImplDir -Force
        Write-Host "  ✓ $file" -ForegroundColor Gray
    } else {
        Write-Host "  ⚠ $file não encontrado" -ForegroundColor Yellow
    }
}

foreach ($file in $reposIntf) {
    $source = ".\IComanda.API\Repositories\Interfaces\$file"
    if (Test-Path $source) {
        Copy-Item $source -Destination $repoIntfDir -Force
        Write-Host "  ✓ $file" -ForegroundColor Gray
    } else {
        Write-Host "  ⚠ $file não encontrado" -ForegroundColor Yellow
    }
}

# Backend - Controllers
$controllersDir = "$packageDir\IComanda.API\Controllers"
New-Item -ItemType Directory -Path $controllersDir -Force | Out-Null

$controllers = @(
    "GrupoProdutoController.cs",
    "OperadorComissaoController.cs",
    "RecebimentoController.cs"
)

foreach ($file in $controllers) {
    $source = ".\IComanda.API\Controllers\$file"
    if (Test-Path $source) {
        Copy-Item $source -Destination $controllersDir -Force
        Write-Host "  ✓ $file" -ForegroundColor Gray
    } else {
        Write-Host "  ⚠ $file não encontrado" -ForegroundColor Yellow
    }
}

Write-Host "✅ Backend copiado" -ForegroundColor Green

# Copiar documentação
Write-Host "📦 Copiando documentação..." -ForegroundColor Yellow
Copy-Item ".\CORRECAO_ERRO_401_LOGIN.md" -Destination "$packageDir\" -Force
Write-Host "✅ Documentação copiada" -ForegroundColor Green

# Criar arquivo README com instruções rápidas
$readmeContent = @"
# Pacote de Correção - Erro 401 após Login

## Arquivos incluídos
- Frontend: Login.tsx, api.ts (com logs melhorados)
- Backend: Repositórios e Controllers atualizados
- Documentação: CORRECAO_ERRO_401_LOGIN.md

## Instalação Rápida

1. Pare os serviços no cliente:
   ``````powershell
   .\parar-tudo.bat
   ``````

2. Copie os arquivos deste pacote para as pastas correspondentes no servidor do cliente

3. Recompile o frontend:
   ``````powershell
   cd IComanda.API\icomanda-frontend
   npm run build
   ``````

4. Reinicie os serviços:
   ``````powershell
   .\iniciar-tudo.bat
   ``````

## Verificação

Após reiniciar, abra o navegador no cliente e:
1. Pressione F12 para abrir o DevTools
2. Vá para a aba Console
3. Faça login
4. Verifique os logs:
   - Deve aparecer "✅ [Login] Login bem-sucedido"
   - Deve aparecer "🔑 [API] Token anexado ao request"
   - As requisições para /vendas/abertas devem retornar 200 OK

## Documentação Completa

Consulte o arquivo CORRECAO_ERRO_401_LOGIN.md para instruções detalhadas,
solução de problemas e diagnóstico avançado.

---
Gerado em: $timestamp
"@

Set-Content -Path "$packageDir\README.txt" -Value $readmeContent -Encoding UTF8

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Compactando pacote..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Compactar o pacote
if (Test-Path "$finalPackage.zip") {
    Remove-Item "$finalPackage.zip" -Force
}

Compress-Archive -Path "$packageDir\*" -DestinationPath "$finalPackage.zip" -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ PACOTE CRIADO COM SUCESSO!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Arquivo: $finalPackage.zip" -ForegroundColor Yellow
Write-Host "📊 Tamanho: $((Get-Item "$finalPackage.zip").Length / 1KB) KB" -ForegroundColor Yellow
Write-Host ""
Write-Host "📤 Envie este arquivo para o cliente e siga as instruções do README.txt" -ForegroundColor Cyan
Write-Host ""

# Limpar diretório temporário
Remove-Item -Path $packageDir -Recurse -Force

# Abrir o Explorer no local do arquivo
explorer.exe /select,"$finalPackage.zip"
