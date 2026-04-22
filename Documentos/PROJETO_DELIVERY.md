# Projeto Delivery – iComanda

## 1. Ideia e convenção

- **Mesmas tabelas** das comandas: `VENDAS`, `ITEVENDAS`, `frente_tmpitvendas`, clientes, receber, etc.
- **Diferença por origem** no campo `VENDAS.ORIGEM`:
  - **`BA`** (Balcão): comanda/mesa, uso atual.
  - **`DL`** (Delivery): pedido para entrega.

Assim não criamos estrutura nova: só outro “tipo” de venda, com relatórios e telas filtrando por `ORIGEM`.

---

## 2. Melhorias em cima da ideia

| Ponto | Proposta |
|-------|----------|
| **Cliente** | No delivery, **cliente obrigatório** (nome, endereço, telefone para entrega). |
| **Endereço** | Usar endereço do cadastro do cliente (CLIENTES: endereço1, bairro1, cidade1, cep1). Opcional depois: campo “referência” ou “complemento” no pedido. |
| **Taxa de entrega** | Já existe produto “TAXA DE ENTREGA” e cadastro de taxas; usar no pedido de delivery normalmente. |
| **Comanda/Mesa** | Em pedidos `ORIGEM = DL` não usar comanda nem mesa (ficam null). |
| **Relatórios** | Relatório de vendas e caixa poderem filtrar por origem (BA x DL) para separar comanda e delivery. |
| **Status de entrega** | Fase 2: opcionalmente um status (ex.: Pendente → Saiu para entrega → Entregue), via campo em `VENDAS` ou tabela auxiliar. Por ora só “aberto” x “fechado” como hoje. |

---

## 3. Regras de negócio (resumo)

- **Novo pedido delivery**
  - Usuário escolhe **cliente** (obrigatório).
  - Monta o pedido (produtos + eventual “TAXA DE ENTREGA”).
  - Ao finalizar: grava venda com `ORIGEM = 'DL'`, `CLIENTE` preenchido, `COMANDA`/`MESA` em branco.
- **Listagem**
  - “Comandas abertas”: `LANCADO = 'ABERTO'` e `ORIGEM = 'BA'`.
  - “Pedidos delivery abertos”: `LANCADO = 'ABERTO'` e `ORIGEM = 'DL'`.
- **Fechamento**
  - Mesmo fluxo de fechamento/recebimento das vendas atuais; origem não muda (continua DL).
- **Impressão / WhatsApp**
  - Podem ser usados para pedido/recibo de delivery como hoje; no texto pode aparecer “Pedido delivery” ou “Entrega” quando `ORIGEM = DL`.

---

## 4. Fases de implementação

### Fase 1 – Backend
- Incluir **Origem** (ou equivalente) no request de criação de venda; ao criar pedido delivery, enviar `Origem = "DL"`.
- **Criação de venda**: aceitar `Origem` no request; default `"BA"`; persistir em `VENDAS` (e onde mais for necessário).
- **Vendas abertas**: novo endpoint ou parâmetro para listar por origem (ex.: `GET /vendas/abertas?origem=DL`).
- **GetVenda / AtualizarTotal / Fechar**: garantir que funcionem para vendas com `ORIGEM = 'DL'` (não travar apenas em `origem = 'BA'`).

### Fase 2 – Frontend
- **Menu**: item “Delivery” com:
  - **Novo pedido delivery**: fluxo que exige cliente, depois produtos (e taxa de entrega se quiser), e finaliza com `origem = DL`.
  - **Pedidos delivery**: lista de vendas abertas com `origem = DL` (mesmo conceito da “grade de comandas”, só que para delivery).
- Reaproveitar carrinho, finalização e recebimento já existentes, passando `origem: 'DL'` quando for delivery.

### Fase 3 – Ajustes e relatórios (opcional)
- Relatório de vendas: filtro por origem (BA / DL).
- Caixa/consolidado: considerar ou separar vendas DL se fizer sentido.
- Melhorias de UX: exibir endereço do cliente na listagem de pedidos delivery; botão “Imprimir” / “WhatsApp” já podem usar o que já existe.

---

## 5. Convenções técnicas

- **ORIGEM**
  - `BA` = comanda (balcão/mesa).
  - `DL` = delivery.
- **Cliente**
  - Em pedidos DL: obrigatório; endereço e telefone vêm do cadastro do cliente.
- **Comanda/Mesa**
  - Em pedidos DL: não utilizados (null ou 0).
- **Taxa de entrega**
  - Produto “TAXA DE ENTREGA” + cadastro de taxas; uso igual ao atual no pedido.

Este documento serve como base para implementação e para alinhar novas melhorias (ex.: status de entrega, relatórios por origem) sem mudar a ideia de usar as mesmas tabelas e diferenciar por `ORIGEM`.
