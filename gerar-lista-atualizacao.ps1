# ═══════════════════════════════════════════════════════════════════════
#  GERAR LISTA DE ARQUIVOS MODIFICADOS PARA ATUALIZAÇÃO DO CLIENTE
# ═══════════════════════════════════════════════════════════════════════
#  Uso: .\gerar-lista-atualizacao.ps1 [-DiretorioAnterior "C:\backup\versao_anterior"]
# ═══════════════════════════════════════════════════════════════════════

param(
    [Parameter(Mandatory=$false)]
    [string]$DiretorioAnterior = "",
    
    [Parameter(Mandatory=$false)]
    [string]$NomeAtualizacao = "Atualização",
    
    [Parameter(Mandatory=$false)]
    [switch]$CompararComGit = $false
)

$ErrorActionPreference = "Stop"

# ═══════════════════════════════════════════════════════════════════════
#  CONFIGURAÇÕES
# ═══════════════════════════════════════════════════════════════════════

$raizProjeto = "C:\VS_Code\icomanda"
$dataAtual = Get-Date -Format "dd/MMMM/yyyy"
$arquivoSaida = Join-Path $raizProjeto "ATUALIZACAO_$(Get-Date -Format 'yyyyMMdd_HHmmss').md"

# Pastas a ignorar
$pastasIgnorar = @(
    "node_modules",
    "bin",
    "obj",
    ".vs",
    ".git",
    "build",
    "publish",
    "backup",
    "legado"
)

# Extensões de arquivos importantes
$extensoesImportantes = @(
    "*.cs",
    "*.csproj",
    "*.tsx",
    "*.ts",
    "*.json",
    "*.js",
    "*.jsx",
    "*.sql",
    "*.ps1",
    "*.bat",
    "*.md",
    "*.config",
    "*.txt",
    "*.env*"
)

# ═══════════════════════════════════════════════════════════════════════
#  FUNÇÕES
# ═══════════════════════════════════════════════════════════════════════

function Write-ColorText {
    param($Text, $Color = "White")
    Write-Host $Text -ForegroundColor $Color
}

function Get-ArquivosModificados {
    if ($CompararComGit) {
        # Usar Git para ver arquivos modificados
        Write-ColorText "🔍 Buscando arquivos modificados no Git..." -Color Cyan
        
        $modificados = git diff --name-only HEAD~1 HEAD
        $novos = git diff --name-only --diff-filter=A HEAD~1 HEAD
        $deletados = git diff --name-only --diff-filter=D HEAD~1 HEAD
        
        return @{
            Modificados = $modificados
            Novos = $novos
            Deletados = $deletados
        }
    }
    elseif ($DiretorioAnterior -and (Test-Path $DiretorioAnterior)) {
        # Comparar com diretório anterior
        Write-ColorText "🔍 Comparando com versão anterior em: $DiretorioAnterior" -Color Cyan
        
        $arquivosAtuais = Get-ChildItem -Path $raizProjeto -Recurse -File | 
            Where-Object { 
                $caminho = $_.FullName
                $ignorar = $false
                foreach ($pasta in $pastasIgnorar) {
                    if ($caminho -like "*\$pasta\*") { $ignorar = $true; break }
                }
                -not $ignorar
            }
        
        $modificados = @()
        $novos = @()
        
        foreach ($arquivo in $arquivosAtuais) {
            $caminhoRelativo = $arquivo.FullName.Replace($raizProjeto, "").TrimStart("\")
            $arquivoAnterior = Join-Path $DiretorioAnterior $caminhoRelativo
            
            if (Test-Path $arquivoAnterior) {
                # Arquivo existe - verificar se foi modificado
                $hashAtual = (Get-FileHash $arquivo.FullName -Algorithm MD5).Hash
                $hashAnterior = (Get-FileHash $arquivoAnterior -Algorithm MD5).Hash
                
                if ($hashAtual -ne $hashAnterior) {
                    $modificados += $caminhoRelativo
                }
            }
            else {
                # Arquivo novo
                $novos += $caminhoRelativo
            }
        }
        
        return @{
            Modificados = $modificados
            Novos = $novos
            Deletados = @()
        }
    }
    else {
        # Listar arquivos modificados recentemente (últimas 24h)
        Write-ColorText "🔍 Listando arquivos modificados nas últimas 24 horas..." -Color Cyan
        
        $dataLimite = (Get-Date).AddHours(-24)
        
        $arquivos = Get-ChildItem -Path $raizProjeto -Recurse -File | 
            Where-Object { 
                $caminho = $_.FullName
                $ignorar = $false
                foreach ($pasta in $pastasIgnorar) {
                    if ($caminho -like "*\$pasta\*") { $ignorar = $true; break }
                }
                -not $ignorar -and $_.LastWriteTime -gt $dataLimite
            } |
            Select-Object -ExpandProperty FullName
        
        $arquivosRelativos = $arquivos | ForEach-Object { $_.Replace($raizProjeto, "").TrimStart("\") }
        
        return @{
            Modificados = $arquivosRelativos
            Novos = @()
            Deletados = @()
        }
    }
}

function Categorizar-Arquivos {
    param($Arquivos)
    
    $backend = @()
    $frontend = @()
    $raiz = @()
    $scripts = @()
    $docs = @()
    
    foreach ($arquivo in $Arquivos) {
        if ($arquivo -like "IComanda.API\*") {
            if ($arquivo -like "*.sql" -or $arquivo -like "*.ps1" -or $arquivo -like "*.bat") {
                $scripts += $arquivo
            } else {
                $backend += $arquivo
            }
        }
        elseif ($arquivo -like "icomanda-frontend\*") {
            $frontend += $arquivo
        }
        elseif ($arquivo -like "*.md" -or $arquivo -like "*.txt") {
            $docs += $arquivo
        }
        else {
            $raiz += $arquivo
        }
    }
    
    return @{
        Backend = $backend
        Frontend = $frontend
        Raiz = $raiz
        Scripts = $scripts
        Docs = $docs
    }
}

function Gerar-Markdown {
    param($Dados)
    
    $md = @"
# 📋 $NomeAtualizacao

---

## 🎯 RESUMO DA ATUALIZAÇÃO

**Data:** $dataAtual  
**Tipo:** [Correção | Nova Feature | Atualização | Otimização]  
**Descrição:** [Descreva brevemente o que foi implementado/corrigido]

---

## 📂 ARQUIVOS PARA COPIAR NO CLIENTE

"@

    # Backend
    if ($Dados.Backend.Count -gt 0) {
        $md += @"


### ✅ Backend - Copiar para ``C:\iComanda\IComanda.API\``

``````powershell
`$origem = "D:\atualizacao"  # Ajuste conforme necessário

"@
        foreach ($arquivo in $Dados.Backend) {
            $destino = $arquivo.Replace("IComanda.API\", "")
            $md += "Copy-Item `"`$origem\$arquivo`" -Destination `"C:\iComanda\IComanda.API\$destino`" -Force`n"
        }
        $md += "``````"
        
        $md += "`n`n**Arquivos:**`n``````"
        foreach ($arquivo in $Dados.Backend) {
            $simbolo = if ($Dados.Novos -contains $arquivo) { "➕ NOVO" } else { "✏️  MODIFICADO" }
            $md += "`n$simbolo`: $arquivo"
        }
        $md += "`n``````"
    }

    # Frontend
    if ($Dados.Frontend.Count -gt 0) {
        $md += @"


### ✅ Frontend - Copiar para ``C:\iComanda\icomanda-frontend\``

``````powershell
"@
        foreach ($arquivo in $Dados.Frontend) {
            $destino = $arquivo.Replace("icomanda-frontend\", "")
            $md += "Copy-Item `"`$origem\$arquivo`" -Destination `"C:\iComanda\icomanda-frontend\$destino`" -Force`n"
        }
        $md += "``````"
        
        $md += "`n`n**Arquivos:**`n``````"
        foreach ($arquivo in $Dados.Frontend) {
            $simbolo = if ($Dados.Novos -contains $arquivo) { "➕ NOVO" } else { "✏️  MODIFICADO" }
            $md += "`n$simbolo`: $arquivo"
        }
        $md += "`n``````"
    }

    # Scripts
    if ($Dados.Scripts.Count -gt 0) {
        $md += @"


### ✅ Scripts - Copiar para ``C:\iComanda\IComanda.API\``

``````powershell
"@
        foreach ($arquivo in $Dados.Scripts) {
            $md += "Copy-Item `"`$origem\$arquivo`" -Destination `"C:\iComanda\IComanda.API\`" -Force`n"
        }
        $md += "``````"
        
        $md += "`n`n**Arquivos:**`n``````"
        foreach ($arquivo in $Dados.Scripts) {
            $simbolo = if ($Dados.Novos -contains $arquivo) { "➕ NOVO" } else { "✏️  MODIFICADO" }
            $md += "`n$simbolo`: $arquivo"
        }
        $md += "`n``````"
    }

    # Documentação
    if ($Dados.Docs.Count -gt 0) {
        $md += @"


### ℹ️ Documentação - Copiar para ``C:\iComanda\``

``````powershell
"@
        foreach ($arquivo in $Dados.Docs) {
            $md += "Copy-Item `"`$origem\$arquivo`" -Destination `"C:\iComanda\`" -Force`n"
        }
        $md += "``````"
        
        $md += "`n`n**Arquivos:**`n``````"
        foreach ($arquivo in $Dados.Docs) {
            $simbolo = if ($Dados.Novos -contains $arquivo) { "➕ NOVO" } else { "✏️  MODIFICADO" }
            $md += "`n$simbolo`: $arquivo"
        }
        $md += "`n``````"
    }

    # Instruções
    $md += @"


---

## 🔄 PASSOS PARA ATUALIZAR

### 1️⃣ Parar Aplicação
``````powershell
cd C:\iComanda
.\parar-tudo.bat
``````

### 2️⃣ Fazer Backup
``````powershell
`$data = Get-Date -Format "yyyyMMdd_HHmmss"
New-Item -Path "C:\iComanda\backup\`$data" -ItemType Directory
Copy-Item "C:\iComanda\IComanda.API\*" -Destination "C:\iComanda\backup\`$data\" -Recurse
``````

### 3️⃣ Copiar Arquivos Atualizados
Execute os comandos PowerShell acima (seções Backend/Frontend/Scripts)

### 4️⃣ Restaurar Dependências (se necessário)
``````powershell
# Backend
cd C:\iComanda\IComanda.API
dotnet restore

# Frontend (se package.json mudou)
cd C:\iComanda\icomanda-frontend
npm install
``````

### 5️⃣ Executar Scripts/Migrações (se houver)
``````powershell
# Exemplo: Se tiver novo SQL
cd C:\iComanda\IComanda.API
.\executar-sql-[nome].ps1
``````

### 6️⃣ Recompilar
``````powershell
cd C:\iComanda\IComanda.API
dotnet build -c Release
``````

### 7️⃣ Reiniciar Aplicação
``````powershell
cd C:\iComanda
.\iniciar-tudo.bat
``````

---

## ✅ CHECKLIST PÓS-ATUALIZAÇÃO

- [ ] Backend inicia sem erros
- [ ] Frontend inicia sem erros
- [ ] Login funciona
- [ ] Funcionalidades testadas
- [ ] Sem erros nos logs

---

## 📊 RESUMO

**Total de arquivos:** $($Dados.Backend.Count + $Dados.Frontend.Count + $Dados.Scripts.Count + $Dados.Docs.Count + $Dados.Raiz.Count)  
- Backend: $($Dados.Backend.Count)  
- Frontend: $($Dados.Frontend.Count)  
- Scripts: $($Dados.Scripts.Count)  
- Documentação: $($Dados.Docs.Count)  
- Outros: $($Dados.Raiz.Count)

---

*Gerado automaticamente em $(Get-Date -Format "dd/MM/yyyy HH:mm:ss")*  
*iComanda - Sistema de Gestão de Comandas*
"@

    return $md
}

# ═══════════════════════════════════════════════════════════════════════
#  EXECUÇÃO PRINCIPAL
# ═══════════════════════════════════════════════════════════════════════

Write-ColorText "`n═══════════════════════════════════════════════════════════════════════" -Color Cyan
Write-ColorText "  GERADOR DE LISTA DE ATUALIZAÇÃO - iComanda" -Color Cyan
Write-ColorText "═══════════════════════════════════════════════════════════════════════`n" -Color Cyan

try {
    # Buscar arquivos modificados
    $resultado = Get-ArquivosModificados
    
    $todosArquivos = @()
    $todosArquivos += $resultado.Modificados
    $todosArquivos += $resultado.Novos
    
    if ($todosArquivos.Count -eq 0) {
        Write-ColorText "⚠️  Nenhum arquivo modificado encontrado!" -Color Yellow
        Write-ColorText "`nDicas:" -Color Yellow
        Write-ColorText "  - Use -DiretorioAnterior para comparar com versão anterior" -Color Gray
        Write-ColorText "  - Use -CompararComGit para usar Git" -Color Gray
        exit 0
    }
    
    Write-ColorText "✅ Encontrados $($todosArquivos.Count) arquivos modificados/novos`n" -Color Green
    
    # Categorizar arquivos
    $categorizado = Categorizar-Arquivos -Arquivos $todosArquivos
    
    Write-ColorText "📊 Distribuição:" -Color Cyan
    Write-ColorText "   Backend: $($categorizado.Backend.Count)" -Color White
    Write-ColorText "   Frontend: $($categorizado.Frontend.Count)" -Color White
    Write-ColorText "   Scripts: $($categorizado.Scripts.Count)" -Color White
    Write-ColorText "   Docs: $($categorizado.Docs.Count)" -Color White
    Write-ColorText "   Outros: $($categorizado.Raiz.Count)`n" -Color White
    
    # Adicionar informação de novos
    $categorizado.Novos = $resultado.Novos
    
    # Gerar markdown
    $markdown = Gerar-Markdown -Dados $categorizado
    
    # Salvar arquivo
    $markdown | Out-File -FilePath $arquivoSaida -Encoding UTF8
    
    Write-ColorText "✅ Lista de atualização gerada com sucesso!" -Color Green
    Write-ColorText "`n📄 Arquivo: $arquivoSaida`n" -Color Cyan
    
    # Abrir arquivo
    $resposta = Read-Host "Deseja abrir o arquivo agora? (S/N)"
    if ($resposta -eq "S" -or $resposta -eq "s") {
        Start-Process notepad $arquivoSaida
    }
}
catch {
    Write-ColorText "`n❌ Erro ao gerar lista: $($_.Exception.Message)" -Color Red
    exit 1
}

Write-ColorText "═══════════════════════════════════════════════════════════════════════`n" -Color Cyan
