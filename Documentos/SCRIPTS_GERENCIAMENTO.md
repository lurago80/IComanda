# Scripts de Gerenciamento do Sistema iComanda

Este documento descreve todos os scripts disponíveis para gerenciar o sistema iComanda.

---

## Scripts Principais

### 🚀 Iniciar Sistema

#### `iniciar-sistema.bat`
Script principal para iniciar o sistema com verificação automática de portas.

**O que faz:**
1. ✅ Verifica se as portas 5000, 3000 e 3001 estão em uso
2. ✅ Encerra processos que estejam ocupando essas portas
3. ✅ Limpa arquivos de build bloqueados
4. ✅ Compila o backend em modo Release
5. ✅ Inicia o backend
6. ✅ Verifica se o backend está respondendo

**Como usar:**
```batch
cd C:\VS_Code\icomanda\IComanda.API
iniciar-sistema.bat
```

#### `iniciar-sistema.ps1`
Versão PowerShell com mais detalhes e logs coloridos.

---

### 🛑 Parar Sistema

#### `parar-sistema.bat`
Para todos os serviços do sistema iComanda.

**O que faz:**
1. ✅ Encerra todos os processos dotnet (Backend)
2. ✅ Encerra todos os processos node (Frontend/WhatsApp)
3. ✅ Verifica se as portas foram liberadas

**Como usar:**
```batch
cd C:\VS_Code\icomanda\IComanda.API
parar-sistema.bat
```

---

### 🔍 Diagnóstico

#### `diagnostico-portas.bat`
Verifica o status detalhado de todas as portas do sistema.

**O que mostra:**
- ✅ Quais portas estão livres ou ocupadas
- ✅ PID e nome do processo usando cada porta
- ✅ Uso de memória de cada processo
- ✅ Comandos para encerrar processos específicos
- ✅ Resumo geral do sistema

**Como usar:**
```batch
cd C:\VS_Code\icomanda\IComanda.API
diagnostico-portas.bat
```

---

### 🧪 Testes

#### `testar-autenticacao-completa.ps1`
Testa o sistema completo de autenticação.

**O que testa:**
1. ✅ Login com credenciais
2. ✅ Geração de token JWT
3. ✅ Acesso a endpoint protegido
4. ✅ Validação do token

**Como usar:**
```powershell
cd C:\VS_Code\icomanda\IComanda.API
.\testar-autenticacao-completa.ps1
```

---

## Portas do Sistema

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| **Backend API** | 5000 | API principal do sistema |
| **Frontend React** | 3000 | Interface web (dev mode) |
| **WhatsApp Baileys** | 3001 | Serviço de mensagens |
| **Firebird** | 3050 | Banco de dados |

---

## Fluxo de Trabalho Recomendado

### Desenvolvimento Diário

1. **Iniciar o sistema:**
   ```batch
   iniciar-sistema.bat
   ```

2. **Testar se está funcionando:**
   ```powershell
   .\testar-autenticacao-completa.ps1
   ```

3. **Ao finalizar o trabalho:**
   ```batch
   parar-sistema.bat
   ```

### Resolução de Problemas

1. **Se o sistema não iniciar:**
   ```batch
   diagnostico-portas.bat
   ```

2. **Se aparecer erro de porta ocupada:**
   ```batch
   parar-sistema.bat
   iniciar-sistema.bat
   ```

3. **Se aparecer erro de DLL bloqueada:**
   ```batch
   parar-sistema.bat
   ```
   Aguarde 3 segundos, depois:
   ```batch
   iniciar-sistema.bat
   ```

---

## Comandos PowerShell Úteis

### Verificar processos rodando:
```powershell
Get-Process -Name "dotnet","node" -ErrorAction SilentlyContinue
```

### Ver uso de portas:
```powershell
netstat -ano | findstr ":5000 :3000 :3001"
```

### Encerrar processo específico:
```powershell
Stop-Process -Id <PID> -Force
```

### Verificar health do backend:
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/health"
```

### Ver logs do backend:
```powershell
Get-Content C:\VS_Code\icomanda\IComanda.API\logs\*.log -Tail 50 -Wait
```

---

## Documentação Adicional

- [PORTAS_SISTEMA.md](PORTAS_SISTEMA.md) - Detalhes sobre portas e conflitos
- [SOLUCAO_ERRO_DLL_BLOQUEADA.md](SOLUCAO_ERRO_DLL_BLOQUEADA.md) - Resolver erro MSB3027
- [COMO_INICIAR.md](../COMO_INICIAR.md) - Guia de início rápido

---

## Troubleshooting

### ❌ Erro: "Porta 5000 já está em uso"
**Solução:**
```batch
parar-sistema.bat
iniciar-sistema.bat
```

### ❌ Erro: "MSB3027: Arquivo bloqueado"
**Solução:**
```batch
parar-sistema.bat
```
Aguarde 5 segundos, depois:
```batch
iniciar-sistema.bat
```

### ❌ Backend não responde após iniciar
**Solução:**
1. Aguarde 10 segundos (pode estar inicializando)
2. Verifique os logs:
   ```powershell
   Get-Content logs\*.log -Tail 20
   ```
3. Verifique se o Firebird está rodando:
   ```powershell
   Get-Service | Where-Object {$_.Name -like "*Firebird*"}
   ```

### ❌ "Cannot find module" no WhatsApp
**Solução:**
```batch
cd C:\VS_Code\icomanda\icomanda-whatsapp-baileys
npm install
```

---

**Desenvolvido por:** GitHub Copilot  
**Última atualização:** 16/02/2026
