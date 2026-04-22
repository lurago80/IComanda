# SISTEMA DE GERENCIAMENTO DE PORTAS - ICOMANDA

## ✅ IMPLEMENTADO COM SUCESSO

Sistema completo de verificação e liberação automática de portas antes de iniciar o iComanda.

---

## 📦 ARQUIVOS CRIADOS

### Scripts de Inicialização:
- ✅ `iniciar-sistema.bat` - Iniciar sistema (Windows Batch)
- ✅ `iniciar-sistema.ps1` - Iniciar sistema (PowerShell)

### Scripts de Parada:
- ✅ `parar-sistema.bat` - Parar todos os serviços (Windows Batch)
- ✅ `parar-sistema.ps1` - Parar todos os serviços (PowerShell)

### Scripts de Diagnóstico:
- ✅ `diagnostico-portas.bat` - Verificar status de portas (Windows Batch)
- ✅ `diagnostico-portas.ps1` - Verificar status de portas (PowerShell)

### Documentação:
- ✅ `PORTAS_SISTEMA.md` - Lista completa de portas e troubleshooting
- ✅ `SCRIPTS_GERENCIAMENTO.md` - Guia de uso dos scripts
- ✅ `SOLUCAO_ERRO_DLL_BLOQUEADA.md` - Atualizado com info de portas

---

## 🔌 PORTAS GERENCIADAS

| Porta | Serviço | Ação |
|-------|---------|------|
| **5000** | Backend API | Libera se ocupada |
| **3000** | Frontend React | Libera se ocupada |
| **3001** | WhatsApp Baileys | Libera se ocupada |
| **3050** | Firebird Database | **NÃO MEXE** (crítico) |

---

## 🚀 COMO USAR

### Iniciar o Sistema:
```batch
cd C:\VS_Code\icomanda\IComanda.API
iniciar-sistema.bat
```

O script automaticamente:
1. ✅ Verifica portas 5000, 3000, 3001
2. ✅ Encerra processos que estejam ocupando essas portas
3. ✅ Preserva porta 3050 (Firebird - não mexe!)
4. ✅ Limpa arquivos de build bloqueados (bin/obj)
5. ✅ Compila backend em Release
6. ✅ Inicia backend em processo isolado
7. ✅ Verifica saúde do backend (http://localhost:5000/health)

### Parar o Sistema:
```batch
cd C:\VS_Code\icomanda\IComanda.API
parar-sistema.bat
```

### Diagnosticar Problemas:
```batch
cd C:\VS_Code\icomanda\IComanda.API
diagnostico-portas.bat
```

---

## 🎯 CARACTERÍSTICAS PRINCIPAIS

### ✅ Verificação Inteligente de Portas
- Detecta automaticamente quais portas estão ocupadas
- Identifica o processo que está usando cada porta
- Mostra PID, nome do processo e uso de memória

### ✅ Liberação Automática
- Encerra processos dotnet/node que bloqueiam portas
- **NUNCA** encerra o Firebird (porta 3050)
- Aguarda 3 segundos após liberar portas

### ✅ Proteção de Dados
- Porta 3050 (Firebird) é protegida
- Script exibe aviso caso tente acessar porta do banco

### ✅ Limpeza de Build
- Remove pastas bin/ e obj/ antes de compilar
- Evita erro MSB3027 (DLL bloqueada)
- Usa flag --no-incremental para build limpo

### ✅ Health Check Automático
- Verifica se backend respondeu após 5 segundos
- Testa endpoint http://localhost:5000/health
- Exibe status e conteúdo da resposta

---

## 📊 EXEMPLO DE SAÍDA

### iniciar-sistema.bat
```
========================================
  INICIANDO SISTEMA ICOMANDA
========================================

[1/6] Verificando portas em uso...

  Verificando porta 5000 (Backend API)... LIVRE
  Verificando porta 3000 (Frontend React)... LIVRE
  Verificando porta 3001 (WhatsApp Baileys)... LIVRE
  Verificando porta 3050 (Firebird Database)... OCUPADA (PID: 5404)
    [FIREBIRD] Porta do banco de dados - NAO sera encerrado

[2/6] Encerrando processos dotnet e node...
  Nenhum processo dotnet encontrado
  Nenhum processo node encontrado

[3/6] Limpando arquivos de build...
  Pasta bin removida
  Pasta obj removida

[4/6] Compilando backend...
  Build concluido com sucesso

[5/6] Iniciando backend...
  Backend iniciado (PID: 29352)

[6/6] Verificando saude do backend...
  Backend ONLINE e respondendo
  Status: 200
  Resposta: Healthy

========================================
  SISTEMA ICOMANDA INICIADO
========================================

URLs do Sistema:
  Backend API: http://localhost:5000
  Swagger: http://localhost:5000/swagger
  Health Check: http://localhost:5000/health
```

### diagnostico-portas.bat
```
========================================
  DIAGNOSTICO DE PORTAS - ICOMANDA
========================================

Porta 3000 - Frontend React
============================================================
  Status: LIVRE

Porta 3001 - WhatsApp Baileys
============================================================
  Status: LIVRE

Porta 3050 - Firebird Database
============================================================
  Status: OCUPADA
  PID: 5404
  Processo: fbserver
  Memoria: 9.73 MB
  [BANCO DE DADOS] NAO encerre este processo manualmente!

Porta 5000 - Backend API
============================================================
  Status: OCUPADA
  PID: 29352
  Processo: dotnet
  Memoria: 45.32 MB
  Tipo: Backend .NET
  Comando para encerrar: Stop-Process -Id 29352 -Force

========================================
  RESUMO
========================================

Total de portas verificadas: 4
Portas livres: 2
Portas ocupadas: 2
```

---

## 🛠️ TROUBLESHOOTING

### Problema: "Porta 5000 já em uso"
**Solução:**
```batch
parar-sistema.bat
iniciar-sistema.bat
```

### Problema: "MSB3027: Arquivo bloqueado"
**Solução:**
```batch
parar-sistema.bat
```
Aguarde 5 segundos, depois:
```batch
iniciar-sistema.bat
```

### Problema: Backend não responde
**Ações:**
1. Aguarde 10 segundos (inicialização lenta)
2. Verifique:
   ```batch
   diagnostico-portas.bat
   ```
3. Verifique Firebird:
   ```powershell
   Get-Service | Where-Object {$_.Name -like "*Firebird*"}
   ```

---

## 📝 COMANDOS ÚTEIS

### Ver processos rodando:
```powershell
Get-Process -Name "dotnet","node"
```

### Ver portas em uso:
```powershell
netstat -ano | findstr ":5000 :3000 :3001"
```

### Encerrar processo específico:
```powershell
Stop-Process -Id <PID> -Force
```

### Verificar health do backend:
```powershell
Invoke-WebRequest http://localhost:5000/health
```

---

## ✅ BENEFÍCIOS

1. **Elimina conflitos de porta** - Sistema verifica e libera portas automaticamente
2. **Previne DLL bloqueada** - Limpeza de bin/obj antes de compilar
3. **Protege dados** - Nunca encerra Firebird
4. **Diagnóstico rápido** - Script mostra status completo em segundos
5. **Inicialização confiável** - Build limpo + health check
6. **Fácil de usar** - Apenas executar .bat

---

**Status:** ✅ IMPLEMENTADO E TESTADO  
**Desenvolvido por:** GitHub Copilot  
**Data:** 16/02/2026
