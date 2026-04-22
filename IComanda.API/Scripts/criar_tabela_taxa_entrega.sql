/*
 * Script para criar a tabela de taxas de entrega (iComanda).
 * Execute este script no Firebird se a tabela ainda não existir.
 * O sistema também pode criar a tabela automaticamente na primeira utilização.
 */

-- Tabela TAXA_ENTREGA: ID, DESCRICAO, VALOR
CREATE TABLE TAXA_ENTREGA (
    ID INTEGER NOT NULL PRIMARY KEY,
    DESCRICAO VARCHAR(100) NOT NULL,
    VALOR NUMERIC(15,2) NOT NULL
);

-- Generator para auto-incremento do ID
CREATE GENERATOR GEN_TAXA_ENTREGA_ID;

-- Trigger para preencher ID antes do INSERT
SET TERM ^ ;
CREATE TRIGGER TR_TAXA_ENTREGA_BI FOR TAXA_ENTREGA
ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    IF (NEW.ID IS NULL) THEN
        NEW.ID = GEN_ID(GEN_TAXA_ENTREGA_ID, 1);
END^
SET TERM ; ^

COMMIT;
