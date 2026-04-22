using System.Data;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class TaxaEntregaRepository : ITaxaEntregaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<TaxaEntregaRepository> _logger;

    public TaxaEntregaRepository(IDbConnectionFactory connectionFactory, ILogger<TaxaEntregaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>Cria a tabela TAXA_ENTREGA e o generator se não existirem.</summary>
    private async Task EnsureTableExistsAsync(IDbConnection connection)
    {
        if (connection is not FbConnection fbConn) return;
        if (connection.State != ConnectionState.Open)
            connection.Open();
        var exists = await connection.ExecuteScalarAsync<int?>(
            "SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'TAXA_ENTREGA'");
        if (exists.HasValue && exists.Value == 1) return;

        _logger.LogInformation("Criando tabela TAXA_ENTREGA e generator no Firebird...");
        var cmd = fbConn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE TAXA_ENTREGA (
    ID INTEGER NOT NULL PRIMARY KEY,
    DESCRICAO VARCHAR(100) NOT NULL,
    VALOR NUMERIC(15,2) NOT NULL
)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE GENERATOR GEN_TAXA_ENTREGA_ID";
        try { cmd.ExecuteNonQuery(); } catch { /* generator pode já existir */ }

        _logger.LogInformation("Tabela TAXA_ENTREGA criada com sucesso.");
    }

    public async Task<IEnumerable<TaxaEntrega>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);
        var sql = @"
            SELECT ID as Id, DESCRICAO as Descricao, VALOR as Valor
            FROM TAXA_ENTREGA
            ORDER BY DESCRICAO";
        var result = await connection.QueryAsync<TaxaEntrega>(sql);
        return result;
    }

    public async Task<TaxaEntrega?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);
        var sql = @"
            SELECT ID as Id, DESCRICAO as Descricao, VALOR as Valor
            FROM TAXA_ENTREGA
            WHERE ID = @Id";
        return await connection.QueryFirstOrDefaultAsync<TaxaEntrega>(sql, new { Id = id });
    }

    public async Task<int> CriarAsync(string descricao, decimal valor)
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);
        var sqlId = "SELECT GEN_ID(GEN_TAXA_ENTREGA_ID, 1) FROM RDB$DATABASE";
        var id = await connection.ExecuteScalarAsync<int>(sqlId);
        var sql = @"
            INSERT INTO TAXA_ENTREGA (ID, DESCRICAO, VALOR)
            VALUES (@Id, @Descricao, @Valor)";
        await connection.ExecuteAsync(sql, new { Id = id, Descricao = descricao, Valor = valor });
        return id;
    }

    public async Task<bool> AtualizarAsync(int id, string descricao, decimal valor)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            UPDATE TAXA_ENTREGA
            SET DESCRICAO = @Descricao, VALOR = @Valor
            WHERE ID = @Id";
        var linhas = await connection.ExecuteAsync(sql, new { Id = id, Descricao = descricao, Valor = valor });
        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "DELETE FROM TAXA_ENTREGA WHERE ID = @Id";
        var linhas = await connection.ExecuteAsync(sql, new { Id = id });
        return linhas > 0;
    }
}
