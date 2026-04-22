using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class MetaFVRepository : IMetaFVRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<MetaFVRepository> _logger;

    public MetaFVRepository(IDbConnectionFactory connectionFactory, ILogger<MetaFVRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    private const string SelectMeta = @"
        SELECT
            M.ID            AS Id,
            M.ID_VENDEDOR   AS IdVendedor,
            V.NOME          AS NomeVendedor,
            M.MES           AS Mes,
            M.ANO           AS Ano,
            M.VALOR_META    AS ValorMeta,
            M.VALOR_REALIZADO AS ValorRealizado
        FROM METAS_FV M
        LEFT JOIN VENDEDOR V ON V.ID = M.ID_VENDEDOR";

    public async Task<IEnumerable<MetaFV>> GetByVendedorAsync(int idVendedor)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectMeta} WHERE M.ID_VENDEDOR = @IdVendedor ORDER BY M.ANO DESC, M.MES DESC";
        return await connection.QueryAsync<MetaFV>(sql, new { IdVendedor = idVendedor });
    }

    public async Task<MetaFV?> GetAsync(int idVendedor, int mes, int ano)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectMeta} WHERE M.ID_VENDEDOR = @IdVendedor AND M.MES = @Mes AND M.ANO = @Ano";
        return await connection.QuerySingleOrDefaultAsync<MetaFV>(sql, new { IdVendedor = idVendedor, Mes = mes, Ano = ano });
    }

    public async Task<IEnumerable<MetaFV>> GetMesAsync(int mes, int ano)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectMeta} WHERE M.MES = @Mes AND M.ANO = @Ano ORDER BY V.NOME";
        return await connection.QueryAsync<MetaFV>(sql, new { Mes = mes, Ano = ano });
    }

    public async Task<int> CriarOuAtualizarAsync(MetaFV meta)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Verificar se já existe
        var existente = await GetAsync(meta.IdVendedor, meta.Mes, meta.Ano);

        if (existente != null)
        {
            const string sqlUpdate = @"
                UPDATE METAS_FV SET VALOR_META = @ValorMeta
                WHERE ID_VENDEDOR = @IdVendedor AND MES = @Mes AND ANO = @Ano";
            await connection.ExecuteAsync(sqlUpdate, new { meta.ValorMeta, meta.IdVendedor, meta.Mes, meta.Ano });
            _logger.LogInformation("Meta FV atualizada: Vendedor={V} Mês={M}/{A} Meta={Meta}", meta.IdVendedor, meta.Mes, meta.Ano, meta.ValorMeta);
            return existente.Id;
        }

        const string sqlInsert = @"
            INSERT INTO METAS_FV (ID_VENDEDOR, MES, ANO, VALOR_META, VALOR_REALIZADO)
            VALUES (@IdVendedor, @Mes, @Ano, @ValorMeta, 0)
            RETURNING ID";
        var id = await connection.ExecuteScalarAsync<int>(sqlInsert, new { meta.IdVendedor, meta.Mes, meta.Ano, meta.ValorMeta });
        _logger.LogInformation("Meta FV criada #{Id}: Vendedor={V} Mês={M}/{A} Meta={Meta}", id, meta.IdVendedor, meta.Mes, meta.Ano, meta.ValorMeta);
        return id;
    }

    public async Task<bool> AtualizarRealizadoAsync(int idVendedor, int mes, int ano, decimal valorRealizado)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE METAS_FV SET VALOR_REALIZADO = @ValorRealizado
            WHERE ID_VENDEDOR = @IdVendedor AND MES = @Mes AND ANO = @Ano";
        var rows = await connection.ExecuteAsync(sql, new { IdVendedor = idVendedor, Mes = mes, Ano = ano, ValorRealizado = valorRealizado });
        return rows > 0;
    }
}
