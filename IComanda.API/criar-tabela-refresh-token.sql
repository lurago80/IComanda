/*
 * Script de criação da tabela REFRESH_TOKEN
 * 
 * ATENÇÃO: Execute este script no banco de dados Firebird
 * ANTES de ativar o RefreshTokenRepository
 * 
 * Firebird SQL DDL Script
 */

-- Criar tabela REFRESH_TOKEN
CREATE TABLE REFRESH_TOKEN (
    ID              INTEGER NOT NULL,
    TOKEN           VARCHAR(100) NOT NULL,
    USUARIO_ID      INTEGER NOT NULL,
    USUARIO_NOME    VARCHAR(100),
    USUARIO_ROLE    VARCHAR(50),
    DATA_CRIACAO    TIMESTAMP NOT NULL,
    DATA_EXPIRACAO  TIMESTAMP NOT NULL,
    REVOGADO        CHAR(1) DEFAULT '0',
    MOTIVO_REVOGACAO VARCHAR(200),
    DATA_REVOGACAO  TIMESTAMP,
    
    CONSTRAINT PK_REFRESH_TOKEN PRIMARY KEY (ID),
    CONSTRAINT UQ_REFRESH_TOKEN_TOKEN UNIQUE (TOKEN)
);

-- Criar índices para performance
CREATE INDEX IDX_REFRESH_TOKEN_USUARIO ON REFRESH_TOKEN (USUARIO_ID);
CREATE INDEX IDX_REFRESH_TOKEN_TOKEN ON REFRESH_TOKEN (TOKEN);
CREATE INDEX IDX_REFRESH_TOKEN_EXPIRACAO ON REFRESH_TOKEN (DATA_EXPIRACAO);
CREATE INDEX IDX_REFRESH_TOKEN_REVOGADO ON REFRESH_TOKEN (REVOGADO);

-- Criar sequence/generator para IDs
CREATE SEQUENCE GEN_REFRESH_TOKEN_ID;

-- Criar trigger para auto-incremento
SET TERM ^ ;

CREATE TRIGGER TRG_REFRESH_TOKEN_BI FOR REFRESH_TOKEN
ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    IF (NEW.ID IS NULL) THEN
        NEW.ID = GEN_ID(GEN_REFRESH_TOKEN_ID, 1);
END^

SET TERM ; ^

COMMIT;

/*
 * TESTES DE INSERÇÃO (opcional - para validar)
 */

-- Inserir teste
INSERT INTO REFRESH_TOKEN (
    TOKEN,
    USUARIO_ID,
    USUARIO_NOME,
    USUARIO_ROLE,
    DATA_CRIACAO,
    DATA_EXPIRACAO,
    REVOGADO
) VALUES (
    'test_token_12345',
    1,
    'admin',
    'Admin',
    CURRENT_TIMESTAMP,
    DATEADD(7 DAY TO CURRENT_TIMESTAMP),
    '0'
);

-- Consultar
SELECT * FROM REFRESH_TOKEN;

-- Limpar teste
DELETE FROM REFRESH_TOKEN WHERE TOKEN = 'test_token_12345';

COMMIT;

/*
 * INSTRUÇÕES DE USO:
 * 
 * 1. Conecte ao banco de dados Firebird:
 *    isql -user SYSDBA -password masterkey "C:\iComanda\Dados\DADOSG5.FDB"
 * 
 * 2. Execute este script:
 *    INPUT "caminho\para\criar-tabela-refresh-token.sql";
 * 
 * 3. Verifique se a tabela foi criada:
 *    SHOW TABLES;
 *    SHOW TABLE REFRESH_TOKEN;
 * 
 * 4. Teste inserção/consulta (opcional)
 * 
 * 5. Ative o RefreshTokenRepository na aplicação
 */
