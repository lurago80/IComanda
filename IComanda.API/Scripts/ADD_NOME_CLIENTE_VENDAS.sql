-- Adiciona coluna nome_cliente na tabela VENDAS para exibir o nome digitado
-- quando o cliente não foi cadastrado (cliente = 0). Aparece nos cards de comandas e na impressão.
-- Execute este script uma vez no banco Firebird antes de usar a funcionalidade.

ALTER TABLE vendas ADD nome_cliente VARCHAR(200);
COMMIT;
