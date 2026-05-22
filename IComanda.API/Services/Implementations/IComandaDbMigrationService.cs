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

        // ── 3. USUARIO.SENHA — tamanho mínimo para BCrypt (60 chars) ─────
        await EnsureUsuarioSenhaTypeAsync(fbConn);

        // ── 4. METAS_FV.VALOR_REALIZADO — coluna ausente em versões anteriores ──
        await EnsureMetasFvValorRealizadoAsync(fbConn);

        // ── 5. VISITAS_FV.RESULTADO — coluna ausente em versões anteriores ──
        await EnsureVisitasFvResultadoAsync(fbConn);

        // ── 6. GRUPO.TIPO — necessário para cardápio/pizza ────────────────
        await EnsureGrupoTipoAsync(fbConn);

        // ── 7. USUARIO LOJA — garantir permissões de Gerente ─────────────
        await EnsureLojaUserPermissoesAsync(fbConn);

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

    // ─────────────────────────────────────────────────────────────────────
    // 3. USUARIO.SENHA — VARCHAR(72) para suportar BCrypt
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureUsuarioSenhaTypeAsync(FbConnection conn)
    {
        // Obtém o tamanho atual do campo SENHA na tabela USUARIO
        var fieldLength = await conn.ExecuteScalarAsync<int?>(
            @"SELECT F.RDB$FIELD_LENGTH
              FROM RDB$RELATION_FIELDS RF
              JOIN RDB$FIELDS F ON F.RDB$FIELD_NAME = RF.RDB$FIELD_SOURCE
              WHERE TRIM(RF.RDB$RELATION_NAME) = 'USUARIO'
                AND TRIM(RF.RDB$FIELD_NAME)    = 'SENHA'");

        if (fieldLength == null)
        {
            Console.WriteLine("  ✔ USUARIO.SENHA: tabela/coluna não encontrada, ignorando.");
            return;
        }

        if (fieldLength >= 72)
        {
            Console.WriteLine($"  ✔ USUARIO.SENHA já suporta BCrypt ({fieldLength} chars).");
            return;
        }

        _logger.LogInformation("[Migration] Expandindo USUARIO.SENHA para VARCHAR(72)...");
        ExecDDL(conn,
            "ALTER TABLE USUARIO ALTER COLUMN SENHA TYPE VARCHAR(72)",
            $"USUARIO.SENHA expandida de {fieldLength} para VARCHAR(72)");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 4. METAS_FV.VALOR_REALIZADO — ausente em versões anteriores
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureMetasFvValorRealizadoAsync(FbConnection conn)
    {
        // Só executa se a tabela já existir (pode ter sido criada pela versão antiga)
        if (!await TableExistsAsync(conn, "METAS_FV"))
        {
            Console.WriteLine("  ✔ METAS_FV não existe ainda, nenhuma migração necessária.");
            return;
        }

        if (await ColumnExistsAsync(conn, "METAS_FV", "VALOR_REALIZADO"))
        {
            Console.WriteLine("  ✔ METAS_FV.VALOR_REALIZADO já existe.");
            return;
        }

        _logger.LogInformation("[Migration] Adicionando METAS_FV.VALOR_REALIZADO...");
        ExecDDL(conn,
            "ALTER TABLE METAS_FV ADD VALOR_REALIZADO DECIMAL(15,2) DEFAULT 0 NOT NULL",
            "METAS_FV.VALOR_REALIZADO criada (DECIMAL(15,2) DEFAULT 0)");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 5. VISITAS_FV.RESULTADO — ausente em versões anteriores
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureVisitasFvResultadoAsync(FbConnection conn)
    {
        // Só executa se a tabela já existir
        if (!await TableExistsAsync(conn, "VISITAS_FV"))
        {
            Console.WriteLine("  ✔ VISITAS_FV não existe ainda, nenhuma migração necessária.");
            return;
        }

        if (await ColumnExistsAsync(conn, "VISITAS_FV", "RESULTADO"))
        {
            Console.WriteLine("  ✔ VISITAS_FV.RESULTADO já existe.");
            return;
        }

        _logger.LogInformation("[Migration] Adicionando VISITAS_FV.RESULTADO...");
        ExecDDL(conn,
            "ALTER TABLE VISITAS_FV ADD RESULTADO VARCHAR(300)",
            "VISITAS_FV.RESULTADO criada (VARCHAR(300))");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 6. GRUPO.TIPO — coluna necessária para grupos do tipo PIZZA
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureGrupoTipoAsync(FbConnection conn)
    {
        if (await ColumnExistsAsync(conn, "GRUPO", "TIPO"))
        {
            Console.WriteLine("  ✔ GRUPO.TIPO já existe.");
            return;
        }

        _logger.LogInformation("[Migration] Adicionando coluna GRUPO.TIPO...");
        ExecDDL(conn,
            "ALTER TABLE GRUPO ADD TIPO VARCHAR(10) DEFAULT 'NORMAL'",
            "GRUPO.TIPO criada (VARCHAR(10) DEFAULT 'NORMAL')");
    }

    // ─────────────────────────────────────────────────────────────────────
    // 7. USUARIO LOJA — garantir CANCELAR='1' e VISUALIZAR='1' para acesso Gerente
    // ─────────────────────────────────────────────────────────────────────

    private async Task EnsureLojaUserPermissoesAsync(FbConnection conn)
    {
        try
        {
            // Verifica se o usuário LOJA já tem permissões de gerente (CANCELAR='1')
            var cancelar = await conn.ExecuteScalarAsync<string?>(
                "SELECT FIRST 1 CANCELAR FROM USUARIO WHERE UPPER(NOME) = 'LOJA' AND ATIVO = '1'");

            if (cancelar == "1")
            {
                Console.WriteLine("  ✔ USUARIO LOJA já possui permissões de Gerente (CANCELAR='1').");
                return;
            }

            _logger.LogInformation("[Migration] Corrigindo permissões do usuário LOJA...");
            var updated = await conn.ExecuteAsync(
                "UPDATE USUARIO SET CANCELAR = '1', VISUALIZAR = '1' WHERE UPPER(NOME) = 'LOJA' AND ATIVO = '1'");

            if (updated > 0)
            {
                _logger.LogInformation("  ✅ USUARIO LOJA: CANCELAR e VISUALIZAR definidos como '1' (acesso Gerente).");
                Console.WriteLine("  ✅ USUARIO LOJA: permissões de Gerente aplicadas (CANCELAR='1', VISUALIZAR='1').");
            }
            else
            {
                Console.WriteLine("  ℹ️ USUARIO LOJA não encontrado ou inativo — nenhuma alteração.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("  ⚠️ EnsureLojaUserPermissoesAsync: {Error}", ex.Message);
            Console.WriteLine($"  ⚠️ USUARIO LOJA permissões: {ex.Message}");
        }
    }

    private static async Task<bool> TableExistsAsync(FbConnection conn, string tableName)
    {
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM RDB$RELATIONS WHERE TRIM(RDB$RELATION_NAME) = @Name",
            new { Name = tableName.ToUpper() });
        return count > 0;
    }
}
