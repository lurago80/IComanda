# Script para iniciar o Frontend IComanda
# Verifica dependências, instala se necessário, fecha processos em execução e inicia o frontend

param(
    [switch]$Force = $false
)

# Configurar encoding para suportar caracteres especiais
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# Cores para output
function Write-Step {
    param([string]$Message, [int]$Step, [int]$Total)
    Write-Host "`n[$Step/$Total] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Blue
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "   Iniciando Frontend IComanda" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Obter diretório do script
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

# Validar pasta correta de execução
$AllowedRoots = @("C:\VS_Code\icomanda\IComanda.API\icomanda-frontend", "C:\iCOMANDA\IComanda.API\icomanda-frontend")
$CurrentRoot = [System.IO.Path]::GetFullPath($ScriptDir).TrimEnd('\\')

if (-not ($AllowedRoots | Where-Object { $CurrentRoot.Equals($_, [System.StringComparison]::OrdinalIgnoreCase) })) {
    Write-Error "Script executado na pasta errada."
    Write-Error "Pasta detectada: $CurrentRoot"
    Write-Warning "Pastas corretas: $($AllowedRoots -join '  OU  ')"
    Write-Warning "Ação: execute este script somente na pasta oficial do projeto."
    exit 1
}

Write-Info "Diretório do frontend: $ScriptDir"

# ========================================
# VERIFICAR NODE.JS E NPM
# ========================================
Write-Step "Verificando Node.js e npm" 1 5

# Verificar Node.js
try {
    $nodeVersion = node --version 2>&1
    if ($LASTEXITCODE -eq 0 -and $nodeVersion -notmatch "error|não encontrado") {
        Write-Success "Node.js encontrado: $nodeVersion"
    } else {
        throw "Node.js não encontrado"
    }
} catch {
    Write-Error "Node.js não está instalado!"
    Write-Info "Por favor, instale o Node.js: https://nodejs.org/"
    Write-Info "Versão recomendada: Node.js 16 ou superior"
    exit 1
}

# Verificar npm
try {
    $npmVersion = npm --version 2>&1
    if ($LASTEXITCODE -eq 0 -and $npmVersion -notmatch "error|não encontrado") {
        Write-Success "npm encontrado: $npmVersion"
    } else {
        throw "npm não encontrado"
    }
} catch {
    Write-Error "npm não está instalado!"
    Write-Info "O npm geralmente vem com o Node.js. Verifique sua instalação."
    exit 1
}

# ========================================
# VERIFICAR E INSTALAR DEPENDÊNCIAS
# ========================================
Write-Step "Verificando dependências do projeto" 2 5

if (-not (Test-Path "package.json")) {
    Write-Error "Arquivo package.json não encontrado!"
    Write-Info "Certifique-se de estar na pasta correta do frontend."
    exit 1
}

if (-not (Test-Path "node_modules") -or $Force) {
    if ($Force) {
        Write-Warning "Forçando reinstalação das dependências..."
        if (Test-Path "node_modules") {
            Remove-Item "node_modules" -Recurse -Force -ErrorAction SilentlyContinue
        }
        if (Test-Path "package-lock.json") {
            Remove-Item "package-lock.json" -Force -ErrorAction SilentlyContinue
        }
    }
    
    Write-Info "Instalando dependências do projeto..."
    Write-Info "Isso pode levar alguns minutos na primeira vez..."
    
    npm install
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha ao instalar dependências!"
        Write-Info "Tente executar manualmente: npm install"
        exit 1
    }
    
    Write-Success "Dependências instaladas com sucesso!"
} else {
    Write-Success "Dependências já instaladas (node_modules encontrado)"
}

# ========================================
# VERIFICAR E FECHAR PROCESSOS EM EXECUÇÃO
# ========================================
Write-Step "Verificando processos em execução" 3 5

$FrontendPort = 3000
$processesKilled = $false

# Verificar processos Node.js na porta 3000
try {
    $frontendConn = Get-NetTCPConnection -LocalPort $FrontendPort -State Listen -ErrorAction SilentlyContinue
    if ($frontendConn) {
        foreach ($conn in $frontendConn) {
            $processId = $conn.OwningProcess
            if ($processId) {
                try {
                    $proc = Get-Process -Id $processId -ErrorAction Stop
                    if ($proc.ProcessName -eq "node") {
                        Write-Warning "Processo Node.js encontrado na porta $FrontendPort (PID: $processId)"
                        Write-Info "Encerrando processo..."
                        Stop-Process -Id $processId -Force -ErrorAction Stop
                        $processesKilled = $true
                        Write-Success "Processo encerrado com sucesso"
                    }
                } catch {
                    Write-Warning "Não foi possível encerrar o processo PID $processId na porta $FrontendPort"
                    Write-Info "Você pode precisar fechar manualmente ou executar como administrador"
                }
            }
        }
    }
} catch {
    # Ignorar erros de permissão
    Write-Info "Não foi possível verificar a porta $FrontendPort (pode precisar de permissões de administrador)"
}

# Verificar processos node.exe relacionados ao React
try {
    $nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
            if ($cmdLine -and ($cmdLine -match "react-scripts" -or $cmdLine -match "webpack" -or $cmdLine -match "start")) {
                return $true
            }
        } catch {
            # Ignorar se não conseguir obter command line
        }
        return $false
    }
    
    if ($nodeProcesses) {
        foreach ($proc in $nodeProcesses) {
            Write-Warning "Processo Node.js relacionado ao React encontrado (PID: $($proc.Id))"
            Write-Info "Encerrando processo..."
            try {
                Stop-Process -Id $proc.Id -Force -ErrorAction Stop
                $processesKilled = $true
                Write-Success "Processo encerrado com sucesso"
            } catch {
                Write-Warning "Não foi possível encerrar o processo PID $($proc.Id)"
            }
        }
    }
} catch {
    # Ignorar erros
}

if ($processesKilled) {
    Write-Info "Aguardando processos encerrarem completamente..."
    Start-Sleep -Seconds 2
}

Write-Success "Verificação de processos concluída"

# ========================================
# VERIFICAR SE A PORTA ESTÁ DISPONÍVEL
# ========================================
Write-Step "Verificando disponibilidade da porta" 4 5

try {
    $portInUse = Get-NetTCPConnection -LocalPort $FrontendPort -State Listen -ErrorAction SilentlyContinue
    if ($portInUse) {
        Write-Warning "A porta $FrontendPort ainda está em uso!"
        Write-Info "Tentando aguardar mais um pouco..."
        Start-Sleep -Seconds 3
        
        $portInUse = Get-NetTCPConnection -LocalPort $FrontendPort -State Listen -ErrorAction SilentlyContinue
        if ($portInUse) {
            Write-Error "A porta $FrontendPort ainda está em uso após tentativas de liberação."
            Write-Info "Por favor, feche manualmente o processo que está usando a porta $FrontendPort"
            Write-Info "Ou altere a porta no arquivo .env (REACT_APP_PORT)"
            exit 1
        }
    }
    Write-Success "Porta $FrontendPort disponível"
} catch {
    Write-Info "Não foi possível verificar a porta (continuando mesmo assim...)"
}

# ========================================
# INICIAR O FRONTEND
# ========================================
Write-Step "Iniciando o Frontend" 5 5

Write-Info "Iniciando servidor de desenvolvimento React..."
Write-Info "O frontend será aberto em: http://localhost:$FrontendPort"
Write-Info "Pressione Ctrl+C para parar o servidor`n"

# Iniciar o frontend
try {
    $env:DISABLE_ESLINT_PLUGIN = "true"
    npm start
} catch {
    Write-Error "Erro ao iniciar o frontend!"
    Write-Info "Verifique se todas as dependências foram instaladas corretamente."
    Write-Info "Tente executar: npm install"
    exit 1
}

