using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IComanda.API.Repositories.Implementations;

public class ReceberRepository : IReceberRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ReceberRepository> _logger;

    public ReceberRepository(IDbConnectionFactory connectionFactory, ILogger<ReceberRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<string> GerarProximoNumeroAsync(IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Buscar o maior número e incrementar
            var sql = @"
                SELECT COALESCE(MAX(CAST(NUMERO AS INTEGER)), 0) + 1 AS PROXIMO_NUMERO
                FROM RECEBER";

            var proximoNumero = await connection.QuerySingleAsync<int>(sql, transaction: transaction);
            return proximoNumero.ToString().PadLeft(7, '0');
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<string> GerarProximaOrdemAsync(string numero, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Buscar a maior ordem para o número e incrementar
            var sql = @"
                SELECT COALESCE(MAX(CAST(ORDEM AS INTEGER)), 0) + 1 AS PROXIMA_ORDEM
                FROM RECEBER
                WHERE NUMERO = @Numero";

            var proximaOrdem = await connection.QuerySingleAsync<int>(sql, new { Numero = numero }, transaction: transaction);
            return proximaOrdem.ToString().PadLeft(3, '0');
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<bool> CriarReceberAsync(Receber receber, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Se o número não foi definido, gerar o próximo
            if (string.IsNullOrWhiteSpace(receber.Numero))
            {
                receber.Numero = await GerarProximoNumeroAsync(transaction);
            }

            // Se a ordem não foi definida, gerar a próxima
            if (string.IsNullOrWhiteSpace(receber.Ordem))
            {
                receber.Ordem = await GerarProximaOrdemAsync(receber.Numero, transaction);
            }

        var sql = @"
            INSERT INTO RECEBER (
                NUMERO, ORDEM, CODIGO, TIPO, MODELO, SERIE, SUBSERIE, ORIGEM,
                HISTORICO, EMISSAO, VENCIMENTO, VALOR, RECEBIMENTO, VALOR_RECEBIDO,
                ACRESCIMO, DESCONTO, FIXO, JUROS, CONTROLE, OPERADOR, ESPECIE,
                BANCO, CONTA, CUSTO, AG_CONTA, AG_BANCO, QUITADONOECF,
                DINHEIRO, CHEQUE, CARTAO, BOLETO, TROCO, ID_VENDEDOR, NUMCHQ,
                CODPROF, CARTAOD, VALOR_ORIGINAL, PIX, HORA_RECEB, IMPRESSO,
                REMESSA, QRCODE, CONTROLE_NOTA, TERMINAL, NOTAFISCAL
            ) VALUES (
                @Numero, @Ordem, @Codigo, @Tipo, @Modelo, @Serie, @Subserie, @Origem,
                @Historico, @Emissao, @Vencimento, @Valor, @Recebimento, @ValorRecebido,
                @Acrescimo, @Desconto, @Fixo, @Juros, @Controle, @Operador, @Especie,
                @Banco, @Conta, @Custo, @AgConta, @AgBanco, @QuitadoNoEcf,
                @Dinheiro, @Cheque, @Cartao, @Boleto, @Troco, @IdVendedor, @NumChq,
                @CodProf, @CartaoD, @ValorOriginal, @Pix, @HoraReceb, @Impresso,
                @Remessa, @Qrcode, @ControleNota, @Terminal, @NotaFiscal
            )";

        _logger.LogInformation("💾 Criando registro em RECEBER - Numero: {Numero}, Ordem: {Ordem}, Valor: {Valor}, Vencimento: {Vencimento}",
            receber.Numero, receber.Ordem, receber.Valor, receber.Vencimento);

            var linhasAfetadas = await connection.ExecuteAsync(sql, new
            {
                receber.Numero,
                receber.Ordem,
                receber.Codigo,
                receber.Tipo,
                receber.Modelo,
                receber.Serie,
                receber.Subserie,
                receber.Origem,
                receber.Historico,
                receber.Emissao,
                receber.Vencimento,
                receber.Valor,
                receber.Recebimento,
                ValorRecebido = receber.ValorRecebido,
                receber.Acrescimo,
                receber.Desconto,
                receber.Fixo,
                receber.Juros,
                receber.Controle,
                receber.Operador,
                receber.Especie,
                receber.Banco,
                receber.Conta,
                receber.Custo,
                AgConta = receber.AgConta,
                AgBanco = receber.AgBanco,
                QuitadoNoEcf = receber.QuitadoNoEcf,
                receber.Dinheiro,
                receber.Cheque,
                receber.Cartao,
                receber.Boleto,
                receber.Troco,
                IdVendedor = receber.IdVendedor,
                NumChq = receber.NumChq,
                CodProf = receber.CodProf,
                CartaoD = receber.CartaoD,
                ValorOriginal = receber.ValorOriginal,
                receber.Pix,
                HoraReceb = receber.HoraReceb,
                receber.Impresso,
                receber.Remessa,
                receber.Qrcode,
                ControleNota = receber.ControleNota,
                receber.Terminal,
                NotaFiscal = receber.NotaFiscal
            }, transaction: transaction);

            if (linhasAfetadas > 0)
            {
                _logger.LogInformation("✅ Registro em RECEBER criado com sucesso - Numero: {Numero}, Ordem: {Ordem}",
                    receber.Numero, receber.Ordem);
                return true;
            }

            throw new Exception("Erro ao criar registro em RECEBER");
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<Receber?> GetReceberPorNumeroOrdemAsync(string numero, string ordem)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                NUMERO AS Numero,
                ORDEM AS Ordem,
                CODIGO AS Codigo,
                TIPO AS Tipo,
                MODELO AS Modelo,
                SERIE AS Serie,
                SUBSERIE AS Subserie,
                ORIGEM AS Origem,
                HISTORICO AS Historico,
                EMISSAO AS Emissao,
                VENCIMENTO AS Vencimento,
                VALOR AS Valor,
                RECEBIMENTO AS Recebimento,
                VALOR_RECEBIDO AS ValorRecebido,
                ACRESCIMO AS Acrescimo,
                DESCONTO AS Desconto,
                FIXO AS Fixo,
                JUROS AS Juros,
                CONTROLE AS Controle,
                OPERADOR AS Operador,
                ESPECIE AS Especie,
                BANCO AS Banco,
                CONTA AS Conta,
                CUSTO AS Custo,
                AG_CONTA AS AgConta,
                AG_BANCO AS AgBanco,
                QUITADONOECF AS QuitadoNoEcf,
                DINHEIRO AS Dinheiro,
                CHEQUE AS Cheque,
                CARTAO AS Cartao,
                BOLETO AS Boleto,
                TROCO AS Troco,
                ID_VENDEDOR AS IdVendedor,
                NUMCHQ AS NumChq,
                CODPROF AS CodProf,
                CARTAOD AS CartaoD,
                VALOR_ORIGINAL AS ValorOriginal,
                PIX AS Pix,
                HORA_RECEB AS HoraReceb,
                IMPRESSO AS Impresso,
                REMESSA AS Remessa,
                QRCODE AS Qrcode,
                CONTROLE_NOTA AS ControleNota,
                TERMINAL AS Terminal,
                NOTAFISCAL AS NotaFiscal
            FROM RECEBER
            WHERE NUMERO = @Numero AND ORDEM = @Ordem";

        _logger.LogInformation("🔍 Buscando conta a receber - Numero: {Numero}, Ordem: {Ordem}", numero, ordem);

        var receber = await connection.QueryFirstOrDefaultAsync<Receber>(sql, new { Numero = numero, Ordem = ordem });
        return receber;
    }

    public async Task<IEnumerable<Receber>> GetReceberPendentesAsync(int? codigoCliente = null, DateTime? dataVencimentoInicio = null, DateTime? dataVencimentoFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (codigoCliente.HasValue)
        {
            // Quando um cliente específico é informado, usar a condição exata: valor_recebido = 0
            whereConditions.Add("CODIGO = @CodigoCliente");
            whereConditions.Add("VALOR_RECEBIDO = 0");
            parameters.Add("@CodigoCliente", codigoCliente.Value);
        }
        else
        {
            // Quando não há cliente específico, buscar todas as pendentes (valor_recebido < valor)
            whereConditions.Add("VALOR_RECEBIDO < VALOR OR VALOR_RECEBIDO IS NULL");
        }

        if (dataVencimentoInicio.HasValue)
        {
            whereConditions.Add("VENCIMENTO >= @DataVencimentoInicio");
            parameters.Add("@DataVencimentoInicio", dataVencimentoInicio.Value);
        }

        if (dataVencimentoFim.HasValue)
        {
            whereConditions.Add("VENCIMENTO <= @DataVencimentoFim");
            parameters.Add("@DataVencimentoFim", dataVencimentoFim.Value);
        }

        var whereClause = $"WHERE {string.Join(" AND ", whereConditions)}";

        var sql = $@"
            SELECT 
                NUMERO AS Numero,
                ORDEM AS Ordem,
                CODIGO AS Codigo,
                TIPO AS Tipo,
                MODELO AS Modelo,
                SERIE AS Serie,
                SUBSERIE AS Subserie,
                ORIGEM AS Origem,
                HISTORICO AS Historico,
                EMISSAO AS Emissao,
                VENCIMENTO AS Vencimento,
                VALOR AS Valor,
                RECEBIMENTO AS Recebimento,
                VALOR_RECEBIDO AS ValorRecebido,
                ACRESCIMO AS Acrescimo,
                DESCONTO AS Desconto,
                FIXO AS Fixo,
                JUROS AS Juros,
                CONTROLE AS Controle,
                OPERADOR AS Operador,
                ESPECIE AS Especie,
                BANCO AS Banco,
                CONTA AS Conta,
                CUSTO AS Custo,
                AG_CONTA AS AgConta,
                AG_BANCO AS AgBanco,
                QUITADONOECF AS QuitadoNoEcf,
                DINHEIRO AS Dinheiro,
                CHEQUE AS Cheque,
                CARTAO AS Cartao,
                BOLETO AS Boleto,
                TROCO AS Troco,
                ID_VENDEDOR AS IdVendedor,
                NUMCHQ AS NumChq,
                CODPROF AS CodProf,
                CARTAOD AS CartaoD,
                VALOR_ORIGINAL AS ValorOriginal,
                PIX AS Pix,
                HORA_RECEB AS HoraReceb,
                IMPRESSO AS Impresso,
                REMESSA AS Remessa,
                QRCODE AS Qrcode,
                CONTROLE_NOTA AS ControleNota,
                TERMINAL AS Terminal,
                NOTAFISCAL AS NotaFiscal
            FROM RECEBER
            {whereClause}
            ORDER BY VENCIMENTO ASC";

        _logger.LogInformation("🔍 Buscando contas a receber pendentes - Cliente: {Cliente}, Data Inicio: {DataInicio}, Data Fim: {DataFim}",
            codigoCliente, dataVencimentoInicio, dataVencimentoFim);

        var receber = await connection.QueryAsync<Receber>(sql, parameters);
        return receber;
    }

    public async Task<IEnumerable<Receber>> GetReceberPorClienteAsync(int codigoCliente, bool apenasPendentes = true)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string> { "CODIGO = @CodigoCliente" };
        var parameters = new DynamicParameters();
        parameters.Add("@CodigoCliente", codigoCliente);

        if (apenasPendentes)
        {
            whereConditions.Add("(VALOR_RECEBIDO < VALOR OR VALOR_RECEBIDO IS NULL)");
        }

        var whereClause = $"WHERE {string.Join(" AND ", whereConditions)}";

        var sql = $@"
            SELECT 
                NUMERO AS Numero,
                ORDEM AS Ordem,
                CODIGO AS Codigo,
                TIPO AS Tipo,
                MODELO AS Modelo,
                SERIE AS Serie,
                SUBSERIE AS Subserie,
                ORIGEM AS Origem,
                HISTORICO AS Historico,
                EMISSAO AS Emissao,
                VENCIMENTO AS Vencimento,
                VALOR AS Valor,
                RECEBIMENTO AS Recebimento,
                VALOR_RECEBIDO AS ValorRecebido,
                ACRESCIMO AS Acrescimo,
                DESCONTO AS Desconto,
                FIXO AS Fixo,
                JUROS AS Juros,
                CONTROLE AS Controle,
                OPERADOR AS Operador,
                ESPECIE AS Especie,
                BANCO AS Banco,
                CONTA AS Conta,
                CUSTO AS Custo,
                AG_CONTA AS AgConta,
                AG_BANCO AS AgBanco,
                QUITADONOECF AS QuitadoNoEcf,
                DINHEIRO AS Dinheiro,
                CHEQUE AS Cheque,
                CARTAO AS Cartao,
                BOLETO AS Boleto,
                TROCO AS Troco,
                ID_VENDEDOR AS IdVendedor,
                NUMCHQ AS NumChq,
                CODPROF AS CodProf,
                CARTAOD AS CartaoD,
                VALOR_ORIGINAL AS ValorOriginal,
                PIX AS Pix,
                HORA_RECEB AS HoraReceb,
                IMPRESSO AS Impresso,
                REMESSA AS Remessa,
                QRCODE AS Qrcode,
                CONTROLE_NOTA AS ControleNota,
                TERMINAL AS Terminal,
                NOTAFISCAL AS NotaFiscal
            FROM RECEBER
            {whereClause}
            ORDER BY VENCIMENTO ASC";

        _logger.LogInformation("🔍 Buscando contas a receber do cliente {CodigoCliente} - Apenas pendentes: {ApenasPendentes}",
            codigoCliente, apenasPendentes);

        var receber = await connection.QueryAsync<Receber>(sql, parameters);
        return receber;
    }

    public async Task<bool> QuitarReceberAsync(string numero, string ordem, decimal valorRecebido, DateTime dataRecebimento, int? operador = null, IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Buscar a conta atual
            var conta = await GetReceberPorNumeroOrdemAsync(numero, ordem);
            if (conta == null)
            {
                throw new Exception($"Conta a receber {numero}/{ordem} não encontrada");
            }

            // Calcular novo valor recebido
            var novoValorRecebido = conta.ValorRecebido + valorRecebido;

            // Validar se não está tentando receber mais que o valor da conta
            if (novoValorRecebido > conta.Valor)
            {
                throw new InvalidOperationException(
                    $"Valor a receber (R$ {valorRecebido:N2}) excede o valor pendente (R$ {conta.Valor - conta.ValorRecebido:N2})");
            }

            // Se for quitação total, atualizar data de recebimento
            DateTime? dataRecebimentoFinal = null;
            if (novoValorRecebido >= conta.Valor)
            {
                dataRecebimentoFinal = dataRecebimento;
            }
            else if (conta.Recebimento == null)
            {
                // Primeiro recebimento parcial
                dataRecebimentoFinal = dataRecebimento;
            }
            else
            {
                // Recebimento parcial adicional, manter data original
                dataRecebimentoFinal = conta.Recebimento;
            }

            var sql = @"
                UPDATE RECEBER
                SET VALOR_RECEBIDO = @ValorRecebido,
                    RECEBIMENTO = @Recebimento,
                    HORA_RECEB = @HoraReceb,
                    OPERADOR = COALESCE(@Operador, OPERADOR)
                WHERE NUMERO = @Numero AND ORDEM = @Ordem";

            _logger.LogInformation("💰 Quitando conta a receber - Numero: {Numero}, Ordem: {Ordem}, Valor Recebido: {ValorRecebido}, Novo Total: {NovoTotal}",
                numero, ordem, valorRecebido, novoValorRecebido);

            var linhasAfetadas = await connection.ExecuteAsync(sql, new
            {
                Numero = numero,
                Ordem = ordem,
                ValorRecebido = novoValorRecebido,
                Recebimento = dataRecebimentoFinal,
                HoraReceb = dataRecebimento.TimeOfDay,
                Operador = operador
            }, transaction: transaction);

            if (linhasAfetadas > 0)
            {
                _logger.LogInformation("✅ Conta a receber {Numero}/{Ordem} quitada com sucesso - Valor Recebido: {ValorRecebido}, Total Recebido: {NovoTotal}",
                    numero, ordem, valorRecebido, novoValorRecebido);
                return true;
            }

            throw new Exception("Erro ao quitar conta a receber");
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<(bool TemContasAberto, decimal ValorTotalPendente, int QuantidadeContas)> VerificarContasAbertoAsync(int codigoCliente)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                COUNT(*) AS QuantidadeContas,
                COALESCE(SUM(VALOR - VALOR_RECEBIDO), 0) AS ValorTotalPendente
            FROM RECEBER
            WHERE CODIGO = @CodigoCliente
              AND TIPO = 'C'
              AND (VALOR - VALOR_RECEBIDO) > 0";

        var resultado = await connection.QueryFirstOrDefaultAsync<(int QuantidadeContas, decimal ValorTotalPendente)>(sql, new { CodigoCliente = codigoCliente });

        var quantidadeContas = resultado.QuantidadeContas;
        var valorTotalPendente = resultado.ValorTotalPendente;

        _logger.LogInformation("🔍 Verificando contas em aberto do cliente {CodigoCliente} - Quantidade: {Quantidade}, Valor: {Valor}",
            codigoCliente, quantidadeContas, valorTotalPendente);

        return (quantidadeContas > 0, valorTotalPendente, quantidadeContas);
    }
}

