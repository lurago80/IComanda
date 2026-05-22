-- Migration: Adiciona coluna STATUS_COZINHA na tabela VENDAS para o KDS (Kitchen Display System)
-- Executar apenas uma vez. Se já existir, o banco retornará erro que pode ser ignorado.

ALTER TABLE VENDAS ADD STATUS_COZINHA VARCHAR(20) DEFAULT 'PENDENTE';

-- Atualiza vendas abertas existentes para PENDENTE (as fechadas ficam sem relevância no KDS)
UPDATE VENDAS SET STATUS_COZINHA = 'PENDENTE' WHERE STATUS_COZINHA IS NULL AND LANCADO = 'ABERTO';

-- Índice para performance nas queries do KDS
CREATE INDEX IDX_VENDAS_STATUS_COZINHA ON VENDAS (STATUS_COZINHA);
