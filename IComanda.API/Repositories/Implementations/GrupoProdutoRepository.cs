using System.Data;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class GrupoProdutoRepository : IGrupoProdutoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<GrupoProdutoRepository> _logger;

    public GrupoProdutoRepository(IDbConnectionFactory connectionFactory, ILogger<GrupoProdutoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>Garante que a tabela GRUPO_PRODUTO e o generator existam.</summary>
    private async Task EnsureTableExistsAsync(IDbConnection connection)
    {
        if (connection is not FbConnection fbConn) return;
        if (connection.State != ConnectionState.Open)
            connection.Open();

        var exists = await connection.ExecuteScalarAsync<int?>(
            "SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'GRUPO_PRODUTO'");
        if (exists.HasValue && exists.Value == 1) return;

        _logger.LogInformation("Criando tabela GRUPO_PRODUTO e generator no Firebird...");

        var cmd = fbConn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE GRUPO_PRODUTO (
    ID       INTEGER      NOT NULL PRIMARY KEY,
    NOME     VARCHAR(100) NOT NULL,
    ATIVO    SMALLINT     DEFAULT 1 NOT NULL,
    ORDEM    INTEGER
)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE GENERATOR GEN_GRUPO_PRODUTO_ID";
        try { cmd.ExecuteNonQuery(); } catch { /* generator pode já existir */ }

        cmd.CommandText = "SET GENERATOR GEN_GRUPO_PRODUTO_ID TO 0";
        cmd.ExecuteNonQuery();

        _logger.LogInformation("Tabela GRUPO_PRODUTO criada com sucesso.");
    }

    public async Task<IEnumerable<GrupoProduto>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);

        const string sql = @"
            SELECT
                ID    AS Id,
                NOME  AS Nome,
                ATIVO AS Ativo,
                ORDEM AS Ordem
            FROM GRUPO_PRODUTO
            ORDER BY COALESCE(ORDEM, 9999), NOME";

        var resultado = await connection.QueryAsync<GrupoProduto>(sql);
        return resultado;
    }

    public async Task<GrupoProduto?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);

        const string sql = @"
            SELECT
                ID    AS Id,
                NOME  AS Nome,
                ATIVO AS Ativo,
                ORDEM AS Ordem
            FROM GRUPO_PRODUTO
            WHERE ID = @Id";

        return await connection.QueryFirstOrDefaultAsync<GrupoProduto>(sql, new { Id = id });
    }

    public async Task CreateAsync(GrupoProduto grupoProduto)
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);

        const string sqlId = "SELECT GEN_ID(GEN_GRUPO_PRODUTO_ID, 1) FROM RDB$DATABASE";
        var novoId = await connection.ExecuteScalarAsync<int>(sqlId);

        const string sql = @"
            INSERT INTO GRUPO_PRODUTO (ID, NOME, ATIVO, ORDEM)
            VALUES (@Id, @Nome, @Ativo, @Ordem)";

        await connection.ExecuteAsync(sql, new
        {
            Id    = novoId,
            Nome  = grupoProduto.Nome,
            Ativo = grupoProduto.Ativo ? 1 : 0,
            Ordem = grupoProduto.Ordem
        });

        grupoProduto.Id = novoId;
        _logger.LogInformation("GrupoProduto criado: ID={Id}, Nome={Nome}", novoId, grupoProduto.Nome);
    }

    public async Task UpdateAsync(GrupoProduto grupoProduto)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE GRUPO_PRODUTO
            SET NOME  = @Nome,
                ATIVO = @Ativo,
                ORDEM = @Ordem
            WHERE ID  = @Id";

        await connection.ExecuteAsync(sql, new
        {
            Id    = grupoProduto.Id,
            Nome  = grupoProduto.Nome,
            Ativo = grupoProduto.Ativo ? 1 : 0,
            Ordem = grupoProduto.Ordem
        });

        _logger.LogInformation("GrupoProduto atualizado: ID={Id}", grupoProduto.Id);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM GRUPO_PRODUTO WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });

        _logger.LogInformation("GrupoProduto excluído: ID={Id}", id);
    }
}
