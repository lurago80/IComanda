# 🔧 Correção do Caminho do Banco de Dados

## ❌ Problema Identificado

A aplicação estava usando um caminho fixo/antigo do banco de dados:
- **Caminho incorreto**: `C:\Users\usuario\Desktop\Base\Eduardo\dadosg5.fdb`
- **Caminho correto**: `C:\iComanda\Dados\DADOSG5.FDB`

## ✅ Correções Aplicadas

### 1. Atualização do `appsettings.json`
O arquivo `appsettings.json` foi atualizado com o caminho correto:
```json
{
  "ConnectionStrings": {
    "Firebird": "Server=localhost;Database=C:\\iComanda\\Dados\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;"
  }
}
```

### 2. Logging Adicionado
- Adicionado logging no `DbConnectionFactory` para mostrar qual connection string está sendo usada
- Adicionado logging no `Program.cs` na inicialização para mostrar o caminho do banco
- Logs mostram o caminho completo (sem senha) para facilitar debug

### 3. Verificação de Arquivos de Configuração
- O `Program.cs` agora verifica e loga qual arquivo `appsettings.json` está sendo usado
- Verifica se há `appsettings.{Environment}.json` que possa estar sobrescrevendo

### 4. Garantia de Cópia no Build
- Adicionado no `.csproj` para garantir que `appsettings.json` seja copiado para o diretório de saída

## 📋 Como Verificar se Está Funcionando

### 1. Execute o Script de Verificação
```powershell
cd C:\iComanda\IComanda.API
.\VERIFICAR_CONNECTION_STRING.ps1
```

Este script irá:
- Verificar se o `appsettings.json` existe
- Mostrar a connection string configurada
- Verificar se o arquivo de banco existe no caminho especificado
- Verificar se há outros arquivos de configuração sobrescrevendo

### 2. Verifique os Logs na Inicialização
Quando a aplicação iniciar, você verá no console:
```
========================================
CONFIGURAÇÃO DE BANCO DE DADOS
========================================
Base Directory: C:\iComanda\...
Environment: Production
appsettings.json path: C:\iComanda\...\appsettings.json
...
🔌 Connection String: Server=localhost;Database=C:\iComanda\Dados\DADOSG5.FDB;User=SYSDBA;Password=***;Port=3050;
📁 Base Directory: C:\iComanda\...
========================================
```

## ⚠️ IMPORTANTE - Após Deploy

### 1. Verificar o `appsettings.json` no Cliente
Após fazer o deploy, **SEMPRE verifique** se o `appsettings.json` no diretório de instalação do cliente está com o caminho correto:

**Localização esperada**: `C:\iComanda\IComanda.API\appsettings.json`

### 2. Se o Caminho Estiver Diferente no Cliente
Se o cliente tiver o banco em outro caminho, você precisa:

1. **Editar o `appsettings.json` no diretório de instalação**:
   ```json
   {
     "ConnectionStrings": {
       "Firebird": "Server=localhost;Database=C:\\CAMINHO\\CORRETO\\DADOSG5.FDB;User=SYSDBA;Password=masterkey;Port=3050;"
     }
   }
   ```

2. **REINICIAR a aplicação** (serviço ou processo)

### 3. Verificar se Não Há Outros Arquivos Sobrescrevendo
Verifique se existe:
- `appsettings.Development.json` (pode sobrescrever em desenvolvimento)
- `appsettings.Production.json` (pode sobrescrever em produção)
- Variáveis de ambiente `ConnectionStrings__Firebird`

## 🔍 Troubleshooting

### Problema: Ainda está usando o caminho antigo

**Soluções**:

1. **Verifique qual `appsettings.json` está sendo usado**:
   - Execute `VERIFICAR_CONNECTION_STRING.ps1`
   - Veja os logs na inicialização da aplicação

2. **Verifique o diretório onde a aplicação está rodando**:
   - A aplicação procura `appsettings.json` no `AppContext.BaseDirectory`
   - Se a aplicação estiver rodando de outro diretório, o arquivo pode não estar sendo encontrado

3. **Verifique se há variáveis de ambiente**:
   ```powershell
   [Environment]::GetEnvironmentVariable("ConnectionStrings__Firebird")
   ```
   Se retornar algo, essa variável está sobrescrevendo o `appsettings.json`

4. **Verifique se o arquivo foi copiado no build**:
   - O `appsettings.json` deve estar no diretório `bin\Release\net8.0\` (ou `bin\Debug\net8.0\`)
   - Se não estiver, o build não copiou corretamente

### Problema: "Connection string 'Firebird' não encontrada"

**Soluções**:

1. Verifique se o `appsettings.json` existe no diretório de execução
2. Verifique se a estrutura JSON está correta (sem erros de sintaxe)
3. Verifique se a chave está exatamente como `"ConnectionStrings": { "Firebird": "..." }`

## 📝 Checklist para Deploy

- [ ] Atualizar `appsettings.json` com o caminho correto do banco
- [ ] Verificar se o caminho do banco existe no cliente
- [ ] Fazer build da aplicação
- [ ] Verificar se `appsettings.json` foi copiado para `bin\Release\net8.0\`
- [ ] Copiar o `appsettings.json` correto para o diretório de instalação no cliente
- [ ] Verificar se não há `appsettings.{Environment}.json` sobrescrevendo
- [ ] Verificar se não há variáveis de ambiente sobrescrevendo
- [ ] Reiniciar a aplicação
- [ ] Verificar os logs na inicialização para confirmar o caminho usado

## 🎯 Resumo

O problema era que o `appsettings.json` tinha um caminho antigo hardcoded. Agora:

1. ✅ O `appsettings.json` foi atualizado com o caminho correto
2. ✅ Logging foi adicionado para facilitar debug
3. ✅ Script de verificação foi criado
4. ✅ Garantia de cópia no build foi adicionada

**Lembre-se**: Após qualquer alteração no `appsettings.json`, **SEMPRE reinicie a aplicação**!
