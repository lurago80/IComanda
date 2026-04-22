using Dapper;
using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Verifica e cria automaticamente colunas/registros necessários no banco Firebird
/// durante a inicialização do sistema. Garante compatibilidade com bancos legados.
/// </summary>
public class IComandaDbMigrationService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<IComandaDbMigrationService> _logger;

    public IComandaDbMigrationService(
        IDbConnectionFactory connectionFactory,
        ILogger<IComandaDbMigrationService> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>Ponto de entrada: verifica/aplica todas as migrações pendentes.</summary>
    public async Task EnsureMigrationsAsync()
    {
        _logger.LogInformation("🔍 [Migration] Verificando estrutura do banco de dados...");
        Console.WriteLine("========================================");
        Console.WriteLine("🔍 MIGRAÇÕES — verificando banco de dados...");

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        if (connection is not FbConnection fbConn)
        {
            _logger.LogWarning("[Migration] Conexão não é FbConnection — verificação ignorada.");
            return;
        }

        // ── 1. Coluna GRUPO.IMPRIMIR2VIAS ────────────────────────────────
        await EnsureGrupoImprimir2ViasAsync(fbConn);

        // ── 2. Parâmetro HABILITAR_IMPRIMIR_2VIAS na tabela PARAMETROS ───
        await EnsureParametroHabilitarImprimir2ViasAsync(fbConn);

        Console.WriteLine("✅ MIGRAÇÕES — verificação concluída.");
        Console.WriteLine("========================================");
        _logger.LogInformation("✅ [Migration] Verificação de estrutura concluída.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────

    private static async Task<bool> ColumnExistsAsync(FbConnection conn, string tableName, string columnName)
    {
        var count = await conn.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM RDB$RELATION_FIELDS
              WHERE TRIM(RDB$RELATION_NAME) = @Table
                AND TRIM(RDB$FIELD_NAME) = @Column",
            new { Table = tableName.ToUpper(), Column = columnName.ToUpper() });
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
    // 1. GRUPO.IMPRIMIR2VIAS
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureGrupoImprimir2ViasAsync(FbConnection conn)
    {
        if (await ColumnExistsAsync(conn, "GRUPO", "IMPRIMIR2VIAS"))
        {
            Console.WriteLine("  ✔ GRUPO.IMPRIMIR2VIAS já existe.");
            return;
        }

        _logger.LogInformation("[Migration] Adicionando coluna GRUPO.IMPRIMIR2VIAS...");
        ExecDDL(conn,
            "ALTER TABLE GRUPO ADD IMPRIMIR2VIAS SMALLINT DEFAULT 0",
            "GRUPO.IMPRIMIR2VIAS criada (SMALLINT DEFAULT 0)");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2. PARAMETROS — registro HABILITAR_IMPRIMIR_2VIAS
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureParametroHabilitarImprimir2ViasAsync(FbConnection conn)
    {
        var exists = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM PARAMETROS WHERE PARAMETRO = 'HABILITAR_IMPRIMIR_2VIAS'");

        if (exists > 0)
        {
            Console.WriteLine("  ✔ PARAMETROS.HABILITAR_IMPRIMIR_2VIAS já existe.");
            return;
        }

        _logger.LogInformation("[Migration] Inserindo parâmetro HABILITAR_IMPRIMIR_2VIAS...");
        try
        {
            // Herda ID_EMITENTE de registro existente (mesmo padrão usado em ConfiguracoesRepository)
            await conn.ExecuteAsync(
                @"INSERT INTO PARAMETROS (ID_EMITENTE, PARAMETRO, VALOR, TIPO)
                  SELECT FIRST 1 ID_EMITENTE, 'HABILITAR_IMPRIMIR_2VIAS', '0', 'BOOLEAN' FROM PARAMETROS");

            _logger.LogInformation("  ✅ PARAMETROS.HABILITAR_IMPRIMIR_2VIAS inserido com valor padrão '0'.");
            Console.WriteLine("  ✅ PARAMETROS.HABILITAR_IMPRIMIR_2VIAS inserido (padrão: desabilitado).");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("  ⚠️ PARAMETROS.HABILITAR_IMPRIMIR_2VIAS: {Error}", ex.Message);
            Console.WriteLine($"  ⚠️ PARAMETROS.HABILITAR_IMPRIMIR_2VIAS: {ex.Message}");
        }
    }
}
