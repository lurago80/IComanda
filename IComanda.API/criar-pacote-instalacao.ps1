# Script para criar pacote ZIP completo para instalação no cliente
# Este script faz o deploy completo e compacta tudo em um ZIP pronto para uso

param(
    [string]$OutputZip = "IComanda-Instalacao.zip"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  📦 Criando Pacote de Instalação" -ForegroundColor Cyan
Write-Host "  IComanda - Sistema de Comandas" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Obter diretório do script
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# Diretório temporário para o pacote
$tempPackageDir = ".\temp-package-instalacao"
$packageDir = ".\IComanda-Pacote-Instalacao"

# Limpar diretórios anteriores
Write-Host "🧹 Limpando diretórios anteriores..." -ForegroundColor Yellow
if (Test-Path $tempPackageDir) {
    Remove-Item $tempPackageDir -Recurse -Force
}
if (Test-Path $packageDir) {
    Remove-Item $packageDir -Recurse -Force
}
if (Test-Path $OutputZip) {
    Remove-Item $OutputZip -Force
    Write-Host "   ✅ Arquivo ZIP anterior removido" -ForegroundColor Green
}

New-Item -ItemType Directory -Path $tempPackageDir | Out-Null
New-Item -ItemType Directory -Path $packageDir | Out-Null

Write-Host ""

# 1. Executar deploy completo
Write-Host "📦 Executando deploy completo..." -ForegroundColor Yellow
Write-Host ""

$deployScript = Join-Path $scriptDir "deploy.ps1"
if (Test-Path $deployScript) {
    & $deployScript -OutputPath $tempPackageDir -IncludeFrontend:$true
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Erro ao executar deploy!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "⚠️  Script deploy.ps1 não encontrado. Executando deploy manual..." -ForegroundColor Yellow
    
    # Deploy manual do backend
    Write-Host "   📦 Publicando Backend..." -ForegroundColor Yellow
    $publishDir = ".\publish"
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
    }
    
    dotnet publish -c Release -o $publishDir --self-contained false
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Erro ao publicar backend!" -ForegroundColor Red
        exit 1
    }
    
    New-Item -ItemType Directory -Path "$tempPackageDir\backend" -Force | Out-Null
    Copy-Item -Path "$publishDir\*" -Destination "$tempPackageDir\backend\" -Recurse -Force
    
    # Deploy manual do frontend
    Write-Host "   📦 Fazendo build do Frontend..." -ForegroundColor Yellow
    $frontendDir = Join-Path $scriptDir "icomanda-frontend"
    if (Test-Path $frontendDir) {
        Set-Location $frontendDir
        
        if (-not (Test-Path "node_modules")) {
            Write-Host "   📥 Instalando dependências..." -ForegroundColor Yellow
            npm install --silent
        }
        
        npm run build
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Erro ao fazer build do frontend!" -ForegroundColor Red
            Set-Location $scriptDir
            exit 1
        }
        
        New-Item -ItemType Directory -Path "$tempPackageDir\frontend" -Force | Out-Null
        Copy-Item -Path "build\*" -Destination "$tempPackageDir\frontend\" -Recurse -Force
        
        Set-Location $scriptDir
    }
}

Write-Host "✅ Deploy concluído" -ForegroundColor Green
Write-Host ""

# 2. Copiar arquivos para o pacote final
Write-Host "📋 Organizando pacote final..." -ForegroundColor Yellow

# Criar diretórios de destino
New-Item -ItemType Directory -Path "$packageDir\backend" -Force | Out-Null
if (Test-Path "$tempPackageDir\frontend") {
    New-Item -ItemType Directory -Path "$packageDir\frontend" -Force | Out-Null
}

# Copiar backend
Copy-Item -Path "$tempPackageDir\backend\*" -Destination "$packageDir\backend\" -Recurse -Force

# Copiar frontend
if (Test-Path "$tempPackageDir\frontend") {
    Copy-Item -Path "$tempPackageDir\frontend\*" -Destination "$packageDir\frontend\" -Recurse -Force
}

# Remover arquivos desnecessários do backend
Write-Host "   🧹 Removendo arquivos desnecessários..." -ForegroundColor Yellow
$filesToRemove = @(
    "*.pdb",
    "*.xml",
    "*.json",
    "*.deps.json"
)

# Manter apenas DLLs e arquivos essenciais
Get-ChildItem -Path "$packageDir\backend" -Recurse -File | Where-Object {
    $ext = $_.Extension.ToLower()
    $ext -in @(".pdb", ".xml") -and $_.Name -notlike "appsettings*"
} | Remove-Item -Force

# 3. Criar arquivos de instalação e documentação
Write-Host "   📝 Criando documentação e scripts..." -ForegroundColor Yellow

# README principal
$readmeContent = @"
╔══════════════════════════════════════════════════════════════╗
║         ICOMANDA - Sistema de Comandas e Vendas              ║
║                  Pacote de Instalação                       ║
╚══════════════════════════════════════════════════════════════╝

📦 CONTEÚDO DO PACOTE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Este pacote contém:
  ✅ backend\     - Backend da API (.NET) publicado e pronto
  ✅ frontend\    - Frontend (React) buildado e pronto
  ✅ Scripts de instalação e configuração


🚀 INSTALAÇÃO RÁPIDA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1️⃣  INSTALAR .NET 8.0 RUNTIME
   
   Baixe e instale o .NET 8.0 Runtime:
   https://dotnet.microsoft.com/download/dotnet/8.0
   
   Escolha: .NET Desktop Runtime 8.0.x (Windows)
   
   Verifique a instalação:
   dotnet --version
   (deve mostrar 8.0.x ou superior)


2️⃣  CONFIGURAR BANCO DE DADOS
   
   Edite o arquivo: backend\appsettings.json
   
   Ajuste a ConnectionString com os dados do seu banco Firebird:
   
   {
     "ConnectionStrings": {
       "Firebird": "Server=localhost;Database=C:\\Caminho\\Para\\Seu\\Banco.FDB;User=SYSDBA;Password=sua_senha;Port=3050;"
     }
   }
   
   ⚠️ IMPORTANTE:
   - Server: IP ou nome do servidor Firebird (localhost se local)
   - Database: Caminho completo do arquivo .FDB
   - User: Usuário do Firebird (geralmente SYSDBA)
   - Password: Senha do Firebird


3️⃣  EXECUTAR A APLICAÇÃO
   
   Opção A - Executar manualmente:
   ────────────────────────────────
   Abra o CMD/PowerShell na pasta backend e execute:
   
   dotnet IComanda.API.dll
   
   Ou simplesmente dê duplo clique em: start-api.bat
   
   
   Opção B - Instalar como Serviço Windows (Recomendado):
   ──────────────────────────────────────────────────────
   1. Abra o CMD como Administrador
   2. Navegue até: backend\
   3. Execute: install-service.bat
   4. O serviço será criado e iniciado automaticamente
   
   Para verificar:
   sc query IComandaAPI
   
   Para iniciar/parar:
   net start IComandaAPI
   net stop IComandaAPI


4️⃣  ACESSAR O SISTEMA
   
   Após iniciar a aplicação, acesse:
   
   🌐 Frontend: http://localhost:65375
   📚 Swagger:  http://localhost:65375/swagger
   
   (A porta padrão é 65375, mas pode ser alterada no appsettings.json)


📋 CHECKLIST DE INSTALAÇÃO
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[ ] .NET 8.0 Runtime instalado
[ ] Firebird instalado e rodando
[ ] Banco de dados configurado
[ ] appsettings.json configurado com connection string correta
[ ] Aplicação executada ou serviço instalado
[ ] API acessível em http://localhost:65375/swagger
[ ] Frontend acessível e funcionando
[ ] Login testado e funcionando


🔧 CONFIGURAÇÕES ADICIONAIS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Mudar Porta da API:
───────────────────
Edite backend\appsettings.json:
{
  "Kestrel": {
    "Port": 8080
  }
}


Liberar Porta no Firewall:
───────────────────────────
Execute no PowerShell como Administrador:
New-NetFirewallRule -DisplayName "IComanda API" -Direction Inbound -LocalPort 65375 -Protocol TCP -Action Allow


Logs:
─────
Os logs ficam em: backend\logs\


🆘 PROBLEMAS COMUNS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

❌ "dotnet não é reconhecido"
   → Instale o .NET 8.0 Runtime
   → Reinicie o terminal/PowerShell

❌ "Cannot open database"
   → Verifique o caminho do banco no appsettings.json
   → Verifique se o Firebird está rodando
   → Verifique permissões de acesso ao arquivo .FDB

❌ "Port already in use"
   → Mude a porta no appsettings.json
   → Ou pare o processo que está usando a porta

❌ Serviço não inicia
   → Verifique os logs em backend\logs\
   → Verifique se o .NET Runtime está instalado
   → Verifique se o appsettings.json está correto


📞 SUPORTE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Em caso de problemas:
1. Verifique os logs em backend\logs\
2. Verifique o Event Viewer do Windows (se instalado como serviço)
3. Execute manualmente para ver erros:
   cd backend
   dotnet IComanda.API.dll


═══════════════════════════════════════════════════════════════
Versão do Pacote: $(Get-Date -Format "yyyy-MM-dd HH:mm")
═══════════════════════════════════════════════════════════════
"@

$readmeContent | Out-File -FilePath "$packageDir\LEIA-ME.txt" -Encoding UTF8

# Script de instalação rápida
$installQuickScript = @"
@echo off
chcp 65001 >nul
echo.
echo ╔══════════════════════════════════════════════════════════════╗
echo ║         ICOMANDA - Instalação Rápida                         ║
echo ╚══════════════════════════════════════════════════════════════╝
echo.

cd /d "%~dp0\backend"

echo [1/3] Verificando .NET Runtime...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo    ❌ .NET 8.0 Runtime não encontrado!
    echo.
    echo    Por favor, instale o .NET 8.0 Runtime primeiro.
    echo    Download: https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)
echo    ✅ .NET encontrado
echo.

echo [2/3] Verificando configuração...
if not exist "appsettings.json" (
    echo    ⚠️  appsettings.json não encontrado!
    echo.
    if exist "appsettings.json.example" (
        copy appsettings.json.example appsettings.json
        echo    ✅ Arquivo appsettings.json.example copiado para appsettings.json
        echo.
        echo    ⚠️  IMPORTANTE: Edite o arquivo appsettings.json e configure:
        echo       - ConnectionString do banco de dados
        echo       - Configurações de impressão (se necessário)
        echo.
        echo    Após configurar, execute este script novamente.
        echo.
        pause
        exit /b 1
    ) else (
        echo    ❌ Arquivo de configuração não encontrado!
        pause
        exit /b 1
    )
)
echo    ✅ Configuração encontrada
echo.

echo [3/3] Criando diretório de logs...
if not exist "logs" mkdir logs
echo    ✅ Diretório criado
echo.

echo ═══════════════════════════════════════════════════════════════
echo   ✅ Instalação concluída!
echo ═══════════════════════════════════════════════════════════════
echo.
echo Para executar a aplicação:
echo   1. Execute: start-api.bat
echo   2. Ou instale como serviço: install-service.bat (como Admin)
echo.
echo Acesse: http://localhost:65375
echo.
pause
"@

$installQuickScript | Out-File -FilePath "$packageDir\INSTALAR.bat" -Encoding UTF8

# Copiar documentação existente
if (Test-Path "GUIA_INSTALACAO.md") {
    Copy-Item -Path "GUIA_INSTALACAO.md" -Destination "$packageDir\" -Force
}
if (Test-Path "INSTRUCOES_INSTALACAO_CLIENTE.md") {
    Copy-Item -Path "INSTRUCOES_INSTALACAO_CLIENTE.md" -Destination "$packageDir\" -Force
}

# 4. Criar arquivo ZIP
Write-Host ""
Write-Host "📦 Compactando pacote em ZIP..." -ForegroundColor Yellow

# Usar Compress-Archive do PowerShell
$zipPath = Join-Path $scriptDir $OutputZip
Compress-Archive -Path "$packageDir\*" -DestinationPath $zipPath -Force

if (Test-Path $zipPath) {
    $zipSize = (Get-Item $zipPath).Length / 1MB
    Write-Host "   ✅ ZIP criado: $OutputZip ($([math]::Round($zipSize, 2)) MB)" -ForegroundColor Green
} else {
    Write-Host "   ❌ Erro ao criar ZIP!" -ForegroundColor Red
    exit 1
}

# 5. Limpar diretórios temporários
Write-Host ""
Write-Host "🧹 Limpando arquivos temporários..." -ForegroundColor Yellow
Remove-Item $tempPackageDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $packageDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ Pacote de instalação criado com sucesso!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Arquivo ZIP: $OutputZip" -ForegroundColor Cyan
Write-Host "📁 Localização: $zipPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Próximos passos:" -ForegroundColor Yellow
Write-Host "   1. Copie o arquivo ZIP para o PC do cliente" -ForegroundColor White
Write-Host "   2. Extraia o ZIP no local desejado (ex: C:\IComanda\)" -ForegroundColor White
Write-Host "   3. Execute INSTALAR.bat e siga as instruções" -ForegroundColor White
Write-Host "   4. Configure o appsettings.json com os dados do banco" -ForegroundColor White
Write-Host "   5. Execute start-api.bat ou instale como serviço" -ForegroundColor White
Write-Host ""
Write-Host "💡 Dica: Leia o arquivo LEIA-ME.txt para instruções detalhadas" -ForegroundColor Cyan
Write-Host ""

