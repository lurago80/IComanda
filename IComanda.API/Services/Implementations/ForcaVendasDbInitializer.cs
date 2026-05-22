using Dapper;
using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Inicializador de banco de dados para o módulo Força de Vendas.
/// Verifica se as tabelas/colunas necessárias existem no Firebird
/// e as cria automaticamente se não existirem.
/// </summary>
public class ForcaVendasDbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ForcaVendasDbInitializer> _logger;

    public ForcaVendasDbInitializer(
        IDbConnectionFactory connectionFactory,
        ILogger<ForcaVendasDbInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>Ponto de entrada: verifica/cria todas as estruturas do módulo FV.</summary>
    public async Task EnsureTablesAsync()
    {
        _logger.LogInformation("🔍 [FV] Verificando estrutura de banco — módulo Força de Vendas...");
        Console.WriteLine("========================================");
        Console.WriteLine("🔍 FORÇA DE VENDAS — verificando tabelas...");

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        if (connection is not FbConnection fbConn)
        {
            _logger.LogWarning("[FV] Conexão não é FbConnection — verificação ignorada.");
            return;
        }

        // ── 1. Colunas extras em VENDEDORES ─────────────────────────────
        await EnsureVendedoresColumnsAsync(fbConn);

        // ── 2. Tabela PEDIDOS_FV ─────────────────────────────────────────
        await EnsureTablePedidosFVAsync(fbConn);

        // ── 3. Tabela ITENS_PEDIDO_FV ────────────────────────────────────
        await EnsureTableItensPedidoFVAsync(fbConn);

        // ── 4. Tabela VISITAS_FV ─────────────────────────────────────────
        await EnsureTableVisitasFVAsync(fbConn);

        // ── 5. Tabela METAS_FV ───────────────────────────────────────────
        await EnsureTableMetasFVAsync(fbConn);

        // ── 6. Colunas faltando em tabelas já existentes ─────────────────
        await EnsureExistingTablesColumnsAsync(fbConn);

        Console.WriteLine("✅ FORÇA DE VENDAS — verificação concluída.");
        Console.WriteLine("========================================");
        _logger.LogInformation("✅ [FV] Verificação de estrutura concluída.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────

    private static async Task<bool> TableExistsAsync(FbConnection conn, string tableName)
    {
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM RDB$RELATIONS WHERE TRIM(RDB$RELATION_NAME) = @Name",
            new { Name = tableName.ToUpper() });
        return count > 0;
    }

    private static async Task<bool> ColumnExistsAsync(FbConnection conn, string tableName, string columnName)
    {
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM RDB$RELATION_FIELDS
              WHERE TRIM(RDB$RELATION_NAME) = @Table
                AND TRIM(RDB$FIELD_NAME) = @Column",
            new { Table = tableName.ToUpper(), Column = columnName.ToUpper() });
        return count > 0;
    }

    private static async Task<bool> GeneratorExistsAsync(FbConnection conn, string generatorName)
    {
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM RDB$GENERATORS WHERE TRIM(RDB$GENERATOR_NAME) = @Name",
            new { Name = generatorName.ToUpper() });
        return count > 0;
    }

    private static async Task<bool> IndexExistsAsync(FbConnection conn, string indexName)
    {
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM RDB$INDICES WHERE TRIM(RDB$INDEX_NAME) = @Name",
            new { Name = indexName.ToUpper() });
        return count > 0;
    }

    private void ExecDDL(FbConnection conn, string ddl, string description)
    {
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = ddl;
            cmd.ExecuteNonQuery();
            _logger.LogInformation("  ✅ {Description}", description);
            Console.WriteLine($"  ✅ {description}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("  ⚠️ {Description}: {Error}", description, ex.Message);
            Console.WriteLine($"  ⚠️ {description}: {ex.Message}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 1. VENDEDOR — colunas adicionais (se não existirem)
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureVendedoresColumnsAsync(FbConnection conn)
    {
        // COMISSAO já existe na tabela VENDEDOR original — adicionamos apenas as colunas extras
        var extraCols = new Dictionary<string, string>
        {
            ["EMAIL"]       = "ALTER TABLE VENDEDOR ADD EMAIL VARCHAR(120)",
            ["CELULAR"]     = "ALTER TABLE VENDEDOR ADD CELULAR VARCHAR(20)",
            ["META"]        = "ALTER TABLE VENDEDOR ADD META DECIMAL(15,2) DEFAULT 0",
            ["REGIAO"]      = "ALTER TABLE VENDEDOR ADD REGIAO VARCHAR(80)",
            ["OBS"]         = "ALTER TABLE VENDEDOR ADD OBS VARCHAR(500)",
            ["DATACADASTRO"]= "ALTER TABLE VENDEDOR ADD DATACADASTRO DATE DEFAULT CURRENT_DATE",
        };

        foreach (var (col, ddl) in extraCols)
        {
            if (!await ColumnExistsAsync(conn, "VENDEDOR", col))
            {
                ExecDDL(conn, ddl, $"VENDEDOR.{col} criada");
            }
        }

        // Garantir que SENHA suporta hash BCrypt (60 chars) — expande se necessário
        ExecDDL(conn, "ALTER TABLE VENDEDOR ALTER COLUMN SENHA TYPE VARCHAR(72)",
            "VENDEDOR.SENHA expandida para VARCHAR(72) (suporte a BCrypt)");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2. PEDIDOS_FV
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureTablePedidosFVAsync(FbConnection conn)
    {
        if (await TableExistsAsync(conn, "PEDIDOS_FV"))
        {
            Console.WriteLine("  ✔ PEDIDOS_FV já existe.");
            return;
        }

        _logger.LogInformation("[FV] Criando tabela PEDIDOS_FV...");
        Console.WriteLine("  🔨 Criando PEDIDOS_FV...");

        // Generator
        if (!await GeneratorExistsAsync(conn, "GEN_PEDIDOS_FV_ID"))
            ExecDDL(conn, "CREATE GENERATOR GEN_PEDIDOS_FV_ID", "Generator GEN_PEDIDOS_FV_ID");

        ExecDDL(conn, @"
CREATE TABLE PEDIDOS_FV (
    ID               INTEGER        NOT NULL,
    ID_VENDEDOR      INTEGER        NOT NULL,
    ID_CLIENTE       INTEGER        NOT NULL,
    DATA_PEDIDO      DATE           DEFAULT CURRENT_DATE NOT NULL,
    HORA_PEDIDO      TIME           DEFAULT CURRENT_TIME NOT NULL,
    STATUS           SMALLINT       DEFAULT 0 NOT NULL,
    SUBTOTAL         DECIMAL(15,2)  DEFAULT 0 NOT NULL,
    DESCONTO         DECIMAL(15,2)  DEFAULT 0 NOT NULL,
    ACRESCIMO        DECIMAL(15,2)  DEFAULT 0 NOT NULL,
    TOTAL            DECIMAL(15,2)  DEFAULT 0 NOT NULL,
    OBS              VARCHAR(500),
    CONDICAO_PGTO    VARCHAR(60),
    TABELA_PRECO     SMALLINT       DEFAULT 1 NOT NULL,
    DATA_APROVACAO   TIMESTAMP,
    ID_APROVADOR     INTEGER,
    MOTIVO_CANCEL    VARCHAR(300),
    NOTA_FISCAL      VARCHAR(20),
    DATA_FATURAMENTO TIMESTAMP,
    CONSTRAINT PK_PEDIDOS_FV PRIMARY KEY (ID),
    CONSTRAINT FK_PFV_VENDEDOR FOREIGN KEY (ID_VENDEDOR) REFERENCES VENDEDOR (ID),
    CONSTRAINT FK_PFV_CLIENTE  FOREIGN KEY (ID_CLIENTE)  REFERENCES CLIENTES (ID),
    CONSTRAINT CK_PFV_STATUS   CHECK (STATUS BETWEEN 0 AND 3)
)", "Tabela PEDIDOS_FV");

        ExecDDL(conn, @"
CREATE TRIGGER TRG_PEDIDOS_FV_BI FOR PEDIDOS_FV
ACTIVE BEFORE INSERT POSITION 0
AS BEGIN
  IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
    NEW.ID = GEN_ID(GEN_PEDIDOS_FV_ID, 1);
END", "Trigger TRG_PEDIDOS_FV_BI");

        if (!await IndexExistsAsync(conn, "IDX_PFV_VENDEDOR"))
            ExecDDL(conn, "CREATE INDEX IDX_PFV_VENDEDOR ON PEDIDOS_FV (ID_VENDEDOR)", "Index IDX_PFV_VENDEDOR");
        if (!await IndexExistsAsync(conn, "IDX_PFV_CLIENTE"))
            ExecDDL(conn, "CREATE INDEX IDX_PFV_CLIENTE ON PEDIDOS_FV (ID_CLIENTE)", "Index IDX_PFV_CLIENTE");
        if (!await IndexExistsAsync(conn, "IDX_PFV_STATUS"))
            ExecDDL(conn, "CREATE INDEX IDX_PFV_STATUS ON PEDIDOS_FV (STATUS)", "Index IDX_PFV_STATUS");
        if (!await IndexExistsAsync(conn, "IDX_PFV_DATA"))
            ExecDDL(conn, "CREATE INDEX IDX_PFV_DATA ON PEDIDOS_FV (DATA_PEDIDO)", "Index IDX_PFV_DATA");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 3. ITENS_PEDIDO_FV
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureTableItensPedidoFVAsync(FbConnection conn)
    {
        if (await TableExistsAsync(conn, "ITENS_PEDIDO_FV"))
        {
            Console.WriteLine("  ✔ ITENS_PEDIDO_FV já existe.");
            return;
        }

        _logger.LogInformation("[FV] Criando tabela ITENS_PEDIDO_FV...");
        Console.WriteLine("  🔨 Criando ITENS_PEDIDO_FV...");

        if (!await GeneratorExistsAsync(conn, "GEN_ITENS_PEDIDO_FV_ID"))
            ExecDDL(conn, "CREATE GENERATOR GEN_ITENS_PEDIDO_FV_ID", "Generator GEN_ITENS_PEDIDO_FV_ID");

        ExecDDL(conn, @"
CREATE TABLE ITENS_PEDIDO_FV (
    ID           INTEGER        NOT NULL,
    ID_PEDIDO_FV INTEGER        NOT NULL,
    ID_PRODUTO   INTEGER        NOT NULL,
    CODIGO       VARCHAR(30),
    DESCRICAO    VARCHAR(200)   NOT NULL,
    QUANTIDADE   DECIMAL(10,3)  DEFAULT 0 NOT NULL,
    UNIDADE      VARCHAR(6)     DEFAULT 'UN' NOT NULL,
    PRECO_UNIT   DECIMAL(15,4)  DEFAULT 0 NOT NULL,
    DESCONTO     DECIMAL(5,2)   DEFAULT 0 NOT NULL,
    TOTAL        DECIMAL(15,2)  DEFAULT 0 NOT NULL,
    OBS          VARCHAR(300),
    CONSTRAINT PK_ITENS_PEDIDO_FV PRIMARY KEY (ID),
    CONSTRAINT FK_IPFV_PEDIDO FOREIGN KEY (ID_PEDIDO_FV) REFERENCES PEDIDOS_FV (ID) ON DELETE CASCADE
)", "Tabela ITENS_PEDIDO_FV");

        ExecDDL(conn, @"
CREATE TRIGGER TRG_ITENS_PEDIDO_FV_BI FOR ITENS_PEDIDO_FV
ACTIVE BEFORE INSERT POSITION 0
AS BEGIN
  IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
    NEW.ID = GEN_ID(GEN_ITENS_PEDIDO_FV_ID, 1);
END", "Trigger TRG_ITENS_PEDIDO_FV_BI");

        if (!await IndexExistsAsync(conn, "IDX_IPFV_PEDIDO"))
            ExecDDL(conn, "CREATE INDEX IDX_IPFV_PEDIDO ON ITENS_PEDIDO_FV (ID_PEDIDO_FV)", "Index IDX_IPFV_PEDIDO");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 4. VISITAS_FV
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureTableVisitasFVAsync(FbConnection conn)
    {
        if (await TableExistsAsync(conn, "VISITAS_FV"))
        {
            Console.WriteLine("  ✔ VISITAS_FV já existe.");
            return;
        }

        _logger.LogInformation("[FV] Criando tabela VISITAS_FV...");
        Console.WriteLine("  🔨 Criando VISITAS_FV...");

        if (!await GeneratorExistsAsync(conn, "GEN_VISITAS_FV_ID"))
            ExecDDL(conn, "CREATE GENERATOR GEN_VISITAS_FV_ID", "Generator GEN_VISITAS_FV_ID");

        ExecDDL(conn, @"
CREATE TABLE VISITAS_FV (
    ID                INTEGER        NOT NULL,
    ID_VENDEDOR       INTEGER        NOT NULL,
    ID_CLIENTE        INTEGER        NOT NULL,
    DATA_AGENDADA     DATE           NOT NULL,
    HORARIO_PREVISTO  TIME,
    STATUS            SMALLINT       DEFAULT 0 NOT NULL,
    OBJETIVO          VARCHAR(300),
    OBS               VARCHAR(500),
    DATA_CHECKIN      TIMESTAMP,
    LAT_CHECKIN       DECIMAL(10,6),
    LNG_CHECKIN       DECIMAL(10,6),
    DATA_CHECKOUT     TIMESTAMP,
    LAT_CHECKOUT      DECIMAL(10,6),
    LNG_CHECKOUT      DECIMAL(10,6),
    RESULTADO         VARCHAR(300),
    MOTIVO_CANCEL     VARCHAR(300),
    ID_PEDIDO_FV      INTEGER,
    CONSTRAINT PK_VISITAS_FV PRIMARY KEY (ID),
    CONSTRAINT FK_VFV_VENDEDOR FOREIGN KEY (ID_VENDEDOR) REFERENCES VENDEDOR (ID),
    CONSTRAINT FK_VFV_CLIENTE  FOREIGN KEY (ID_CLIENTE)  REFERENCES CLIENTES (ID),
    CONSTRAINT CK_VFV_STATUS   CHECK (STATUS BETWEEN 0 AND 3)
)", "Tabela VISITAS_FV");

        ExecDDL(conn, @"
CREATE TRIGGER TRG_VISITAS_FV_BI FOR VISITAS_FV
ACTIVE BEFORE INSERT POSITION 0
AS BEGIN
  IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
    NEW.ID = GEN_ID(GEN_VISITAS_FV_ID, 1);
END", "Trigger TRG_VISITAS_FV_BI");

        if (!await IndexExistsAsync(conn, "IDX_VFV_VENDEDOR"))
            ExecDDL(conn, "CREATE INDEX IDX_VFV_VENDEDOR ON VISITAS_FV (ID_VENDEDOR)", "Index IDX_VFV_VENDEDOR");
        if (!await IndexExistsAsync(conn, "IDX_VFV_DATA"))
            ExecDDL(conn, "CREATE INDEX IDX_VFV_DATA ON VISITAS_FV (DATA_AGENDADA)", "Index IDX_VFV_DATA");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 5. METAS_FV
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureTableMetasFVAsync(FbConnection conn)
    {
        if (await TableExistsAsync(conn, "METAS_FV"))
        {
            Console.WriteLine("  ✔ METAS_FV já existe.");
            return;
        }

        _logger.LogInformation("[FV] Criando tabela METAS_FV...");
        Console.WriteLine("  🔨 Criando METAS_FV...");

        if (!await GeneratorExistsAsync(conn, "GEN_METAS_FV_ID"))
            ExecDDL(conn, "CREATE GENERATOR GEN_METAS_FV_ID", "Generator GEN_METAS_FV_ID");

        ExecDDL(conn, @"
CREATE TABLE METAS_FV (
    ID               INTEGER        NOT NULL,
    ID_VENDEDOR      INTEGER        NOT NULL,
    MES              SMALLINT       NOT NULL,
    ANO              SMALLINT       NOT NULL,
    VALOR_META       DECIMAL(15,2)  DEFAULT 0 NOT NULL,
    VALOR_REALIZADO  DECIMAL(15,2)  DEFAULT 0 NOT NULL,
    COMISSAO         DECIMAL(5,2)   DEFAULT 0 NOT NULL,
    CONSTRAINT PK_METAS_FV PRIMARY KEY (ID),
    CONSTRAINT FK_MFV_VENDEDOR FOREIGN KEY (ID_VENDEDOR) REFERENCES VENDEDOR (ID),
    CONSTRAINT UQ_METAS_FV UNIQUE (ID_VENDEDOR, MES, ANO)
)", "Tabela METAS_FV");

        ExecDDL(conn, @"
CREATE TRIGGER TRG_METAS_FV_BI FOR METAS_FV
ACTIVE BEFORE INSERT POSITION 0
AS BEGIN
  IF (NEW.ID IS NULL OR NEW.ID = 0) THEN
    NEW.ID = GEN_ID(GEN_METAS_FV_ID, 1);
END", "Trigger TRG_METAS_FV_BI");

        if (!await IndexExistsAsync(conn, "IDX_MFV_VENDEDOR"))
            ExecDDL(conn, "CREATE INDEX IDX_MFV_VENDEDOR ON METAS_FV (ID_VENDEDOR)", "Index IDX_MFV_VENDEDOR");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 6. Colunas faltando em tabelas que já existem (ALTER TABLE)
    //    Necessário quando o banco foi criado por uma versão anterior do código.
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureExistingTablesColumnsAsync(FbConnection conn)
    {
        // METAS_FV.VALOR_REALIZADO — era ausente no CREATE TABLE original
        if (await TableExistsAsync(conn, "METAS_FV") &&
            !await ColumnExistsAsync(conn, "METAS_FV", "VALOR_REALIZADO"))
        {
            ExecDDL(conn,
                "ALTER TABLE METAS_FV ADD VALOR_REALIZADO DECIMAL(15,2) DEFAULT 0 NOT NULL",
                "METAS_FV.VALOR_REALIZADO adicionada");
        }
        else
        {
            Console.WriteLine("  ✔ METAS_FV.VALOR_REALIZADO já existe.");
        }

        // VISITAS_FV.RESULTADO — era ausente no CREATE TABLE original (havia RESUMO no lugar)
        if (await TableExistsAsync(conn, "VISITAS_FV") &&
            !await ColumnExistsAsync(conn, "VISITAS_FV", "RESULTADO"))
        {
            ExecDDL(conn,
                "ALTER TABLE VISITAS_FV ADD RESULTADO VARCHAR(300)",
                "VISITAS_FV.RESULTADO adicionada");
        }
        else
        {
            Console.WriteLine("  ✔ VISITAS_FV.RESULTADO já existe.");
        }

        // PEDIDOS_FV — garantir colunas que podem ter sido criadas sem elas
        if (await TableExistsAsync(conn, "PEDIDOS_FV"))
        {
            var pedidosCols = new Dictionary<string, string>
            {
                ["ACRESCIMO"]        = "ALTER TABLE PEDIDOS_FV ADD ACRESCIMO DECIMAL(15,2) DEFAULT 0 NOT NULL",
                ["CONDICAO_PGTO"]    = "ALTER TABLE PEDIDOS_FV ADD CONDICAO_PGTO VARCHAR(60)",
                ["DATA_APROVACAO"]   = "ALTER TABLE PEDIDOS_FV ADD DATA_APROVACAO TIMESTAMP",
                ["ID_APROVADOR"]     = "ALTER TABLE PEDIDOS_FV ADD ID_APROVADOR INTEGER",
                ["MOTIVO_CANCEL"]    = "ALTER TABLE PEDIDOS_FV ADD MOTIVO_CANCEL VARCHAR(300)",
                ["NOTA_FISCAL"]      = "ALTER TABLE PEDIDOS_FV ADD NOTA_FISCAL VARCHAR(20)",
                ["DATA_FATURAMENTO"] = "ALTER TABLE PEDIDOS_FV ADD DATA_FATURAMENTO TIMESTAMP",
            };
            foreach (var (col, ddl) in pedidosCols)
            {
                if (!await ColumnExistsAsync(conn, "PEDIDOS_FV", col))
                    ExecDDL(conn, ddl, $"PEDIDOS_FV.{col} adicionada");
            }
        }

        // ITENS_PEDIDO_FV — garantir coluna OBS
        if (await TableExistsAsync(conn, "ITENS_PEDIDO_FV") &&
            !await ColumnExistsAsync(conn, "ITENS_PEDIDO_FV", "OBS"))
        {
            ExecDDL(conn, "ALTER TABLE ITENS_PEDIDO_FV ADD OBS VARCHAR(300)", "ITENS_PEDIDO_FV.OBS adicionada");
        }
    }
}
