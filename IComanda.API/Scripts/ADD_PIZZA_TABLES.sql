-- ============================================================
-- Migration: Sistema de Pizzas Montáveis
-- Executar uma única vez. Erros de "already exists" podem ser ignorados.
-- ============================================================

-- 1. Adiciona tipo ao grupo (NORMAL = padrão, PIZZA = montagem)
ALTER TABLE GRUPO ADD TIPO VARCHAR(10) DEFAULT 'NORMAL';

-- 2. Tamanhos de pizza vinculados a um grupo
CREATE TABLE PIZZA_TAMANHOS (
    ID      INTEGER      NOT NULL PRIMARY KEY,
    GRUPO_ID INTEGER     NOT NULL,
    DESCRICAO VARCHAR(30) NOT NULL,
    ORDEM   INTEGER      DEFAULT 0
);
CREATE GENERATOR PIZZA_TAMANHOS_GEN;

-- 3. Sabores por tamanho
CREATE TABLE PIZZA_SABORES (
    ID          INTEGER       NOT NULL PRIMARY KEY,
    TAMANHO_ID  INTEGER       NOT NULL,
    DESCRICAO   VARCHAR(100)  NOT NULL,
    INGREDIENTES VARCHAR(300),
    PRECO       DECIMAL(10,2) DEFAULT 0,
    ATIVO       CHAR(1)       DEFAULT '1'
);
CREATE GENERATOR PIZZA_SABORES_GEN;

-- 4. Bordas (globais — compartilhadas entre todos os tamanhos)
CREATE TABLE PIZZA_BORDAS (
    ID        INTEGER       NOT NULL PRIMARY KEY,
    DESCRICAO VARCHAR(50)   NOT NULL,
    PRECO     DECIMAL(10,2) DEFAULT 0,
    ATIVO     CHAR(1)       DEFAULT '1'
);
CREATE GENERATOR PIZZA_BORDAS_GEN;

-- Índices de performance
CREATE INDEX IDX_PIZZA_TAMANHOS_GRUPO ON PIZZA_TAMANHOS (GRUPO_ID);
CREATE INDEX IDX_PIZZA_SABORES_TAMANHO ON PIZZA_SABORES (TAMANHO_ID);
CREATE INDEX IDX_PIZZA_SABORES_ATIVO ON PIZZA_SABORES (ATIVO);
CREATE INDEX IDX_PIZZA_BORDAS_ATIVO ON PIZZA_BORDAS (ATIVO);
