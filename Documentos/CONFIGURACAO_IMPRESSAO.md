# printer Configuração de Impressão

Este documento explica como configurar a impressão de cupons térmicos pela rede.

## 📋 Configuração no appsettings.json

A configuração de impressão está localizada na seção `"Impressao"` do arquivo `appsettings.json`:

```json
{
  "Impressao": {
    "Tipo": "Rede",
    "ImpressoraRede": {
      "IP": "192.168.1.100",
      "Porta": 9100,
      "Nome": "Impressora Termica",
      "LarguraPapel": 80,
      "Timeout": 5000
    },
    "ImpressoraLocal": {
      "Nome": "",
      "LarguraPapel": 80
    }
  }
}
```

## 🔧 Parâmetros de Configuração

### Tipo de Impressão
- **"Rede"**: Impressão via rede (TCP/IP)
- **"Local"**: Impressão via impressora local do Windows

### ImpressoraRede (quando Tipo = "Rede")

| Parâmetro | Descrição | Valor Padrão | Exemplo |
|-----------|-----------|--------------|---------|
| **IP** | Endereço IP da impressora na rede | `192.168.1.100` | `192.168.1.50` |
| **Porta** | Porta TCP da impressora | `9100` | `9100` (RAW) ou `515` (LPR) |
| **Nome** | Nome descritivo da impressora | `Impressora Termica` | `Cupom Balcao` |
| **LarguraPapel** | Largura do papel em mm | `80` | `80` (cupom) ou `58` (bobina) |
| **Timeout** | Timeout de conexão em ms | `5000` | `3000` a `10000` |

### ImpressoraLocal (quando Tipo = "Local")

| Parâmetro | Descrição | Valor Padrão |
|-----------|-----------|--------------|
| **Nome** | Nome da impressora no Windows | `""` |
| **LarguraPapel** | Largura do papel em mm | `80` |

## 🔍 Como Descobrir o IP da Impressora

### Método 1: Menu da Impressora
1. Na impressora térmica, pressione o botão de menu/configuração
2. Navegue até "Configuração de Rede" ou "Network Settings"
3. Anote o endereço IP exibido

### Método 2: Relatório de Configuração
1. Na impressora, imprima o relatório de configuração (geralmente segurando um botão específico)
2. O relatório mostrará o IP configurado

### Método 3: Roteador/Network
1. Acesse o painel do roteador (geralmente `192.168.1.1` ou `192.168.0.1`)
2. Procure por "Dispositivos Conectados" ou "DHCP Clients"
3. Identifique a impressora pelo nome ou MAC address
4. Anote o IP atribuído

### Método 4: Ping/Scan de Rede
```powershell
# Escanear rede local (Windows)
for /L %i in (1,1,254) do @ping -n 1 -w 100 192.168.1.%i | find "TTL"
```

## 🔌 Portas Comuns de Impressoras Térmicas

- **9100**: Porta RAW (mais comum para impressoras térmicas)
- **515**: Porta LPR/LPD (protocolo de impressão em rede)
- **631**: Porta IPP (Internet Printing Protocol)

## 📝 Exemplo de Configuração

### Impressora na Rede Local
```json
{
  "Impressao": {
    "Tipo": "Rede",
    "ImpressoraRede": {
      "IP": "192.168.1.50",
      "Porta": 9100,
      "Nome": "Cupom Balcao",
      "LarguraPapel": 80,
      "Timeout": 5000
    }
  }
}
```

### Impressora Local do Windows
```json
{
  "Impressao": {
    "Tipo": "Local",
    "ImpressoraLocal": {
      "Nome": "EPSON TM-T20",
      "LarguraPapel": 80
    }
  }
}
```

## ✅ Teste de Conectividade

Para testar se a impressora está acessível na rede:

```powershell
# Windows PowerShell
Test-NetConnection -ComputerName 192.168.1.100 -Port 9100

# Ou usando ping
ping 192.168.1.100
```

## ⚠️ Observações Importantes

1. **Firewall**: Certifique-se de que o firewall não está bloqueando a porta da impressora
2. **Rede**: A impressora e o servidor devem estar na mesma rede ou com roteamento configurado
3. **IP Fixo**: Recomenda-se configurar IP fixo na impressora para evitar mudanças
4. **Timeout**: Ajuste o timeout conforme a velocidade da rede (valores maiores para redes mais lentas)

## 🔄 Após Configurar

1. Salve o arquivo `appsettings.json`
2. Reinicie o serviço/backend para aplicar as alterações
3. Teste a impressão através da interface

## 📞 Suporte

Se tiver problemas com a configuração:
1. Verifique se o IP está correto
2. Teste a conectividade (ping)
3. Verifique se a porta está aberta
4. Consulte o manual da impressora para configurações específicas

