/* ================================================================
   SCRIPT DE CRIAÇÃO DAS TABELAS — MÓDULO FORÇA DE VENDAS
   iComanda — 2026
   
   Execute este script NO BANCO DE DADOS FIREBIRD do sistema.
   Ferramentas compatíveis:
     - FlameRobin
     - IBExpert
     - DBeaver (driver Firebird)
     - isql (linha de comando Firebird)
   
   IMPORTANTE: executar em ordem (generators antes das tabelas).
   ================================================================ */

/* ----------------------------------------------------------------
   1. ATUALIZAR TABELA VENDEDORES (se necessário)
   Verificar se as colunas abaixo já existem antes de executar.
   Se a tabela/coluna já existir, o ALTER será ignorado.
   ---------------------------------------------------------------- */

-- Adicionar colunas extras na tabela VENDEDORES (se não existirem)
-- Execute cada ALTER TABLE individualmente e ignore erros de "coluna já existe"

ALTER TABLE VENDEDORES ADD EMAIL    VARCHAR(120);
ALTER TABLE VENDEDORES ADD CELULAR  VARCHAR(20);
ALTER TABLE VENDEDORES ADD COMISSAO DECIMAL(5,2) DEFAULT 0;
ALTER TABLE VENDEDORES ADD META     DECIMAL(15,2) DEFAULT 0;
ALTER TABLE VENDEDORES ADD REGIAO   VARCHAR(80);
ALTER TABLE VENDEDORES ADD OBS      VARCHAR(500);
ALTER TABLE VENDEDORES ADD DATACADASTRO DATE DEFAULT CURRENT_DATE;


/* ----------------------------------------------------------------
   2. GENERATOR (SEQUENCE) para ID auto-incremento
   ---------------------------------------------------------------- */

CREATE GENERATOR GEN_PEDIDOS_FV_ID;
SET GENERATOR GEN_PEDIDOS_FV_ID TO 0;

CREATE GENERATOR GEN_ITENS_PEDIDO_FV_ID;
SET GENERATOR GEN_ITENS_PEDIDO_FV_ID TO 0;

CREATE GENERATOR GEN_VISITAS_FV_ID;
SET GENERATOR GEN_VISITAS_FV_ID TO 0;

CREATE GENERATOR GEN_METAS_FV_ID;
SET GENERATOR GEN_METAS_FV_ID TO 0;


/* ----------------------------------------------------------------
   3. TABELA PEDIDOS_FV
   Status: 0=Pendente | 1=Aprovado | 2=Faturado | 3=Cancelado
   ---------------------------------------------------------------- */

CREATE TABLE PEDIDOS_FV (
    ID              INTEGER       NOT NULL,
    ID_VENDEDOR     INTEGER       NOT NULL,
    ID_CLIENTE      INTEGER       NOT NULL,
    DATA_PEDIDO     DATE          NOT NULL DEFAULT CURRENT_DATE,
    HORA_PEDIDO     TIME          NOT NULL DEFAULT CURRENT_TIME,
    STATUS          SMALLINT      NOT NULL DEFAULT 0,
    SUBTOTAL        DECIMAL(15,2) NOT NULL DEFAULT 0,
    DESCONTO        DECIMAL(15,2) NOT NULL DEFAULT 0,
    ACRESCIMO       DECIMAL(15,2) NOT NULL DEFAULT 0,
    TOTAL           DECIMAL(15,2) NOT NULL DEFAULT 0,
    OBS             VARCHAR(500),
    CONDICAO_PGTO   VARCHAR(60),
    TABELA_PRECO    SMALLINT      NOT NULL DEFAULT 1,
    DATA_APROVACAO  TIMESTAMP,
    ID_APROVADOR    INTEGER,
    MOTIVO_CANCEL   VARCHAR(300),
    NOTA_FISCAL     VARCHAR(20),
    DATA_FATURAMENTO TIMESTAMP,
    CONSTRAINT PK_PEDIDOS_FV PRIMARY KEY (ID),
    CONSTRAINT FK_PFV_VENDEDOR FOREIGN KEY (ID_VENDEDOR) REFERENCES VENDEDORES (ID),
    CONSTRAINT FK_PFV_CLIENTE  FOREIGN KEY (ID_CLIENTE)  REFERENCES CLIENTES (ID),
    CONSTRAINT CK_PFV_STATUS   CHECK (STATUS BETWEEN 0 AND 3)
);

/* Trigger de auto-incremento */
SET TERM ^ ;
CREATE OR ALTER TRIGGER TRG_PEDIDOS_FV_BI
BEFORE INSERT ON PEDIDOS_FV
AS BEGIN
    IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
        NEW.ID = GEN_ID(GEN_PEDIDOS_FV_ID, 1);
END ^
SET TERM ; ^

/* Índices */
CREATE INDEX IDX_PFV_VENDEDOR ON PEDIDOS_FV (ID_VENDEDOR);
CREATE INDEX IDX_PFV_CLIENTE  ON PEDIDOS_FV (ID_CLIENTE);
CREATE INDEX IDX_PFV_STATUS   ON PEDIDOS_FV (STATUS);
CREATE INDEX IDX_PFV_DATA     ON PEDIDOS_FV (DATA_PEDIDO);


/* ----------------------------------------------------------------
   4. TABELA ITENS_PEDIDO_FV
   ---------------------------------------------------------------- */

CREATE TABLE ITENS_PEDIDO_FV (
    ID              INTEGER        NOT NULL,
    ID_PEDIDO_FV    INTEGER        NOT NULL,
    ID_PRODUTO      INTEGER        NOT NULL,
    CODIGO          VARCHAR(30),
    DESCRICAO       VARCHAR(200)   NOT NULL,
    QUANTIDADE      DECIMAL(10,3)  NOT NULL DEFAULT 0,
    UNIDADE         VARCHAR(6)     NOT NULL DEFAULT 'UN',
    PRECO_UNIT      DECIMAL(15,4)  NOT NULL DEFAULT 0,
    DESCONTO        DECIMAL(5,2)   NOT NULL DEFAULT 0,
    TOTAL           DECIMAL(15,2)  NOT NULL DEFAULT 0,
    OBS             VARCHAR(300),
    CONSTRAINT PK_ITENS_PEDIDO_FV PRIMARY KEY (ID),
    CONSTRAINT FK_IPFV_PEDIDO  FOREIGN KEY (ID_PEDIDO_FV) REFERENCES PEDIDOS_FV (ID) ON DELETE CASCADE
);

SET TERM ^ ;
CREATE OR ALTER TRIGGER TRG_ITENS_PEDIDO_FV_BI
BEFORE INSERT ON ITENS_PEDIDO_FV
AS BEGIN
    IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
        NEW.ID = GEN_ID(GEN_ITENS_PEDIDO_FV_ID, 1);
END ^
SET TERM ; ^

CREATE INDEX IDX_IPFV_PEDIDO   ON ITENS_PEDIDO_FV (ID_PEDIDO_FV);
CREATE INDEX IDX_IPFV_PRODUTO  ON ITENS_PEDIDO_FV (ID_PRODUTO);


/* ----------------------------------------------------------------
   5. TABELA VISITAS_FV
   Status: 0=Agendada | 1=EmAndamento | 2=Concluida | 3=NaoRealizada
   ---------------------------------------------------------------- */

CREATE TABLE VISITAS_FV (
    ID              INTEGER        NOT NULL,
    ID_VENDEDOR     INTEGER        NOT NULL,
    ID_CLIENTE      INTEGER        NOT NULL,
    DATA_AGENDADA   TIMESTAMP      NOT NULL,
    DATA_CHECKIN    TIMESTAMP,
    DATA_CHECKOUT   TIMESTAMP,
    LAT_CHECKIN     DECIMAL(10,7),
    LNG_CHECKIN     DECIMAL(10,7),
    LAT_CHECKOUT    DECIMAL(10,7),
    LNG_CHECKOUT    DECIMAL(10,7),
    STATUS          SMALLINT       NOT NULL DEFAULT 0,
    OBS             VARCHAR(500),
    RESULTADO       VARCHAR(60),
    ID_PEDIDO_FV    INTEGER,
    CONSTRAINT PK_VISITAS_FV PRIMARY KEY (ID),
    CONSTRAINT FK_VFV_VENDEDOR FOREIGN KEY (ID_VENDEDOR) REFERENCES VENDEDORES (ID),
    CONSTRAINT FK_VFV_CLIENTE  FOREIGN KEY (ID_CLIENTE)  REFERENCES CLIENTES (ID),
    CONSTRAINT CK_VFV_STATUS   CHECK (STATUS BETWEEN 0 AND 3)
);

SET TERM ^ ;
CREATE OR ALTER TRIGGER TRG_VISITAS_FV_BI
BEFORE INSERT ON VISITAS_FV
AS BEGIN
    IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
        NEW.ID = GEN_ID(GEN_VISITAS_FV_ID, 1);
END ^
SET TERM ; ^

CREATE INDEX IDX_VFV_VENDEDOR ON VISITAS_FV (ID_VENDEDOR);
CREATE INDEX IDX_VFV_CLIENTE  ON VISITAS_FV (ID_CLIENTE);
CREATE INDEX IDX_VFV_DATA     ON VISITAS_FV (DATA_AGENDADA);
CREATE INDEX IDX_VFV_STATUS   ON VISITAS_FV (STATUS);


/* ----------------------------------------------------------------
   6. TABELA METAS_FV
   ---------------------------------------------------------------- */

CREATE TABLE METAS_FV (
    ID               INTEGER        NOT NULL,
    ID_VENDEDOR      INTEGER        NOT NULL,
    MES              SMALLINT       NOT NULL,
    ANO              SMALLINT       NOT NULL,
    VALOR_META       DECIMAL(15,2)  NOT NULL DEFAULT 0,
    VALOR_REALIZADO  DECIMAL(15,2)  NOT NULL DEFAULT 0,
    CONSTRAINT PK_METAS_FV     PRIMARY KEY (ID),
    CONSTRAINT FK_MFV_VENDEDOR FOREIGN KEY (ID_VENDEDOR) REFERENCES VENDEDORES (ID),
    CONSTRAINT UQ_METAS_FV     UNIQUE (ID_VENDEDOR, MES, ANO),
    CONSTRAINT CK_MFV_MES      CHECK (MES BETWEEN 1 AND 12),
    CONSTRAINT CK_MFV_ANO      CHECK (ANO BETWEEN 2020 AND 2100)
);

SET TERM ^ ;
CREATE OR ALTER TRIGGER TRG_METAS_FV_BI
BEFORE INSERT ON METAS_FV
AS BEGIN
    IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
        NEW.ID = GEN_ID(GEN_METAS_FV_ID, 1);
END ^
SET TERM ; ^

CREATE INDEX IDX_MFV_VENDEDOR ON METAS_FV (ID_VENDEDOR);
CREATE INDEX IDX_MFV_MES_ANO  ON METAS_FV (ANO, MES);


/* ----------------------------------------------------------------
   7. GRANT DE PERMISSÕES (ajuste conforme usuário do banco)
   Substitua SYSDBA pelo usuário da aplicação se diferente
   ---------------------------------------------------------------- */

GRANT ALL ON PEDIDOS_FV       TO SYSDBA WITH GRANT OPTION;
GRANT ALL ON ITENS_PEDIDO_FV  TO SYSDBA WITH GRANT OPTION;
GRANT ALL ON VISITAS_FV       TO SYSDBA WITH GRANT OPTION;
GRANT ALL ON METAS_FV         TO SYSDBA WITH GRANT OPTION;

COMMIT;

/* ================================================================
   FIM DO SCRIPT
   Tabelas criadas:
     - PEDIDOS_FV
     - ITENS_PEDIDO_FV
     - VISITAS_FV
     - METAS_FV
   Colunas adicionadas a VENDEDORES:
     - EMAIL, CELULAR, COMISSAO, META, REGIAO, OBS, DATACADASTRO
   ================================================================ */
