# ========================================
# DIAGNOSTICO COMPLETO - WhatsApp Web
# ========================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  DIAGNOSTICO COMPLETO - WhatsApp Web" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$erros = @()
$sucessos = @()

# ========================================
# TESTE 1: Verificar processos do Chrome
# ========================================
Write-Host "[TESTE 1] Verificando processos do Chrome..." -ForegroundColor Yellow
$chromeProcesses = Get-Process -Name "chrome" -ErrorAction SilentlyContinue
if ($chromeProcesses) {
    $count = $chromeProcesses.Count
    Write-Host "   [OK] Chrome esta rodando ($count processo(s))" -ForegroundColor Green
    $sucessos += "Chrome esta rodando"
    
    # Verificar argumentos de linha de comando
    Write-Host "   Verificando argumentos de linha de comando..." -ForegroundColor Gray
    $temRemoteDebugging = $false
    foreach ($proc in $chromeProcesses) {
        try {
            $cmdLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($proc.Id)").CommandLine
            if ($cmdLine -like "*remote-debugging-port*") {
                Write-Host "   [OK] Processo $($proc.Id) tem remote-debugging-port" -ForegroundColor Green
                if ($cmdLine -match "remote-debugging-port=(\d+)") {
                    $porta = $matches[1]
                    Write-Host "   [INFO] Porta detectada: $porta" -ForegroundColor Cyan
                    $temRemoteDebugging = $true
                }
            }
        } catch {
            # Ignorar erros ao acessar linha de comando
        }
    }
    
    if (-not $temRemoteDebugging) {
        Write-Host "   [AVISO] Nenhum processo Chrome encontrado com remote-debugging-port" -ForegroundColor Yellow
        $erros += "Chrome nao iniciado com remote-debugging-port"
    }
} else {
    Write-Host "   [ERRO] Chrome NAO esta rodando" -ForegroundColor Red
    $erros += "Chrome nao esta rodando"
}
Write-Host ""

# ========================================
# TESTE 2: Verificar portas abertas
# ========================================
Write-Host "[TESTE 2] Verificando portas de remote debugging..." -ForegroundColor Yellow
$portas = @(9222, 9223, 9224, 9225)
$portaAberta = $null

foreach ($porta in $portas) {
    $connection = Test-NetConnection -ComputerName localhost -Port $porta -WarningAction SilentlyContinue -InformationLevel Quiet
    if ($connection) {
        Write-Host "   [OK] Porta $porta esta ABERTA" -ForegroundColor Green
        $portaAberta = $porta
        $sucessos += "Porta $porta esta aberta"
        break
    } else {
        Write-Host "   [ERRO] Porta $porta esta FECHADA" -ForegroundColor Red
    }
}

if (-not $portaAberta) {
    Write-Host "   [ERRO] NENHUMA porta de remote debugging esta aberta!" -ForegroundColor Red
    $erros += "Nenhuma porta de remote debugging aberta"
}
Write-Host ""

# ========================================
# TESTE 3: Conectar via HTTP ao Chrome DevTools
# ========================================
if ($portaAberta) {
    Write-Host "[TESTE 3] Testando conexao HTTP com Chrome DevTools (porta $portaAberta)..." -ForegroundColor Yellow
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:$portaAberta/json" -Method Get -ErrorAction Stop
        Write-Host "   [OK] Conectado ao Chrome DevTools!" -ForegroundColor Green
        $totalAbas = $response.Count
        Write-Host "   [INFO] Total de abas: $totalAbas" -ForegroundColor Cyan
        $sucessos += "Conexao HTTP com Chrome DevTools OK"
        
        # Verificar se WhatsApp Web esta aberto
        $whatsappAberto = $false
        foreach ($tab in $response) {
            if ($tab.url -like "*web.whatsapp.com*") {
                Write-Host "   [OK] WhatsApp Web encontrado!" -ForegroundColor Green
                Write-Host "      URL: $($tab.url)" -ForegroundColor Gray
                Write-Host "      Titulo: $($tab.title)" -ForegroundColor Gray
                $whatsappAberto = $true
                $sucessos += "WhatsApp Web esta aberto"
                break
            }
        }
        
        if (-not $whatsappAberto) {
            Write-Host "   [AVISO] WhatsApp Web NAO esta aberto" -ForegroundColor Yellow
            $erros += "WhatsApp Web nao esta aberto"
        }
        
        # Listar todas as abas
        Write-Host "   [INFO] Abas abertas:" -ForegroundColor Cyan
        foreach ($tab in $response) {
            Write-Host "      - $($tab.title) ($($tab.url))" -ForegroundColor Gray
        }
    } catch {
        Write-Host "   [ERRO] Erro ao conectar: $_" -ForegroundColor Red
        $erros += "Erro ao conectar via HTTP: $_"
    }
} else {
    Write-Host "[TESTE 3] Pulado - nenhuma porta aberta" -ForegroundColor Gray
}
Write-Host ""

# ========================================
# TESTE 4: Verificar backend
# ========================================
Write-Host "[TESTE 4] Verificando backend..." -ForegroundColor Yellow
try {
    $statusResponse = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/status" -Method Get -ErrorAction Stop
    if ($statusResponse.conectado) {
        Write-Host "   [OK] Backend retorna: CONECTADO" -ForegroundColor Green
        $sucessos += "Backend conectado"
    } else {
        Write-Host "   [ERRO] Backend retorna: NAO CONECTADO" -ForegroundColor Red
        $erros += "Backend nao conectado"
    }
} catch {
    Write-Host "   [ERRO] Backend nao esta respondendo: $_" -ForegroundColor Red
    $erros += "Backend nao esta rodando ou nao responde"
}
Write-Host ""

# ========================================
# TESTE 5: Tentar inicializar via API
# ========================================
Write-Host "[TESTE 5] Tentando inicializar via API..." -ForegroundColor Yellow
try {
    $initResponse = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/inicializar" -Method Post -ErrorAction Stop
    Write-Host "   [OK] Inicializacao: $($initResponse.mensagem)" -ForegroundColor Green
    Start-Sleep -Seconds 5
    
    # Verificar status novamente
    $statusResponse = Invoke-RestMethod -Uri "http://localhost:65375/api/whatsapp/status" -Method Get -ErrorAction Stop
    if ($statusResponse.conectado) {
        Write-Host "   [OK] Apos inicializacao: CONECTADO!" -ForegroundColor Green
        $sucessos += "Inicializacao bem-sucedida"
    } else {
        Write-Host "   [ERRO] Apos inicializacao: Ainda NAO CONECTADO" -ForegroundColor Red
        $erros += "Inicializacao nao conectou"
    }
} catch {
    Write-Host "   [ERRO] Erro ao inicializar: $_" -ForegroundColor Red
    $erros += "Erro ao inicializar: $_"
}
Write-Host ""

# ========================================
# RESUMO FINAL
# ========================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RESUMO DO DIAGNOSTICO" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($sucessos.Count -gt 0) {
    Write-Host "[SUCESSOS] ($($sucessos.Count)):" -ForegroundColor Green
    foreach ($s in $sucessos) {
        Write-Host "   - $s" -ForegroundColor Green
    }
    Write-Host ""
}

if ($erros.Count -gt 0) {
    Write-Host "[PROBLEMAS ENCONTRADOS] ($($erros.Count)):" -ForegroundColor Red
    foreach ($e in $erros) {
        Write-Host "   - $e" -ForegroundColor Red
    }
    Write-Host ""
    
    Write-Host "[SOLUCOES RECOMENDADAS]:" -ForegroundColor Yellow
    Write-Host ""
    
    $temErroChrome = $false
    foreach ($erro in $erros) {
        if ($erro -like "*Chrome nao esta rodando*" -or $erro -like "*remote-debugging-port*" -or $erro -like "*Nenhuma porta*") {
            $temErroChrome = $true
            break
        }
    }
    
    if ($temErroChrome) {
        Write-Host "1. Feche TODAS as janelas do Chrome:" -ForegroundColor White
        Write-Host "   taskkill /F /IM chrome.exe" -ForegroundColor Gray
        Write-Host ""
        Write-Host "2. Execute o script de inicializacao:" -ForegroundColor White
        Write-Host "   .\iniciar-chrome-whatsapp.bat" -ForegroundColor Gray
        Write-Host ""
    }
    
    $temErroWhatsApp = $false
    foreach ($erro in $erros) {
        if ($erro -like "*WhatsApp Web nao esta aberto*") {
            $temErroWhatsApp = $true
            break
        }
    }
    
    if ($temErroWhatsApp) {
        Write-Host "3. Abra o WhatsApp Web no Chrome:" -ForegroundColor White
        Write-Host "   https://web.whatsapp.com" -ForegroundColor Gray
        Write-Host ""
        Write-Host "4. Faca o login (escaneie o QR Code)" -ForegroundColor White
        Write-Host ""
    }
    
    $temErroBackend = $false
    foreach ($erro in $erros) {
        if ($erro -like "*Backend nao*") {
            $temErroBackend = $true
            break
        }
    }
    
    if ($temErroBackend) {
        Write-Host "5. Inicie o backend:" -ForegroundColor White
        Write-Host "   cd IComanda.API" -ForegroundColor Gray
        Write-Host "   dotnet run" -ForegroundColor Gray
        Write-Host ""
    }
    
    Write-Host "6. Execute este diagnostico novamente:" -ForegroundColor White
    Write-Host "   .\DIAGNOSTICO_COMPLETO.ps1" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "[TUDO OK] O sistema deve estar funcionando!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Teste enviar uma mensagem agora!" -ForegroundColor Cyan
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
