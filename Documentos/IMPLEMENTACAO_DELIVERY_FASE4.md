# 🚚 Phase 4 - Delivery - Implementação Completa

## ✅ O que foi implementado

### 1. Validações de Negócio para Delivery

✅ **Cliente Obrigatório**: Pedidos de delivery DEVEM ter um cliente válido
- Lançar exceção se `Cliente <= 0` para origem DL
- Mensagem clara: "Cliente é obrigatório para pedidos de delivery"

✅ **Sem Comanda/Mesa**: Pedidos delivery NÃO usam comanda ou mesa
- Validação rejeita se `Comanda > 0` ou `Mesa > 0` para origem DL
- Mensagem clara: "Pedidos de delivery não utilizam comanda/mesa"

✅ **Origem "BA" vs "DL"**:
- BA = Balcão (comanda/mesa obrigatória)
- DL = Delivery (cliente obrigatório, comanda/mesa proibida)

### 2. Novos DTOs

✅ **PedidoDeliveryDto** - Visão simplificada para pedidos delivery
- Informações essenciais: nota, cliente, endereço, telefone, totais
- Itens com observações (ex: "sem cebola")
- Pronto para exibir na tela de "Pedidos para Entrega"

✅ **ItemPedidoDeliveryDto** - Item simplificado para delivery
- Código, descrição, quantidade, preço
- Total calculado
- Observações especiais

✅ **CriarPedidoDeliveryRequest** - Request específico para delivery
- Cliente obrigatório
- Sem campo de comanda/mesa
- Formas de pagamento adaptadas (dinheiro, cartão, PIX, a prazo)
- Lista de itens

### 3. Novos Endpoints

#### POST /api/vendas/delivery
Cria um novo pedido de delivery

**Request:**
```json
{
  "cliente": 123,
  "formasPgto": "DINHEIRO",
  "especie": "DINHEIRO",
  "desconto": 0,
  "acrescimo": 15.50,
  "operador": 1,
  "vendedor": 1,
  "itens": [
    {
      "codigo": 456,
      "und": "UN",
      "qtd": 2,
      "preco": 25.50,
      "observacao": "sem cebola"
    },
    {
      "codigo": 789,
      "und": "UN",
      "qtd": 1,
      "preco": 15.50,
      "observacao": null
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "nota": "000042",
  "dataHora": "2026-02-13T14:30:00",
  "cliente": 123,
  "nomeCliente": "João Silva",
  "telefoneCliente": "11999999999",
  "enderecoEntrega": "Rua A, 123, Apto 45 - Centro, São Paulo - SP, CEP: 01000-000",
  "subtotal": 66.50,
  "desconto": 0,
  "acrescimo": 15.50,
  "total": 82.00,
  "formasPgto": "DINHEIRO",
  "totalItens": 2,
  "lancado": "ABERTO",
  "operador": 1,
  "itens": [
    {
      "codigo": 456,
      "descricao": "Pão Francês",
      "quantidade": 2,
      "preco": 25.50,
      "total": 51.00,
      "observacao": "sem cebola"
    },
    {
      "codigo": 789,
      "descricao": "Taxa de Entrega",
      "quantidade": 1,
      "preco": 15.50,
      "total": 15.50,
      "observacao": null
    }
  ]
}
```

#### GET /api/vendas/delivery/abertos
Lista todos os pedidos de delivery em aberto

**Query Parameters:**
- Nenhum (filtra automaticamente por origem = "DL" e lancado = "ABERTO")

**Response (200 OK):**
```json
[
  {
    "nota": "000042",
    "dataHora": "2026-02-13T14:30:00",
    "cliente": 123,
    "nomeCliente": "João Silva",
    "telefoneCliente": "11999999999",
    "enderecoEntrega": "Rua A, 123, Apto 45 - Centro, São Paulo - SP, CEP: 01000-000",
    "subtotal": 66.50,
    "desconto": 0,
    "acrescimo": 15.50,
    "total": 82.00,
    "formasPgto": "DINHEIRO",
    "totalItens": 2,
    "lancado": "ABERTO",
    "operador": 1,
    "itens": [...]
  }
]
```

### 4. Novos Métodos de Serviço

✅ **VendaService.CriarPedidoDeliveryAsync(CriarPedidoDeliveryRequest)**
- Converte CriarPedidoDeliveryRequest para CriarVendaRequest com origem = "DL"
- Valida cliente obrigatório
- Impede comanda/mesa
- Retorna PedidoDeliveryDto
- Automáticamente envia WhatsApp de "Pedido aprovado" se cliente tiver telefone
- Imprime cupom para entregador

✅ **VendaService.GetPedidosDeliveryAbertosAsync()**
- Lista apenas vendas com origem = "DL" e lancado = "ABERTO"
- Retorna collection de PedidoDeliveryDto
- Inclui informações do cliente e endereço de entrega

### 5. Melhorias em Relatórios

Todos os relatórios agora suportam filtro por origem:

✅ **GET /api/relatorios/cliente/{codigoCliente}?origem=DL**
- Filtra apenas compras de delivery do cliente

✅ **GET /api/relatorios/vendas?dataInicio=...&dataFim=...&origem=BA**
- Filtra apenas vendas de balcão/comanda

✅ **GET /api/relatorios/periodo?dataInicio=...&dataFim=...&origem=DL**
- Relatório de período apenas para delivery

✅ **GET /api/relatorios/caixa-consolidado?dataInicio=...&dataFim=...&origem=DL**
- Consolidado de caixa apenas para delivery

### 6. Fluxo Completo de Delivery

```
1. GARÇOM/OPERADOR
   ↓
   POST /api/vendas/delivery
   - Escolhe cliente (obrigatório)
   - Seleciona produtos (+ taxa de entrega se quiser)
   - Define forma de pagamento
   ↓
   RETORNA: Pedido criado (nota, cliente, endereço, total)
   - WhatsApp enviado automaticamente ao cliente
   - Cupom impresso automaticamente para entregador

2. CONSULTAR PEDIDOS
   ↓
   GET /api/vendas/delivery/abertos
   - Lista todos pedidos delivery aguardando
   - Mostra cliente, endereço, total

3. MARCAR COMO SAINDO PARA ENTREGA
   ↓
   POST /api/vendas/delivery/{nota}/saiu-para-entrega
   - Envia WhatsApp: "Seu pedido está saindo para entrega"

4. FECHAR PEDIDO (igual comandas)
   ↓
   POST /api/recebimentos/fechar-venda/{nota}
   - Registra recebimento
   - Muda status para EFETIVADO
   - Gera contas a receber se necessário (a prazo)
```

### 7. Arquivos Criados/Modificados

**Criados:**
- [Models/DTOs/PedidoDeliveryDto.cs](Models/DTOs/PedidoDeliveryDto.cs)
- [Models/Requests/CriarPedidoDeliveryRequest.cs](Models/Requests/CriarPedidoDeliveryRequest.cs)

**Modificados - Validações:**
- [Services/Implementations/VendaService.cs](Services/Implementations/VendaService.cs):
  - Adicionar validações de cliente obrigatório para DL
  - Adicionar validações de comanda/mesa proibidas para DL
  - CriarVendaAsync() agora valida origem e impõe regras específicas
  - Novo método CriarPedidoDeliveryAsync()
  - Novo método GetPedidosDeliveryAbertosAsync()
  - ConvertVendaToPedidoDeliveryDtoAsync() helper

- [Services/Interfaces/IVendaService.cs](Services/Interfaces/IVendaService.cs):
  - CriarPedidoDeliveryAsync(CriarPedidoDeliveryRequest)
  - GetPedidosDeliveryAbertosAsync()

**Modificados - Controllers:**
- [Controllers/VendasController.cs](Controllers/VendasController.cs):
  - POST /api/vendas/delivery (CriarPedidoDelivery)
  - GET /api/vendas/delivery/abertos (GetPedidosDeliveryAbertos)

**Modificados - Relatórios:**
- [Services/Interfaces/IRelatorioService.cs](Services/Interfaces/IRelatorioService.cs):
  - Adicionar parâmetro `origem` em todos métodos públicos

- [Services/Implementations/RelatorioService.cs](Services/Implementations/RelatorioService.cs):
  - Passar origem para repositório em todos métodos

- [Controllers/RelatoriosController.cs](Controllers/RelatoriosController.cs):
  - Adicionar parâmetro `origem` em todos endpoints
  - Documentação atualizada

## 🧪 Como Testar

### Teste 1: Criar Pedido Delivery

```bash
curl -X POST http://localhost:65375/api/vendas/delivery \
  -H "Content-Type: application/json" \
  -d '{
    "cliente": 1,
    "formasPgto": "DINHEIRO",
    "especie": "DINHEIRO",
    "desconto": 0,
    "acrescimo": 15,
    "operador": 1,
    "vendedor": 1,
    "itens": [
      {
        "codigo": 1,
        "und": "UN",
        "qtd": 2,
        "preco": 10,
        "observacao": "bem passado"
      }
    ]
  }'
```

**Resultado esperado:** 201 Created com PedidoDeliveryDto

### Teste 2: Listar Pedidos Abertos

```bash
curl http://localhost:65375/api/vendas/delivery/abertos
```

**Resultado esperado:** Array com todos os pedidos delivery em aberto

### Teste 3: Relatório de Vendas apenas Delivery

```bash
curl 'http://localhost:65375/api/relatorios/vendas?dataInicio=2026-02-13&dataFim=2026-02-13&origem=DL'
```

**Resultado esperado:** Relatório com apenas vendas delivery

### Teste 4: Erro - Cliente Obrigatório

```bash
curl -X POST http://localhost:65375/api/vendas/delivery \
  -H "Content-Type: application/json" \
  -d '{
    "cliente": 0,
    "formasPgto": "DINHEIRO",
    ...
  }'
```

**Resultado esperado:** 400 Bad Request
```json
{
  "mensagem": "Cliente é obrigatório para pedidos de delivery. Selecione ou cadastre um cliente antes de criar o pedido."
}
```

### Teste 5: Erro - Comanda Não Permitida em Delivery

```bash
curl -X POST http://localhost:65375/api/vendas/delivery \
  -H "Content-Type: application/json" \
  -d '{
    "cliente": 1,
    "comanda": 5,
    ...
  }'
```

**Resultado esperado:** 400 Bad Request
```json
{
  "mensagem": "Pedidos de delivery não utilizam comanda. Deixe o campo em branco."
}
```

## 🚀 Recursos Também Aproveitados

Todos esses recursos já existentes funcionam com delivery (origem = "DL"):

✅ **GET /api/vendas/abertas?origem=DL** - Lista pedidos delivery abertos
✅ **GET /api/vendas/{nota}** - Busca detalhes de um pedido específico
✅ **POST /api/recebimentos/fechar-venda/{nota}** - Fecha pedido (mesmo fluxo de comanda)
✅ **POST /api/vendas/delivery/{nota}/saiu-para-entrega** - Notifica cliente por WhatsApp
✅ **GET /api/relatorios/...?origem=DL** - Todos os relatórios com filtro delivery
✅ Impressão de cupom delivery (automática ao criar)
✅ WhatsApp de pedido aprovado (automático ao criar)

## 📋 Próximos Passos (Fase 2/3 - Futuro)

- [ ] Status de entrega (Pendente → Saiu → Entregue) - campo em VENDAS
- [ ] Tracking em tempo real
- [ ] Mapa de entrega
- [ ] Assinatura de recebimento
- [ ] Foto de entrega
- [ ] Histórico de entregas
- [ ] Relatório de performance entregador
- [ ] Integração com Loggi/Speedy se necessário

## 📝 Notas Importantes

1. **Banco de Dados**: O campo ORIGEM já existe em VENDAS (foi criado antes desta implementação)
2. **WhatsApp**: Automático ao criar delivery se cliente tiver telefone/celular
3. **Impressão**: Automática ao criar delivery (cupom para entregador)
4. **Taxa de Entrega**: Usar produto já existente "TAXA DE ENTREGA" ou similar
5. **Endereço**: Montado automaticamente do cadastro do cliente
6. **Compatibilidade**: Funciona com desktop Delphi (mesmas tabelas)

## 🔗 Fluxo com Desktop

Delivery usa mesmas tabelas VENDAS/ITVENDAS que comanda:

```
FRONTEND (Mobile/Web) criar pedido delivery
  ↓
BACKEND (API) valida e grava em VENDAS (origen=DL)
  ↓
DELPHI (Desktop) lê VENDAS/ITVENDAS como sempre fez
  ↓
CAIXA finaliza na máquina Delphi
  ↓
Status muda para EFETIVADO (igual comanda)
```

NÃO há separação de tabelas - é só um parâmetro ORIGEM para filtragem!
