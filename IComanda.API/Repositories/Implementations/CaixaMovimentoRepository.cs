using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IComanda.API.Repositories.Implementations;

public class CaixaMovimentoRepository : ICaixaMovimentoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CaixaMovimentoRepository> _logger;

    public CaixaMovimentoRepository(IDbConnectionFactory connectionFactory, ILogger<CaixaMovimentoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<int> GerarProximoCodigoAsync(IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Tentar usar generator se existir
            try
            {
                var sql = "SELECT GEN_ID(CAIXA_GEN, 1) AS GEN FROM RDB$DATABASE";
                var resultado = await connection.QuerySingleAsync<dynamic>(sql, transaction: transaction);
                return (int)resultado.GEN;
            }
            catch
            {
                // Se não existir generator, usar MAX + 1
                var sql = "SELECT COALESCE(MAX(CODIGO), 0) + 1 AS PROXIMO_CODIGO FROM CAIXA";
                var proximoCodigo = await connection.QuerySingleAsync<int>(sql, transaction: transaction);
                return proximoCodigo;
            }
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<int> CriarMovimentoAsync(CaixaMovimento movimento, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Se o código não foi definido, gerar o próximo
            if (movimento.Codigo == 0)
            {
                movimento.Codigo = await GerarProximoCodigoAsync(transaction);
            }

            // Calcular saldo atual antes de inserir
            var saldoAtual = await GetSaldoAtualAsync(movimento.Terminal);
            movimento.Saldo = saldoAtual + movimento.Entrada - movimento.Saida;

            var sql = @"
                INSERT INTO CAIXA (
                    CODIGO, DATA, HORA, DOCUMENTO, CUSTO, CONTA,
                    ENTRADA, SAIDA, SALDO, ORIGEM, OPERADOR, HISTORICO,
                    GRAVACAO, CODPROF, TERMINAL, VENDEDOR
                ) VALUES (
                    @Codigo, @Data, @Hora, @Documento, @Custo, @Conta,
                    @Entrada, @Saida, @Saldo, @Origem, @Operador, @Historico,
                    @Gravacao, @CodProf, @Terminal, @Vendedor
                )";

            _logger.LogInformation("💾 Criando movimento de caixa - Codigo: {Codigo}, Terminal: {Terminal}, Origem: {Origem}, Entrada: {Entrada}, Saida: {Saida}",
                movimento.Codigo, movimento.Terminal, movimento.Origem, movimento.Entrada, movimento.Saida);

            var linhasAfetadas = await connection.ExecuteAsync(sql, new
            {
                movimento.Codigo,
                movimento.Data,
                movimento.Hora,
                movimento.Documento,
                movimento.Custo,
                movimento.Conta,
                movimento.Entrada,
                movimento.Saida,
                movimento.Saldo,
                movimento.Origem,
                movimento.Operador,
                movimento.Historico,
                movimento.Gravacao,
                movimento.CodProf,
                movimento.Terminal,
                movimento.Vendedor
            }, transaction: transaction);

            if (linhasAfetadas > 0)
            {
                _logger.LogInformation("✅ Movimento de caixa criado com sucesso - Codigo: {Codigo}, Saldo: {Saldo}",
                    movimento.Codigo, movimento.Saldo);
                return movimento.Codigo;
            }

            throw new Exception("Erro ao criar movimento de caixa");
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<decimal> GetSaldoAtualAsync(int terminal)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT COALESCE(MAX(SALDO), 0)
            FROM CAIXA
            WHERE TERMINAL = @Terminal
              AND DATA = CURRENT_DATE";

        var saldo = await connection.QuerySingleOrDefaultAsync<decimal?>(sql, new { Terminal = terminal });
        return saldo ?? 0;
    }

    public async Task<IEnumerable<CaixaMovimento>> GetMovimentosAsync(int terminal, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE TERMINAL = @Terminal";
        var parameters = new DynamicParameters();
        parameters.Add("@Terminal", terminal);

        if (dataInicio.HasValue)
        {
            whereClause += " AND DATA >= @DataInicio";
            parameters.Add("@DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND DATA <= @DataFim";
            parameters.Add("@DataFim", dataFim.Value.Date.AddDays(1).AddSeconds(-1));
        }

        var sql = $@"
            SELECT 
                CODIGO, DATA, HORA, DOCUMENTO, CUSTO, CONTA,
                ENTRADA, SAIDA, SALDO, ORIGEM, OPERADOR, HISTORICO,
                GRAVACAO, CODPROF, TERMINAL, VENDEDOR
            FROM CAIXA
            {whereClause}
            ORDER BY DATA DESC, HORA DESC, CODIGO DESC";

        _logger.LogInformation("🔍 Buscando movimentos de caixa - Terminal: {Terminal}, Data Inicio: {DataInicio}, Data Fim: {DataFim}",
            terminal, dataInicio, dataFim);

        return await connection.QueryAsync<CaixaMovimento>(sql, parameters);
    }

    public async Task<IEnumerable<CaixaMovimento>> GetMovimentosPorOrigemAsync(int terminal, string origem, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE TERMINAL = @Terminal AND ORIGEM = @Origem";
        var parameters = new DynamicParameters();
        parameters.Add("@Terminal", terminal);
        parameters.Add("@Origem", origem);

        if (dataInicio.HasValue)
        {
            whereClause += " AND DATA >= @DataInicio";
            parameters.Add("@DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND DATA <= @DataFim";
            parameters.Add("@DataFim", dataFim.Value.Date.AddDays(1).AddSeconds(-1));
        }

        var sql = $@"
            SELECT 
                CODIGO, DATA, HORA, DOCUMENTO, CUSTO, CONTA,
                ENTRADA, SAIDA, SALDO, ORIGEM, OPERADOR, HISTORICO,
                GRAVACAO, CODPROF, TERMINAL, VENDEDOR
            FROM CAIXA
            {whereClause}
            ORDER BY DATA DESC, HORA DESC, CODIGO DESC";

        _logger.LogInformation("🔍 Buscando movimentos de caixa por origem - Terminal: {Terminal}, Origem: {Origem}",
            terminal, origem);

        return await connection.QueryAsync<CaixaMovimento>(sql, parameters);
    }

    public async Task<decimal?> GetValorAberturaAsync(int terminal, DateTime data)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT FIRST 1 ENTRADA
            FROM CAIXA
            WHERE TERMINAL = @Terminal
              AND ORIGEM = 'ABERTURA'
              AND DATA = @Data
            ORDER BY DATA DESC, HORA DESC, CODIGO DESC";

        var valor = await connection.QuerySingleOrDefaultAsync<decimal?>(sql, new { Terminal = terminal, Data = data.Date });
        return valor;
    }

    public async Task<IEnumerable<CaixaMovimento>> GetAberturasPorPeriodoAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE ORIGEM = 'ABERTURA'";
        var parameters = new DynamicParameters();

        if (dataInicio.HasValue)
        {
            whereClause += " AND DATA >= @DataInicio";
            parameters.Add("@DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND DATA <= @DataFim";
            parameters.Add("@DataFim", dataFim.Value.Date);
        }

        var sql = $@"
            SELECT 
                CODIGO, DATA, HORA, DOCUMENTO, CUSTO, CONTA,
                ENTRADA, SAIDA, SALDO, ORIGEM, OPERADOR, HISTORICO,
                GRAVACAO, CODPROF, TERMINAL, VENDEDOR
            FROM CAIXA
            {whereClause}
            ORDER BY TERMINAL, DATA DESC, HORA DESC";

        var movimentos = await connection.QueryAsync<CaixaMovimento>(sql, parameters);
        return movimentos;
    }

    public async Task<CaixaMovimento?> GetPrimeiraAberturaAsync(int terminal, DateTime data)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT FIRST 1
                CODIGO, DATA, HORA, DOCUMENTO, CUSTO, CONTA,
                ENTRADA, SAIDA, SALDO, ORIGEM, OPERADOR, HISTORICO,
                GRAVACAO, CODPROF, TERMINAL, VENDEDOR
            FROM CAIXA
            WHERE TERMINAL = @Terminal
              AND ORIGEM = 'ABERTURA'
              AND CAST(DATA AS DATE) = @Data
            ORDER BY DATA ASC, HORA ASC, CODIGO ASC";

        var movimento = await connection.QueryFirstOrDefaultAsync<CaixaMovimento>(sql, new { Terminal = terminal, Data = data.Date });
        return movimento;
    }

    public async Task<IEnumerable<CaixaMovimento>> GetFechamentosPorPeriodoAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE ORIGEM = 'FECHAMENTO'";
        var parameters = new DynamicParameters();

        if (dataInicio.HasValue)
        {
            whereClause += " AND DATA >= @DataInicio";
            parameters.Add("@DataInicio", dataInicio.Value.Date);
        }

        if (dataFim.HasValue)
        {
            whereClause += " AND DATA <= @DataFim";
            parameters.Add("@DataFim", dataFim.Value.Date);
        }

        var sql = $@"
            SELECT 
                CODIGO, DATA, HORA, DOCUMENTO, CUSTO, CONTA,
                ENTRADA, SAIDA, SALDO, ORIGEM, OPERADOR, HISTORICO,
                GRAVACAO, CODPROF, TERMINAL, VENDEDOR
            FROM CAIXA
            {whereClause}
            ORDER BY TERMINAL, DATA DESC, HORA DESC";

        var movimentos = await connection.QueryAsync<CaixaMovimento>(sql, parameters);
        return movimentos;
    }

    public async Task<bool> ExisteFechamentoAsync(int terminal, DateTime data)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT FIRST 1 1
            FROM CAIXA
            WHERE TERMINAL = @Terminal
              AND ORIGEM = 'FECHAMENTO'
              AND CAST(DATA AS DATE) = @Data";

        var resultado = await connection.QueryFirstOrDefaultAsync<int?>(sql, new { Terminal = terminal, Data = data.Date });
        return resultado.HasValue;
    }
}
