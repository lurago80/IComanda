# 🚚 Delivery - Quick Start (TL;DR)

## 1️⃣ Criar Pedido Delivery

```bash
POST /api/vendas/delivery

{
  "cliente": 123,                    # OBRIGATÓRIO!
  "formasPgto": "DINHEIRO",
  "desconto": 0,
  "acrescimo": 15.50,                # Taxa de entrega
  "itens": [
    {
      "codigo": 456,
      "qtd": 2,
      "preco": 25.50,
      "observacao": "bem passado"
    }
  ]
}
```

✅ Automático: WhatsApp enviado + Cupom impresso

---

## 2️⃣ Listar Pedidos Abertos

```bash
GET /api/vendas/delivery/abertos
```

Retorna todos os pedidos delivery aguardando.

---

## 3️⃣ O que MÃO pode fazer na criação

❌ Deixar `cliente = 0`  
❌ Passar `comanda > 0`  
❌ Passar `mesa > 0`  

Se tentar, receberá 400 Bad Request com mensagem clara.

---

## 4️⃣ Diferença: BA vs DL

| | Balcão (BA) | Delivery (DL) |
|---|---|---|
| **Cliente** | Opcional | ✅ OBRIGATÓRIO |
| **Comanda** | ✅ Obrigatório | ❌ Proibido |
| **Mesa** | ✅ Opcional | ❌ Proibido |
| **Endpoint** | POST /api/vendas | POST /api/vendas/delivery |
| **WhatsApp** | Manual | ✅ Automático |
| **Cupom** | Manual | ✅ Automático |

---

## 5️⃣ Fluxo Completo

```
1. Criar pedido          → POST /api/vendas/delivery
2. Listar abertos        → GET /api/vendas/delivery/abertos
3. Marcar saindo         → POST /api/vendas/delivery/{nota}/saiu-para-entrega
4. Fechar (igual comanda) → POST /api/recebimentos/fechar-venda/{nota}
```

---

## 6️⃣ Filtrar Relatórios por Tipo

```bash
# Só delivery
GET /api/relatorios/vendas?origem=DL

# Só balcão
GET /api/relatorios/vendas?origem=BA

# Todos
GET /api/relatorios/vendas
```

---

## 7️⃣ Dados Importantes

- **Origem da venda**: "BA" (balcão) ou "DL" (delivery)
- **Endereço**: Montado automaticamente do cadastro cliente
- **Telefone**: Busca celular, depois telefone
- **Produto Taxa**: Use "TAXA DE ENTREGA" ou similar

---

**Simples assim! 🚀**
