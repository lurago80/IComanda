/* ===================================================================
   SCRIPT DE MIGRAÇÃO PARA AUTENTICAÇÃO COMPLETA 
   - Aumenta campo SENHA para suportar BCrypt (60 caracteres)
   - Cria tabela REFRESH_TOKEN para persistência de tokens
   =================================================================== */

-- CONEXÃO: Execute usando:
-- isql -user SYSDBA -password masterkey C:\iComanda\Dados\DADOSG5.FDB -i MIGRAR_AUTENTICACAO_COMPLETA.sql

SET SQL DIALECT 3;
SET AUTODDL ON;

-- ===================================================================
-- ETAPA 1: BACKUP DOS DADOS ATUAIS
-- ===================================================================

-- Criar tabela temporária para backup de usuários
CREATE TABLE USUARIO_BACKUP (
    ID INTEGER,
    NOME VARCHAR(50),
    SENHA_ANTIGA VARCHAR(20),
    ATIVO VARCHAR(1),
    BLOQUEIO VARCHAR(1),
    VISUALIZAR VARCHAR(1),
    TOTAL VARCHAR(1),
    TIPO VARCHAR(1),
    CANCELAR VARCHAR(1),
    DATA_BACKUP TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Fazer backup dos usuários atuais
INSERT INTO USUARIO_BACKUP (ID, NOME, SENHA_ANTIGA, ATIVO, BLOQUEIO, VISUALIZAR, TOTAL, TIPO, CANCELAR)
SELECT ID, NOME, SENHA, ATIVO, BLOQUEIO, VISUALIZAR, TOTAL, TIPO, CANCELAR
FROM USUARIO;

COMMIT;

-- ===================================================================
-- ETAPA 2: ALTERAR CAMPO SENHA PARA SUPORTAR BCRYPT (60 CARACTERES)
-- ===================================================================

-- Nota: Firebird 2.5 não suporta ALTER COLUMN TYPE diretamente com dados
-- Solução: Criar nova coluna, copiar dados, dropar antiga, renomear nova

-- Adicionar nova coluna SENHA_NOVA com tamanho correto
ALTER TABLE USUARIO ADD SENHA_NOVA VARCHAR(60);

-- Copiar senhas existentes (plaintext)
UPDATE USUARIO SET SENHA_NOVA = SENHA;

COMMIT;

-- Dropar coluna antiga
ALTER TABLE USUARIO DROP SENHA;

COMMIT;

-- Renomear nova coluna para SENHA
ALTER TABLE USUARIO ALTER COLUMN SENHA_NOVA TO SENHA;

COMMIT;

-- ===================================================================
-- ETAPA 3: CRIAR TABELA REFRESH_TOKEN
-- ===================================================================

CREATE TABLE REFRESH_TOKEN (
    ID INTEGER NOT NULL,
    TOKEN VARCHAR(200) NOT NULL UNIQUE,
    USUARIO_ID INTEGER NOT NULL,
    USERNAME VARCHAR(50) NOT NULL,
    ROLE VARCHAR(20) NOT NULL,
    CRIADO_EM TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    EXPIRA_EM TIMESTAMP NOT NULL,
    REVOGADO CHAR(1) DEFAULT '0' NOT NULL,
    RAZAO_REVOGACAO VARCHAR(200),
    PRIMARY KEY (ID)
);

-- Criar generator para ID
CREATE GENERATOR GEN_REFRESH_TOKEN_ID;
SET GENERATOR GEN_REFRESH_TOKEN_ID TO 0;

-- Criar trigger para auto-incremento do ID
SET TERM ^ ;
CREATE TRIGGER TRG_REFRESH_TOKEN_BI FOR REFRESH_TOKEN
ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
  IF (NEW.ID IS NULL) THEN
    NEW.ID = GEN_ID(GEN_REFRESH_TOKEN_ID, 1);
END^
SET TERM ; ^

-- Criar índices para performance
CREATE INDEX IDX_REFRESH_TOKEN_TOKEN ON REFRESH_TOKEN(TOKEN);
CREATE INDEX IDX_REFRESH_TOKEN_USUARIO ON REFRESH_TOKEN(USUARIO_ID);
CREATE INDEX IDX_REFRESH_TOKEN_EXPIRA ON REFRESH_TOKEN(EXPIRA_EM);

COMMIT;

-- ===================================================================
-- ETAPA 4: VALIDAÇÃO
-- ===================================================================

-- Exibir tamanho do campo SENHA (deve ser 60)
SELECT 
    'USUARIO' AS TABELA,
    RDB$FIELD_NAME AS CAMPO,
    F.RDB$FIELD_LENGTH AS TAMANHO,
    F.RDB$CHARACTER_LENGTH AS TAMANHO_CARACTERES
FROM RDB$RELATION_FIELDS RF
JOIN RDB$FIELDS F ON RF.RDB$FIELD_SOURCE = F.RDB$FIELD_NAME
WHERE RF.RDB$RELATION_NAME = 'USUARIO' 
  AND RF.RDB$FIELD_NAME = 'SENHA';

-- Exibir estrutura da tabela REFRESH_TOKEN
SELECT 
    RDB$FIELD_NAME AS CAMPO,
    F.RDB$FIELD_LENGTH AS TAMANHO,
    CASE 
        WHEN RDB$NULL_FLAG = 1 THEN 'NÃO'
        ELSE 'SIM'
    END AS PERMITE_NULL
FROM RDB$RELATION_FIELDS RF
JOIN RDB$FIELDS F ON RF.RDB$FIELD_SOURCE = F.RDB$FIELD_NAME
WHERE RF.RDB$RELATION_NAME = 'REFRESH_TOKEN'
ORDER BY RDB$FIELD_POSITION;

-- Contar registros preservados
SELECT 
    (SELECT COUNT(*) FROM USUARIO) AS USUARIOS_ATUAIS,
    (SELECT COUNT(*) FROM USUARIO_BACKUP) AS USUARIOS_BACKUP;

COMMIT;

-- ===================================================================
-- ETAPA 5: INSTRUÇÕES PARA ATIVAR AUTENTICAÇÃO COMPLETA NO C#
-- ===================================================================

/* 
APÓS EXECUTAR ESTE SCRIPT COM SUCESSO:

1. No arquivo: IComanda.API/Extensions/ServiceCollectionExtensions.cs

   TROCAR:
   services.AddSingleton<IRefreshTokenService, RefreshTokenService>();

   POR:
   services.AddScoped<IRefreshTokenService, RefreshTokenDatabaseService>();

2. No arquivo: IComanda.API/Controllers/AuthController.cs

   REMOVER o bloco de código:
   // MIGRAÇÃO DE SENHA DESABILITADA TEMPORARIAMENTE
   if (precisaMigrarSenha)
   {
       _logger.LogWarning(...);
   }

   E ADICIONAR:
   // Migrar senha para hash se necessário
   if (precisaMigrarSenha)
   {
       try
       {
           var hashedPassword = _passwordHasher.HashPassword(request.Password);
           await _usuarioRepository.AtualizarSenhaAsync(usuario.Id, hashedPassword);
           _logger.LogInformation($"✅ Senha do usuário {usuario.Id} migrada para BCrypt hash");
       }
       catch (Exception ex)
       {
           _logger.LogError(ex, $"❌ Erro ao migrar senha do usuário {usuario.Id}");
       }
   }

3. Recompilar: dotnet build
4. Reiniciar backend: dotnet run

BENEFÍCIOS:
✅ Senhas criptografadas com BCrypt (segurança)
✅ Refresh tokens persistentes (sobrevivem a reinicializações)
✅ Migração automática de senhas no primeiro login
*/

-- ===================================================================
-- FIM DO SCRIPT
-- ===================================================================
