# 🚨 CORREÇÃO URGENTE - Caminho do Banco de Dados

## ❌ Problema

A aplicação está tentando acessar o banco em:
- **Caminho ERRADO**: `C:\ICOMANDA\DADOS\DADOSG5.FDB`

Mas deveria usar o caminho do `appsettings.json`:
- **Caminho CORRETO**: `C:\iComanda\Dados\DADOSG5.FDB` (ou o que estiver configurado)

## 🔍 Diagnóstico

Execute o script de diagnóstico:
```powershell
cd C:\fontes\icomanda\IComanda.API
.\DIAGNOSTICAR_CONNECTION_STRING.ps1
```

Este script irá verificar:
- ✅ Se o `appsettings.json` existe e está correto
- ✅ Se há `appsettings.Production.json` ou `appsettings.Development.json` sobrescrevendo
- ✅ Se há variáveis de ambiente sobrescrevendo
- ✅ Se o arquivo está no diretório de publicação

## ✅ Solução Imediata

### Passo 1: Verificar onde a aplicação está rodando

A aplicação procura o `appsettings.json` no **diretório onde ela está executando**, não no diretório do código fonte!

**Se estiver rodando via `dotnet run`:**
- O `appsettings.json` deve estar em: `C:\fontes\icomanda\IComanda.API\appsettings.json`

**Se estiver rodando o executável compilado:**
- O `appsettings.json` deve estar em: `C:\fontes\icomanda\IComanda.API\bin\Release\net8.0\appsettings.json`

**Se estiver rodando como serviço Windows:**
- O `appsettings.json` deve estar no diretório onde o serviço está instalado (geralmente onde está o `.exe`)

### Passo 2: Verificar o appsettings.json no diretório de execução

1. **Identifique onde a aplicação está rodando:**
   - Veja os logs na inicialização - eles mostram o "Base Directory"
   - Ou verifique onde está o arquivo `IComanda.API.exe` ou `IComanda.API.dll`

2. **Verifique o `appsettings.json` nesse diretório:**
   - Abra o arquivo `appsettings.json` no diretório onde a aplicação está rodando
   - Verifique se o caminho do banco está correto

3. **Se o caminho estiver errado, corrija:**
   ```json
   {
     "ConnectionStrings": {
       "Firebird": "Server=localhost;Database=C:\\iComanda\\Dados\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;"
     }
   }
   ```
   **Ajuste o caminho** `C:\\iComanda\\Dados\\DADOSG5.FDB` para o caminho real do banco no cliente!

### Passo 3: Verificar se há outros arquivos sobrescrevendo

Verifique se existe:
- `appsettings.Production.json` - **SOBRESCREVE** o `appsettings.json` em produção
- `appsettings.Development.json` - **SOBRESCREVE** o `appsettings.json` em desenvolvimento

**Se existir, edite ou delete esses arquivos!**

### Passo 4: Verificar variáveis de ambiente

Execute no PowerShell:
```powershell
[Environment]::GetEnvironmentVariable("ConnectionStrings__Firebird")
```

Se retornar algo, essa variável está sobrescrevendo o `appsettings.json`!

**Para remover:**
```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__Firebird", $null, "Machine")
[Environment]::SetEnvironmentVariable("ConnectionStrings__Firebird", $null, "User")
```

### Passo 5: Reiniciar a aplicação

**IMPORTANTE**: Após qualquer alteração no `appsettings.json`, você DEVE reiniciar a aplicação!

## 📋 Checklist de Verificação

- [ ] Executei `.\DIAGNOSTICAR_CONNECTION_STRING.ps1` para diagnosticar
- [ ] Verifiquei o `appsettings.json` no diretório onde a aplicação está rodando
- [ ] O caminho do banco no `appsettings.json` está correto
- [ ] Verifiquei se há `appsettings.{Environment}.json` sobrescrevendo
- [ ] Verifiquei se há variáveis de ambiente sobrescrevendo
- [ ] Reiniciei a aplicação após as alterações
- [ ] Verifiquei os logs na inicialização para confirmar o caminho usado

## 🔍 Logs na Inicialização

Agora a aplicação mostra logs detalhados na inicialização. Procure por:

```
========================================
🔍 VERIFICAÇÃO DE CONNECTION STRING
========================================
Base Directory: ...
appsettings.json path: ...
🔌 Connection String: ...
📁 Caminho do banco: ...
📁 Arquivo existe? ...
========================================
```

**Se o caminho estiver errado, você verá exatamente qual arquivo está sendo usado!**

## ⚠️ IMPORTANTE - No Cliente

Quando fizer o deploy no cliente:

1. **Copie o `appsettings.json` correto** para o diretório onde a aplicação será executada
2. **Edite o `appsettings.json`** com o caminho correto do banco no cliente
3. **Verifique se não há outros arquivos** `appsettings.*.json` sobrescrevendo
4. **Verifique se não há variáveis de ambiente** definidas no sistema do cliente

## 🎯 Resumo

O problema é que o `appsettings.json` no diretório de execução está diferente do esperado, ou há outro arquivo/variável sobrescrevendo.

**Solução**: Verifique e corrija o `appsettings.json` no diretório onde a aplicação está rodando!
