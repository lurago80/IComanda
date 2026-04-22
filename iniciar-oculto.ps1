# IComanda - Inicializacao oculta (sem janelas abertas)
# Executado pelo iniciar-oculto.vbs
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$LogFile = Join-Path $PSScriptRoot "icomanda-startup.log"

function Write-Log {
    param([string]$Msg, [string]$Level = "INF")
    $linha = "[$(Get-Date -Format 'HH:mm:ss')] [$Level] $Msg"
    Add-Content -Path $LogFile -Value $linha -Encoding UTF8
}

function Show-Balloon {
    param([string]$Titulo, [string]$Texto, [string]$Icone = "Info")
    try {
        Add-Type -AssemblyName System.Windows.Forms -ErrorAction Stop
        $balloon = New-Object System.Windows.Forms.NotifyIcon
        $balloon.Icon = [System.Drawing.SystemIcons]::Application
        $balloon.BalloonTipIcon  = $Icone
        $balloon.BalloonTipTitle = $Titulo
        $balloon.BalloonTipText  = $Texto
        $balloon.Visible = $true
        $balloon.ShowBalloonTip(6000)
        Start-Sleep -Seconds 1
        $balloon.Dispose()
    } catch {
        Write-Log "Nao foi possivel exibir notificacao: $_" "WRN"
    }
}

# --- Configuracoes ---
$ScriptDir   = $PSScriptRoot
$ApiDir      = Join-Path $ScriptDir "IComanda.API"
$FrontendDir = Join-Path $ApiDir "icomanda-frontend"

# Ler porta do appsettings.json
$BackendPort = 65375
try {
    $cfg = Get-Content (Join-Path $ApiDir "appsettings.json") -Raw | ConvertFrom-Json
    if ($cfg.Kestrel.Port) { $BackendPort = $cfg.Kestrel.Port }
} catch { }

# ------------------------------------------------------------------
Write-Log "====== INICIANDO ICOMANDA ======"
Write-Log "Pasta raiz : $ScriptDir"
Write-Log "Pasta API  : $ApiDir"
Write-Log "Porta API  : $BackendPort"

# --- Passo 1: Parar processos anteriores ---
Write-Log "Parando processos anteriores..."
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "node"   -ErrorAction SilentlyContinue | Where-Object {
    (Get-NetTCPConnection -OwningProcess $_.Id -ErrorAction SilentlyContinue |
     Where-Object { $_.LocalPort -eq 3000 })
} | Stop-Process -Force -ErrorAction SilentlyContinue

# Liberar portas residuais pelo PID dono
Get-NetTCPConnection -LocalPort $BackendPort,3000,3001 -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }

# Aguardar porta do backend ficar realmente livre (max 15s)
Write-Log "Aguardando porta $BackendPort ficar livre..."
$_waited = 0
while ($_waited -lt 15) {
    if (-not (Get-NetTCPConnection -LocalPort $BackendPort -ErrorAction SilentlyContinue)) { break }
    Start-Sleep -Seconds 1
    $_waited++
}
if ($_waited -ge 15) {
    Write-Log "AVISO: Porta $BackendPort ainda ocupada apos 15s. Tentando continuar." "WRN"
}
Write-Log "Processos parados."

# --- Passo 2: Compilar backend (silencioso) ---
Write-Log "Compilando backend..."
$buildOut = & dotnet build "$ApiDir" --configuration Release --no-incremental --verbosity quiet 2>&1
if ($LASTEXITCODE -ne 0) {
    $buildOut | ForEach-Object { Write-Log $_ "ERR" }
    Write-Log "FALHA na compilacao. Abortando." "ERR"
    Show-Balloon "iComanda - ERRO" "Falha ao compilar o backend. Verifique icomanda-startup.log." "Error"
    exit 1
}
Write-Log "Build concluido com sucesso."

# --- Passo 3: Iniciar backend oculto ---
Write-Log "Iniciando backend na porta $BackendPort (oculto)..."
$dllPath = Join-Path $ApiDir "bin\Release\net8.0\IComanda.API.dll"
Start-Process -FilePath "dotnet" `
              -ArgumentList "`"$dllPath`"" `
              -WorkingDirectory $ApiDir `
              -WindowStyle Hidden

# Aguardar backend responder
$backendOk = $false
for ($i = 1; $i -le 6; $i++) {
    Start-Sleep -Seconds 5
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:$BackendPort/health" `
                               -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        if ($r.StatusCode -eq 200) {
            $backendOk = $true
            Write-Log "Backend respondeu OK na tentativa $i."
            break
        }
    } catch {
        Write-Log "Aguardando backend... tentativa $i/6"
    }
}

if (-not $backendOk) {
    Write-Log "AVISO: Backend nao respondeu apos 30s. Pode ainda estar subindo." "WRN"
}

# --- Passo 4: Iniciar frontend oculto (npm start) ---
if (Test-Path $FrontendDir) {
    Write-Log "Iniciando frontend React na porta 3000 (oculto)..."
    Start-Process -FilePath "cmd" `
                  -ArgumentList "/c", "cd /d `"$FrontendDir`" && npm start" `
                  -WindowStyle Hidden
    Start-Sleep -Seconds 3
    Write-Log "Frontend iniciado."
} else {
    Write-Log "AVISO: Pasta do frontend nao encontrada: $FrontendDir" "WRN"
}

# --- Passo 5: Abrir navegador ---
Start-Sleep -Seconds 8
Start-Process "http://localhost:3000"

# --- Notificacao final ---
Write-Log "Sistema iniciado com sucesso!"
Write-Log "Backend : http://localhost:$BackendPort"
Write-Log "Frontend: http://localhost:3000"

if ($backendOk) {
    Show-Balloon "iComanda Iniciado" "Sistema pronto!`nFrontend: http://localhost:3000`nBackend : http://localhost:$BackendPort"
} else {
    Show-Balloon "iComanda Iniciado" "Frontend e backend iniciados (backend pode precisar de mais alguns segundos)." "Warning"
}
