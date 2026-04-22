-- ================================================================
-- Script SQL: Índices de Performance - Firebird
-- Projeto: iComanda
-- Propósito: Otimizar queries em VENDAS, ITEVENDAS, CLIENTES
-- Data: 2026-02-13
-- ================================================================

-- IMPORTANTE: Este script deve ser executado uma ÚNICA vez contra o banco de dados
-- Conexão: SYSDBA ou usuário com permissão de criar índices

-- ================================================================
-- 1. ÍNDICES NA TABELA VENDAS (Tabela principal)
-- ================================================================

-- Índice em NOTA (chave primária, já existe - comentado para referência)
-- CREATE UNIQUE INDEX IDX_VENDAS_NOTA ON VENDAS (NOTA);

-- Índice em COMANDA (filtragem por comanda)
CREATE INDEX IDX_VENDAS_COMANDA ON VENDAS (COMANDA);

-- Índice em DATA (filtragem por período)
CREATE INDEX IDX_VENDAS_DATA ON VENDAS (DATA);

-- Índice composto: DATA + NOTA (buscas por período e ordenação)
CREATE INDEX IDX_VENDAS_DATA_NOTA ON VENDAS (DATA DESC, NOTA);

-- Índice em MESA (filtragem por mesa)
CREATE INDEX IDX_VENDAS_MESA ON VENDAS (MESA);

-- Índice em STATUS (filtragem por vendas abertas/fechadas)
CREATE INDEX IDX_VENDAS_STATUS ON VENDAS (STATUS);

-- Índice em OPERADOR (auditoria de quem criou)
CREATE INDEX IDX_VENDAS_OPERADOR ON VENDAS (OPERADOR);

-- Índice em DESCRICAO_CLIENTE (busca por delivery cliente)
CREATE INDEX IDX_VENDAS_DESCRICAO_CLIENTE ON VENDAS (DESCRICAO_CLIENTE);

-- ================================================================
-- 2. ÍNDICES NA TABELA ITEVENDAS (Itens de vendas)
-- ================================================================

-- Índice em CUPOM (relacionamento com VENDAS)
CREATE INDEX IDX_ITEVENDAS_CUPOM ON ITEVENDAS (CUPOM);

-- Índice em CODIGO (filtragem por produto)
CREATE INDEX IDX_ITEVENDAS_CODIGO ON ITEVENDAS (CODIGO);

-- Índice composto: CUPOM + ITEM (busca rápida de item específico)
CREATE UNIQUE INDEX IDX_ITEVENDAS_CUPOM_ITEM ON ITEVENDAS (CUPOM, ITEM);

-- ================================================================
-- 3. ÍNDICES NA TABELA CLIENTES
-- ================================================================

-- Índice em NOME (busca de cliente por nome)
CREATE INDEX IDX_CLIENTES_NOME ON CLIENTES (NOME);

-- Índice em CPFCNPJ (busca de cliente por CPF/CNPJ)
CREATE INDEX IDX_CLIENTES_CPFCNPJ ON CLIENTES (CPFCNPJ);

-- Índice em ATIVO (filtragem de clientes ativos)
CREATE INDEX IDX_CLIENTES_ATIVO ON CLIENTES (ATIVO);

-- ================================================================
-- 4. ÍNDICES NA TABELA PRODUTO (ou PRODUTOS)
-- ================================================================

-- Índice em CODIGO (busca de produto por código)
CREATE UNIQUE INDEX IDX_PRODUTO_CODIGO ON PRODUTO (CODIGO);

-- Índice em DESCRICAO (busca textual de produto)
CREATE INDEX IDX_PRODUTO_DESCRICAO ON PRODUTO (DESCRICAO);

-- Índice em GRUPO (filtragem por categoria)
CREATE INDEX IDX_PRODUTO_GRUPO ON PRODUTO (GRUPO);

-- Índice em ATIVO (filtragem de produtos ativos)
CREATE INDEX IDX_PRODUTO_ATIVO ON PRODUTO (ATIVO);

-- ================================================================
-- 5. ÍNDICES NA TABELA USUARIO (Autenticação)
-- ================================================================

-- Índice em NOME (login por nome de usuário)
CREATE UNIQUE INDEX IDX_USUARIO_NOME ON USUARIO (NOME);

-- Índice em ATIVO (filtragem de usuários ativos)
CREATE INDEX IDX_USUARIO_ATIVO ON USUARIO (ATIVO);

-- ================================================================
-- 6. ÍNDICES NA TABELA CAIXA (Movimentação de caixa)
-- ================================================================

-- Índice em DATA (filtragem por período)
CREATE INDEX IDX_CAIXA_DATA ON CAIXA (DATA);

-- Índice em OPERADOR (auditoria)
CREATE INDEX IDX_CAIXA_OPERADOR ON CAIXA (OPERADOR);

-- Índice em STATUS (filtragem por caixa aberto/fechado)
CREATE INDEX IDX_CAIXA_STATUS ON CAIXA (STATUS);

-- ================================================================
-- 7. ÍNDICES NA TABELA RECEBIMENTO
-- ================================================================

-- Índice em CUPOM (relacionamento com VENDAS)
CREATE INDEX IDX_RECEBIMENTO_CUPOM ON RECEBIMENTO (CUPOM);

-- Índice em DATA (filtragem por período)
CREATE INDEX IDX_RECEBIMENTO_DATA ON RECEBIMENTO (DATA);

-- Índice em FORMA (filtragem por forma de pagamento)
CREATE INDEX IDX_RECEBIMENTO_FORMA ON RECEBIMENTO (FORMA);

-- ================================================================
-- INFORMAÇÕES E DICAS
-- ================================================================

/*
DICAS DE PERFORMANCE:

1. VALIDAR ÍNDICES CRIADOS:
   SELECT * FROM RDB$INDICES 
   WHERE RDB$RELATION_NAME = 'VENDAS'
   ORDER BY RDB$INDEX_NAME;

2. MONITORAR ÍNDICES (Firebird Enterprise):
   SELECT * FROM RDB$INDEX_STATISTICS
   WHERE RDB$INDEX_NAME LIKE 'IDX_VENDAS_%';

3. RECRIAR ÍNDICE SE NECESSÁRIO:
   ALTER INDEX idx_vendas_data ACTIVE;
   -- ou
   ALTER INDEX idx_vendas_data INACTIVE;
   ALTER INDEX idx_vendas_data ACTIVE;

4. LIMPEZA DE ÍNDICES ÓRFÃOS:
   SELECT RDB$INDEX_NAME FROM RDB$INDICES 
   WHERE RDB$RELATION_NAME = 'VENDAS' 
   AND RDB$INDEX_NAME LIKE 'IDX_%';

5. ESTATÍSTICAS DE PERFORMANCE:
   -- Ativar em firebird.conf:
   -- DatabaseAccessPath = ...
   -- RedirectIO = 1
   -- UserAuthenticationPlugin = Srp,Win1252
   
6. TAMANHO DO BANCO:
   SELECT DATEDIFF(DAY, '1970-01-01', NOW()) * 1 + 
          (SELECT COUNT(*) FROM VENDAS) * 100 as ESTIMATED_SIZE_MB;

7. ÍNDICES MAIS USADOS (Firebird 4.0+):
   SELECT RDB$INDEX_NAME, COUNT(*) 
   FROM RDB$INDICES 
   WHERE RDB$RELATION_NAME = 'VENDAS'
   GROUP BY RDB$INDEX_NAME;
*/

-- ================================================================
-- SCRIPTS DE TESTE
-- ================================================================

-- Teste 1: Buscar vendas por período (deve usar IDX_VENDAS_DATA)
-- EXPLAIN PLAN FOR
-- SELECT * FROM VENDAS 
-- WHERE DATA BETWEEN '2026-01-01' AND '2026-02-13'
-- ORDER BY DATA DESC, NOTA;

-- Teste 2: Buscar venda por comanda (deve usar IDX_VENDAS_COMANDA)
-- EXPLAIN PLAN FOR
-- SELECT * FROM VENDAS WHERE COMANDA = '001';

-- Teste 3: Buscar cliente por CPF (deve usar IDX_CLIENTES_CPFCNPJ)
-- EXPLAIN PLAN FOR
-- SELECT * FROM CLIENTES WHERE CPFCNPJ = '12345678901234';

-- Teste 4: Buscar produto por código (deve usar IDX_PRODUTO_CODIGO)
-- EXPLAIN PLAN FOR
-- SELECT * FROM PRODUTO WHERE CODIGO = 12345;

-- ================================================================
-- SCRIPT DE DESFAZIMENTO (Se necessário remover índices)
-- ================================================================

/*
-- Comentado por segurança. Descomentar APENAS se necessário remover índices

DROP INDEX IDX_VENDAS_COMANDA;
DROP INDEX IDX_VENDAS_DATA;
DROP INDEX IDX_VENDAS_DATA_NOTA;
DROP INDEX IDX_VENDAS_MESA;
DROP INDEX IDX_VENDAS_STATUS;
DROP INDEX IDX_VENDAS_OPERADOR;
DROP INDEX IDX_VENDAS_DESCRICAO_CLIENTE;

DROP INDEX IDX_ITEVENDAS_CUPOM;
DROP INDEX IDX_ITEVENDAS_CODIGO;
DROP INDEX IDX_ITEVENDAS_CUPOM_ITEM;

DROP INDEX IDX_CLIENTES_NOME;
DROP INDEX IDX_CLIENTES_CPFCNPJ;
DROP INDEX IDX_CLIENTES_ATIVO;

DROP INDEX IDX_PRODUTO_CODIGO;
DROP INDEX IDX_PRODUTO_DESCRICAO;
DROP INDEX IDX_PRODUTO_GRUPO;
DROP INDEX IDX_PRODUTO_ATIVO;

DROP INDEX IDX_USUARIO_NOME;
DROP INDEX IDX_USUARIO_ATIVO;

DROP INDEX IDX_CAIXA_DATA;
DROP INDEX IDX_CAIXA_OPERADOR;
DROP INDEX IDX_CAIXA_STATUS;

DROP INDEX IDX_RECEBIMENTO_CUPOM;
DROP INDEX IDX_RECEBIMENTO_DATA;
DROP INDEX IDX_RECEBIMENTO_FORMA;
*/

-- ================================================================
-- FIM DO SCRIPT
-- ================================================================
COMMIT;
