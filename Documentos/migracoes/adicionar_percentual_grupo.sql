-- Script de migração: adicionar coluna PERCENTUAL na tabela GRUPO
-- Execute este script uma única vez no banco de dados Firebird

ALTER TABLE GRUPO ADD PERCENTUAL NUMERIC(5,2) DEFAULT 0;

UPDATE GRUPO SET PERCENTUAL = 0 WHERE PERCENTUAL IS NULL;
