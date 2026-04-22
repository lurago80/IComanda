# 📦 Instruções de Instalação no Cliente

## ✅ O QUE VOCÊ JÁ TEM PRONTO

O pacote de instalação está na pasta:
```
C:\fontes\icomanda\IComanda.API\deploy-package\
```

Esta pasta contém:
- ✅ **backend/** - Backend publicado e pronto
- ✅ **frontend/** - Frontend buildado e pronto
- ✅ Scripts de instalação
- ✅ Documentação

---

## 📋 O QUE FAZER AGORA

### 1️⃣ **COPIAR PARA O PC DO CLIENTE**

Copie a pasta **`deploy-package`** completa para o PC do cliente.

**Caminho recomendado no cliente:**
```
C:\IComanda\
```

Então a estrutura ficará:
```
C:\IComanda\
  ├── backend\
  ├── frontend\
  ├── GUIA_INSTALACAO.md
  └── LEIA-ME.txt
```

---

### 2️⃣ **INSTALAR .NET 8.0 RUNTIME NO CLIENTE**

No PC do cliente:

1. Baixe o **.NET 8.0 Runtime** (não SDK):
   - https://dotnet.microsoft.com/download/dotnet/8.0
   - Escolha: **.NET Desktop Runtime 8.0.x** (Windows)

2. Execute o instalador

3. Verifique a instalação:
   ```cmd
   dotnet --version
   ```
   Deve mostrar: `8.0.x` ou superior

---

### 3️⃣ **CONFIGURAR BANCO DE DADOS**

No PC do cliente, edite o arquivo:
```
C:\IComanda\backend\appsettings.json
```

**Ajuste a ConnectionString:**
```json
{
  "ConnectionStrings": {
    "Firebird": "Server=localhost;Database=C:\\Dados\\ICOMANDA.FDB;User=SYSDBA;Password=masterkey;Port=3050;Charset=UTF8;"
  }
}
```

**⚠️ IMPORTANTE:**
- `Server` - IP ou nome do servidor Firebird (localhost se local)
- `Database` - Caminho completo do arquivo .FDB
- `User` - Usuário do Firebird (geralmente SYSDBA)
- `Password` - Senha do Firebird

---

### 4️⃣ **INSTALAR COMO SERVIÇO WINDOWS (RECOMENDADO)**

No PC do cliente:

1. Abra o **CMD como Administrador**
   - Clique com botão direito no CMD → "Executar como administrador"

2. Navegue até a pasta:
   ```cmd
   cd C:\IComanda\backend
   ```

3. Execute o script de instalação:
   ```cmd
   install-service.bat
   ```

4. O serviço será criado e iniciado automaticamente

**Para verificar se está rodando:**
```cmd
sc query IComandaAPI
```

**Para iniciar/parar manualmente:**
```cmd
net start IComandaAPI
net stop IComandaAPI
```

---

### 5️⃣ **CONFIGURAR FRONTEND**

#### **Opção A: Servir pelo Backend (Mais Simples)**

O backend já está configurado para servir o frontend automaticamente.
Acesse: `http://localhost:5000`

#### **Opção B: Servidor Web Separado (IIS/Nginx)**

1. Copie a pasta `frontend` para o servidor web
2. Configure o servidor para servir os arquivos estáticos
3. Ajuste a URL da API no frontend (se necessário)

---

### 6️⃣ **VERIFICAR INSTALAÇÃO**

1. **Verificar API:**
   - Abra: `http://localhost:5000/swagger`
   - Deve mostrar a documentação da API

2. **Verificar Frontend:**
   - Abra: `http://localhost:5000` (se servido pelo backend)
   - Ou: `http://localhost:3000` (se servido separadamente)

3. **Teste o login e funcionalidades básicas**

---

## 🔧 CONFIGURAÇÕES ADICIONAIS

### **Mudar Porta da API**

Edite `C:\IComanda\backend\appsettings.json`:
```json
{
  "Kestrel": {
    "Port": 8080,
    "Urls": "http://*:8080"
  }
}
```

### **Liberar Porta no Firewall**

Execute no PowerShell como Administrador:
```powershell
New-NetFirewallRule -DisplayName "IComanda API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
```

### **Logs**

Os logs ficam em:
```
C:\IComanda\backend\logs\
```

---

## 📝 CHECKLIST DE INSTALAÇÃO

- [ ] .NET 8.0 Runtime instalado no cliente
- [ ] Firebird instalado e rodando no cliente
- [ ] Banco de dados configurado
- [ ] `appsettings.json` configurado com connection string correta
- [ ] Pasta `deploy-package` copiada para `C:\IComanda\`
- [ ] Serviço Windows instalado e rodando
- [ ] API acessível em `http://localhost:5000/swagger`
- [ ] Frontend acessível e funcionando
- [ ] Login testado e funcionando

---

## 🆘 PROBLEMAS COMUNS

### **Erro: "dotnet não é reconhecido"**
- Instale o .NET 8.0 Runtime
- Reinicie o terminal/PowerShell

### **Erro: "Cannot open database"**
- Verifique o caminho do banco no `appsettings.json`
- Verifique se o Firebird está rodando
- Verifique permissões de acesso ao arquivo .FDB

### **Erro: "Port already in use"**
- Mude a porta no `appsettings.json`
- Ou pare o processo que está usando a porta

### **Serviço não inicia**
- Verifique os logs em `C:\IComanda\backend\logs\`
- Verifique se o .NET Runtime está instalado
- Verifique se o `appsettings.json` está correto

---

## 📞 SUPORTE

Em caso de problemas:
1. Verifique os logs em `C:\IComanda\backend\logs\`
2. Verifique o Event Viewer do Windows (se instalado como serviço)
3. Execute manualmente para ver erros:
   ```cmd
   cd C:\IComanda\backend
   dotnet IComanda.API.dll
   ```

---

## 📦 RESUMO DO QUE COPIAR

**Pasta completa:**
```
C:\fontes\icomanda\IComanda.API\deploy-package\
```

**Copiar para:**
```
C:\IComanda\  (no PC do cliente)
```

**Estrutura final no cliente:**
```
C:\IComanda\
  ├── backend\          (Backend da API)
  ├── frontend\         (Frontend buildado)
  ├── GUIA_INSTALACAO.md
  └── LEIA-ME.txt
```

---

**✅ Pronto! Siga os passos acima para instalar no cliente.**

