using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IComanda.API.Repositories.Implementations;

public class CaixaRepository : ICaixaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CaixaRepository> _logger;

    public CaixaRepository(IDbConnectionFactory connectionFactory, ILogger<CaixaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Caixa?> GetCaixaAbertoAsync(int numero)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Como não há tabela CAIXA, vamos simular usando VENDAS
        // Um caixa está "aberto" se houver vendas do dia com aquele número de caixa
        var sql = @"
            SELECT FIRST 1
                CAST(@Numero AS INTEGER) AS Id,
                CAST(@Numero AS INTEGER) AS Numero,
                CURRENT_DATE AS DataAbertura,
                NULL AS DataFechamento,
                CAST(COALESCE(MAX(OPERADOR), 1) AS INTEGER) AS OperadorAbertura,
                NULL AS OperadorFechamento,
                0.0 AS ValorAbertura,
                NULL AS ValorFechamento,
                'ABERTO' AS Status,
                NULL AS Observacoes
            FROM VENDAS
            WHERE CAIXA = @Numero 
              AND DATA_SAIDA = CURRENT_DATE
              AND ORIGEM = 'BA'";

        _logger.LogInformation("🔍 Buscando caixa aberto número {Numero}", numero);

        var caixa = await connection.QueryFirstOrDefaultAsync<Caixa>(sql, new { Numero = numero });
        return caixa;
    }

    public async Task<Caixa?> GetCaixaPorIdAsync(int id)
    {
        // Para simplificar, vamos usar o número como ID
        return await GetCaixaAbertoAsync(id);
    }

    public async Task<IEnumerable<Caixa>> GetCaixasAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE ORIGEM = 'BA'";
        var parameters = new DynamicParameters();

        if (dataInicio.HasValue)
        {
            whereClause += " AND DATA_SAIDA >= @DataInicio";
            parameters.Add("@DataInicio", dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND DATA_SAIDA <= @DataFim";
            parameters.Add("@DataFim", dataFim.Value);
        }

        var sql = $@"
            SELECT DISTINCT
                CAIXA AS Numero,
                CAIXA AS Id,
                MIN(DATA_SAIDA) AS DataAbertura,
                MAX(CASE WHEN LANCADO = 'EFETIVADO' THEN DATA_SAIDA ELSE NULL END) AS DataFechamento,
                CAST(COALESCE(MIN(OPERADOR), 1) AS INTEGER) AS OperadorAbertura,
                NULL AS OperadorFechamento,
                0.0 AS ValorAbertura,
                NULL AS ValorFechamento,
                CASE 
                    WHEN MAX(CASE WHEN LANCADO = 'ABERTO' THEN 1 ELSE 0 END) = 1 THEN 'ABERTO'
                    ELSE 'FECHADO'
                END AS Status,
                NULL AS Observacoes
            FROM VENDAS
            {whereClause}
            AND CAIXA > 0
            GROUP BY CAIXA
            ORDER BY CAIXA DESC";

        _logger.LogInformation("🔍 Buscando caixas - Data Inicio: {DataInicio}, Data Fim: {DataFim}", dataInicio, dataFim);

        var caixas = await connection.QueryAsync<Caixa>(sql, parameters);
        return caixas;
    }

    public async Task<int> CriarCaixaAsync(Caixa caixa, IDbTransaction? transaction = null)
    {
        // Como não há tabela CAIXA, vamos apenas validar se o caixa já está aberto
        var caixaExistente = await GetCaixaAbertoAsync(caixa.Numero);
        if (caixaExistente != null && caixaExistente.Status == "ABERTO")
        {
            throw new InvalidOperationException($"Caixa {caixa.Numero} já está aberto");
        }

        _logger.LogInformation("✅ Caixa {Numero} considerado aberto (será usado nas próximas vendas)", caixa.Numero);
        return caixa.Numero;
    }

    public Task<bool> AtualizarCaixaAsync(Caixa caixa, IDbTransaction? transaction = null)
    {
        // Como não há tabela CAIXA, vamos apenas logar
        _logger.LogInformation("✅ Caixa {Numero} atualizado - Status: {Status}", caixa.Numero, caixa.Status);
        return Task.FromResult(true);
    }

    public async Task<decimal> GetTotalVendasPorCaixaAsync(int caixaId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE CAIXA = @CaixaId AND ORIGEM = 'BA' AND LANCADO = 'EFETIVADO'";
        var parameters = new DynamicParameters();
        parameters.Add("@CaixaId", caixaId);

        if (dataInicio.HasValue)
        {
            whereClause += " AND DATA_SAIDA >= @DataInicio";
            parameters.Add("@DataInicio", dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND DATA_SAIDA <= @DataFim";
            parameters.Add("@DataFim", dataFim.Value);
        }

        var sql = $@"
            SELECT COALESCE(SUM(TOTAL), 0)
            FROM VENDAS
            {whereClause}";

        var total = await connection.QuerySingleAsync<decimal>(sql, parameters);
        return total;
    }

    public async Task<decimal> GetTotalRecebimentosPorCaixaAsync(int caixaId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE RV.N_CAIXA = @CaixaId";
        var parameters = new DynamicParameters();
        parameters.Add("@CaixaId", caixaId);

        if (dataInicio.HasValue)
        {
            whereClause += " AND V.DATA_SAIDA >= @DataInicio";
            parameters.Add("@DataInicio", dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND V.DATA_SAIDA <= @DataFim";
            parameters.Add("@DataFim", dataFim.Value);
        }

        var sql = $@"
            SELECT COALESCE(SUM(RV.VALOR), 0)
            FROM RECEBIMENTO_VENDAS RV
            INNER JOIN VENDAS V ON V.NOTA = RV.NOTA
            {whereClause}
            AND V.ORIGEM = 'BA'";

        var total = await connection.QuerySingleAsync<decimal>(sql, parameters);
        return total;
    }
}

