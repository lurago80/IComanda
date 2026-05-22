using System.Data;
using Dapper;
using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class OperadorComissaoRepository : IOperadorComissaoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<OperadorComissaoRepository> _logger;

    public OperadorComissaoRepository(IDbConnectionFactory connectionFactory, ILogger<OperadorComissaoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>Garante que a tabela OPERADOR_COMISSAO e o generator existam.</summary>
    private async Task EnsureTableExistsAsync(IDbConnection connection)
    {
        if (connection is not FbConnection fbConn) return;
        if (connection.State != ConnectionState.Open)
            connection.Open();

        var exists = await connection.ExecuteScalarAsync<int?>(
            "SELECT 1 FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = 'OPERADOR_COMISSAO'");
        if (exists.HasValue && exists.Value == 1) return;

        _logger.LogInformation("Criando tabela OPERADOR_COMISSAO e generator no Firebird...");

        var cmd = fbConn.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE OPERADOR_COMISSAO (
    ID              INTEGER      NOT NULL PRIMARY KEY,
    OPERADOR_ID     INTEGER      NOT NULL,
    DATA_INICIO     DATE         NOT NULL,
    DATA_FIM        DATE         NOT NULL,
    VALOR_VENDAS    NUMERIC(15,2) DEFAULT 0 NOT NULL,
    PERCENTUAL      NUMERIC(5,2) DEFAULT 0 NOT NULL,
    VALOR_COMISSAO  NUMERIC(15,2) DEFAULT 0 NOT NULL
)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "CREATE GENERATOR GEN_OPERADOR_COMISSAO_ID";
        try { cmd.ExecuteNonQuery(); } catch { /* generator pode já existir */ }

        cmd.CommandText = "SET GENERATOR GEN_OPERADOR_COMISSAO_ID TO 0";
        cmd.ExecuteNonQuery();

        _logger.LogInformation("Tabela OPERADOR_COMISSAO criada com sucesso.");
    }

    public async Task<IEnumerable<OperadorComissao>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);

        const string sql = @"
            SELECT
                ID             AS Id,
                OPERADOR_ID    AS OperadorId,
                DATA_INICIO    AS DataInicio,
                DATA_FIM       AS DataFim,
                VALOR_VENDAS   AS ValorVendas,
                PERCENTUAL     AS Percentual,
                VALOR_COMISSAO AS ValorComissao
            FROM OPERADOR_COMISSAO
            ORDER BY DATA_INICIO DESC";

        return await connection.QueryAsync<OperadorComissao>(sql);
    }

    public async Task<OperadorComissao?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);

        const string sql = @"
            SELECT
                ID             AS Id,
                OPERADOR_ID    AS OperadorId,
                DATA_INICIO    AS DataInicio,
                DATA_FIM       AS DataFim,
                VALOR_VENDAS   AS ValorVendas,
                PERCENTUAL     AS Percentual,
                VALOR_COMISSAO AS ValorComissao
            FROM OPERADOR_COMISSAO
            WHERE ID = @Id";

        return await connection.QueryFirstOrDefaultAsync<OperadorComissao>(sql, new { Id = id });
    }

    public async Task<IEnumerable<OperadorComissao>> GetByOperadorAsync(int operadorId)
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);

        const string sql = @"
            SELECT
                ID             AS Id,
                OPERADOR_ID    AS OperadorId,
                DATA_INICIO    AS DataInicio,
                DATA_FIM       AS DataFim,
                VALOR_VENDAS   AS ValorVendas,
                PERCENTUAL     AS Percentual,
                VALOR_COMISSAO AS ValorComissao
            FROM OPERADOR_COMISSAO
            WHERE OPERADOR_ID = @OperadorId
            ORDER BY DATA_INICIO DESC";

        return await connection.QueryAsync<OperadorComissao>(sql, new { OperadorId = operadorId });
    }

    public async Task CreateAsync(OperadorComissao operadorComissao)
    {
        using var connection = _connectionFactory.CreateConnection();
        await EnsureTableExistsAsync(connection);

        const string sqlId = "SELECT GEN_ID(GEN_OPERADOR_COMISSAO_ID, 1) FROM RDB$DATABASE";
        var novoId = await connection.ExecuteScalarAsync<int>(sqlId);

        const string sql = @"
            INSERT INTO OPERADOR_COMISSAO
                (ID, OPERADOR_ID, DATA_INICIO, DATA_FIM, VALOR_VENDAS, PERCENTUAL, VALOR_COMISSAO)
            VALUES
                (@Id, @OperadorId, @DataInicio, @DataFim, @ValorVendas, @Percentual, @ValorComissao)";

        await connection.ExecuteAsync(sql, new
        {
            Id             = novoId,
            OperadorId     = operadorComissao.OperadorId,
            DataInicio     = operadorComissao.DataInicio.Date,
            DataFim        = operadorComissao.DataFim.Date,
            ValorVendas    = operadorComissao.ValorVendas,
            Percentual     = operadorComissao.Percentual,
            ValorComissao  = operadorComissao.ValorComissao
        });

        operadorComissao.Id = novoId;
        _logger.LogInformation("OperadorComissao criada: ID={Id}, Operador={OperadorId}", novoId, operadorComissao.OperadorId);
    }

    public async Task UpdateAsync(OperadorComissao operadorComissao)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE OPERADOR_COMISSAO
            SET OPERADOR_ID    = @OperadorId,
                DATA_INICIO    = @DataInicio,
                DATA_FIM       = @DataFim,
                VALOR_VENDAS   = @ValorVendas,
                PERCENTUAL     = @Percentual,
                VALOR_COMISSAO = @ValorComissao
            WHERE ID = @Id";

        await connection.ExecuteAsync(sql, new
        {
            Id             = operadorComissao.Id,
            OperadorId     = operadorComissao.OperadorId,
            DataInicio     = operadorComissao.DataInicio.Date,
            DataFim        = operadorComissao.DataFim.Date,
            ValorVendas    = operadorComissao.ValorVendas,
            Percentual     = operadorComissao.Percentual,
            ValorComissao  = operadorComissao.ValorComissao
        });

        _logger.LogInformation("OperadorComissao atualizada: ID={Id}", operadorComissao.Id);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "DELETE FROM OPERADOR_COMISSAO WHERE ID = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });

        _logger.LogInformation("OperadorComissao excluída: ID={Id}", id);
    }

    /// <summary>
    /// Calcula o total de vendas de um operador no período e retorna o valor da comissão.
    /// Consulta diretamente a tabela VENDAS (campo VENDEDOR) do legado.
    /// </summary>
    public async Task<decimal> CalcularComissaoAsync(int operadorId, DateTime dataInicio, DateTime dataFim, decimal percentual)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COALESCE(SUM(TOTAL), 0)
            FROM VENDAS
            WHERE VENDEDOR   = @OperadorId
              AND EMISSAO   >= @DataInicio
              AND EMISSAO   <= @DataFim
              AND LANCADO    = 'EFETIVADO'";

        var totalVendas = await connection.ExecuteScalarAsync<decimal>(sql, new
        {
            OperadorId = operadorId,
            DataInicio = dataInicio.Date,
            DataFim    = dataFim.Date
        });

        var comissao = Math.Round(totalVendas * (percentual / 100m), 2);
        _logger.LogInformation(
            "Comissão calculada: Operador={Op}, Período={Ini:d}-{Fim:d}, Vendas={Vendas:C2}, Percentual={Pct}%, Comissão={Com:C2}",
            operadorId, dataInicio, dataFim, totalVendas, percentual, comissao);

        return comissao;
    }
}
