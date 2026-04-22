using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class RelatorioRepository : IRelatorioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<RelatorioRepository> _logger;

    public RelatorioRepository(IDbConnectionFactory connectionFactory, ILogger<RelatorioRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<RelatorioClienteDto> GetRelatorioClienteAsync(int codigoCliente, DateTime? dataInicio = null, DateTime? dataFim = null, string? origem = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereClause = "WHERE V.CLIENTE = @CodigoCliente AND V.LANCADO = 'EFETIVADO'";
        var parameters = new DynamicParameters();
        parameters.Add("@CodigoCliente", codigoCliente);

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

        // Buscar dados do cliente (tabela CLIENTES, coluna CPFCNPJ)
        var sqlCliente = @"
            SELECT 
                ID,
                NOME,
                CPFCNPJ,
                TELEFONE
            FROM CLIENTES
            WHERE ID = @CodigoCliente";

        var cliente = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlCliente, new { CodigoCliente = codigoCliente });

        // Buscar compras do cliente (comandas e delivery)
        var sqlCompras = $@"
            SELECT 
                V.NOTA AS Nota,
                V.DATA_SAIDA AS Data,
                V.HORA_SAIDA AS Hora,
                V.TOTAL AS Total,
                V.LANCADO AS Status,
                V.MESA AS Mesa,
                V.COMANDA AS Comanda,
                V.FORMAS_PGTO AS FormaPagamento,
                V.ORIGEM AS Origem,
                (SELECT COUNT(*) FROM ITEVENDAS I WHERE I.NOTA = V.NOTA) AS QuantidadeItens
            FROM VENDAS V
            {whereClause}
            ORDER BY V.DATA_SAIDA DESC, V.HORA_SAIDA DESC";

        var compras = await connection.QueryAsync<CompraClienteDto>(sqlCompras, parameters);

        var comprasList = compras.ToList();
        var totalCompras = comprasList.Count;
        var valorTotal = comprasList.Sum(c => c.Total);
        var ticketMedio = totalCompras > 0 ? valorTotal / totalCompras : 0;
        var primeiraCompra = comprasList.OrderBy(c => c.Data).FirstOrDefault()?.Data;
        var ultimaCompra = comprasList.OrderByDescending(c => c.Data).FirstOrDefault()?.Data;

        _logger.LogInformation("📊 Relatório cliente {CodigoCliente} - Total compras: {Total}, Valor: {Valor}", 
            codigoCliente, totalCompras, valorTotal);

        return new RelatorioClienteDto
        {
            CodigoCliente = codigoCliente,
            NomeCliente = cliente?.NOME?.ToString() ?? "Cliente não encontrado",
            CpfCnpj = cliente?.CPFCNPJ?.ToString(),
            Telefone = cliente?.TELEFONE?.ToString(),
            TotalCompras = totalCompras,
            ValorTotalPago = valorTotal,
            TicketMedio = ticketMedio,
            PrimeiraCompra = primeiraCompra,
            UltimaCompra = ultimaCompra,
            Compras = comprasList
        };
    }

    public async Task<RelatorioVendasDto> GetRelatorioVendasAsync(DateTime data, string? origem = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT 
                V.NOTA AS Nota,
                V.DATA_SAIDA AS Data,
                V.HORA_SAIDA AS Hora,
                V.CLIENTE AS Cliente,
                C.NOME AS NomeCliente,
                V.TOTAL AS Total,
                V.FORMAS_PGTO AS FormaPagamento,
                V.OPERADOR AS Operador,
                V.MESA AS Mesa,
                V.COMANDA AS Comanda,
                V.ORIGEM AS Origem,
                (SELECT COUNT(*) FROM ITEVENDAS I WHERE I.NOTA = V.NOTA) AS QuantidadeItens
            FROM VENDAS V
            LEFT JOIN CLIENTES C ON C.ID = V.CLIENTE
            WHERE V.DATA_SAIDA = @Data
              AND V.LANCADO = 'EFETIVADO'
            ORDER BY V.HORA_SAIDA DESC";

        var vendas = await connection.QueryAsync<VendaResumoDto>(sql, new { Data = data.Date });
        var vendasList = vendas.ToList();

        var vendasBa = vendasList.Where(v => string.Equals((v.Origem ?? "BA").Trim(), "BA", StringComparison.OrdinalIgnoreCase)).ToList();
        var vendasDl = vendasList.Where(v => string.Equals((v.Origem ?? "").Trim(), "DL", StringComparison.OrdinalIgnoreCase)).ToList();

        // Buscar totais por forma de pagamento (comandas + delivery)
        var sqlTotais = @"
            SELECT 
                COALESCE(SUM(V.DINHEIRO), 0) AS TotalDinheiro,
                COALESCE(SUM(V.CARTAO + V.CARTAOD), 0) AS TotalCartao,
                COALESCE(SUM(V.PIX), 0) AS TotalPix,
                COALESCE(SUM(V.CHEQUE), 0) AS TotalCheque,
                COALESCE(SUM(V.BOLETO), 0) AS TotalBoleto,
                COALESCE(SUM((SELECT COUNT(*) FROM ITEVENDAS I WHERE I.NOTA = V.NOTA)), 0) AS TotalItens
            FROM VENDAS V
            WHERE V.DATA_SAIDA = @Data
              AND V.LANCADO = 'EFETIVADO'";

        var totais = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlTotais, new { Data = data.Date });

        var totalVendas = vendasList.Count;
        var valorTotal = vendasList.Sum(v => v.Total);
        var ticketMedio = totalVendas > 0 ? valorTotal / totalVendas : 0;

        _logger.LogInformation("📊 Relatório vendas {Data} - Total: {Total}, Valor: {Valor}", 
            data, totalVendas, valorTotal);

        return new RelatorioVendasDto
        {
            Data = data,
            TotalVendas = totalVendas,
            ValorTotal = valorTotal,
            TicketMedio = ticketMedio,
            TotalItens = totais?.TotalItens ?? 0,
            TotalDinheiro = totais?.TotalDinheiro ?? 0,
            TotalCartao = totais?.TotalCartao ?? 0,
            TotalPix = totais?.TotalPix ?? 0,
            TotalCheque = totais?.TotalCheque ?? 0,
            TotalBoleto = totais?.TotalBoleto ?? 0,
            TotalVendasComandas = vendasBa.Count,
            ValorTotalComandas = vendasBa.Sum(v => v.Total),
            TotalVendasDelivery = vendasDl.Count,
            ValorTotalDelivery = vendasDl.Sum(v => v.Total),
            Vendas = vendasList
        };
    }

    public async Task<RelatorioVendasDto> GetRelatorioVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var dataFimLimite = dataFim.Date.AddDays(1).AddSeconds(-1);

        var sql = @"
            SELECT 
                V.NOTA AS Nota,
                V.DATA_SAIDA AS Data,
                V.HORA_SAIDA AS Hora,
                V.CLIENTE AS Cliente,
                C.NOME AS NomeCliente,
                V.TOTAL AS Total,
                V.FORMAS_PGTO AS FormaPagamento,
                V.OPERADOR AS Operador,
                V.MESA AS Mesa,
                V.COMANDA AS Comanda,
                V.ORIGEM AS Origem,
                (SELECT COUNT(*) FROM ITEVENDAS I WHERE I.NOTA = V.NOTA) AS QuantidadeItens
            FROM VENDAS V
            LEFT JOIN CLIENTES C ON C.ID = V.CLIENTE
            WHERE V.DATA_SAIDA >= @DataInicio
              AND V.DATA_SAIDA <= @DataFim
              AND V.LANCADO = 'EFETIVADO'
            ORDER BY V.DATA_SAIDA DESC, V.HORA_SAIDA DESC";

        var vendas = await connection.QueryAsync<VendaResumoDto>(sql, new
        {
            DataInicio = dataInicio.Date,
            DataFim = dataFimLimite
        });
        var vendasList = vendas.ToList();

        var vendasBa = vendasList.Where(v => string.Equals((v.Origem ?? "BA").Trim(), "BA", StringComparison.OrdinalIgnoreCase)).ToList();
        var vendasDl = vendasList.Where(v => string.Equals((v.Origem ?? "").Trim(), "DL", StringComparison.OrdinalIgnoreCase)).ToList();

        var sqlTotais = @"
            SELECT 
                COALESCE(SUM(V.DINHEIRO), 0) AS TotalDinheiro,
                COALESCE(SUM(V.CARTAO + V.CARTAOD), 0) AS TotalCartao,
                COALESCE(SUM(V.PIX), 0) AS TotalPix,
                COALESCE(SUM(V.CHEQUE), 0) AS TotalCheque,
                COALESCE(SUM(V.BOLETO), 0) AS TotalBoleto,
                COALESCE(SUM((SELECT COUNT(*) FROM ITEVENDAS I WHERE I.NOTA = V.NOTA)), 0) AS TotalItens
            FROM VENDAS V
            WHERE V.DATA_SAIDA >= @DataInicio
              AND V.DATA_SAIDA <= @DataFim
              AND V.LANCADO = 'EFETIVADO'";

        var totais = await connection.QueryFirstOrDefaultAsync<dynamic>(sqlTotais, new
        {
            DataInicio = dataInicio.Date,
            DataFim = dataFimLimite
        });

        var totalVendas = vendasList.Count;
        var valorTotal = vendasList.Sum(v => v.Total);
        var ticketMedio = totalVendas > 0 ? valorTotal / totalVendas : 0;

        _logger.LogInformation("📊 Relatório vendas período {DataInicio} a {DataFim} - Total: {Total}, Valor: {Valor}",
            dataInicio, dataFim, totalVendas, valorTotal);

        return new RelatorioVendasDto
        {
            Data = dataInicio,
            TotalVendas = totalVendas,
            ValorTotal = valorTotal,
            TicketMedio = ticketMedio,
            TotalItens = (int)(totais?.TotalItens ?? 0),
            TotalDinheiro = (decimal)(totais?.TotalDinheiro ?? 0),
            TotalCartao = (decimal)(totais?.TotalCartao ?? 0),
            TotalPix = (decimal)(totais?.TotalPix ?? 0),
            TotalCheque = (decimal)(totais?.TotalCheque ?? 0),
            TotalBoleto = (decimal)(totais?.TotalBoleto ?? 0),
            TotalVendasComandas = vendasBa.Count,
            ValorTotalComandas = vendasBa.Sum(v => v.Total),
            TotalVendasDelivery = vendasDl.Count,
            ValorTotalDelivery = vendasDl.Sum(v => v.Total),
            Vendas = vendasList
        };
    }

    public async Task<RelatorioProdutosMaisVendidosDto> GetProdutosMaisVendidosAsync(DateTime dataInicio, DateTime dataFim, int top = 10, string? origem = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var dataFimLimite = dataFim.Date.AddDays(1).AddSeconds(-1);
        var origemFiltro = string.IsNullOrWhiteSpace(origem) ? null : origem.Trim().ToUpperInvariant();

        var sql = @"
            SELECT FIRST @Top
                I.CODIGO AS Codigo,
                P.DESCRICAO AS Descricao,
                SUM(I.QTD) AS QuantidadeVendida,
                SUM(I.TOTAL) AS ValorTotal,
                AVG(I.PRECO) AS ValorMedio,
                COUNT(DISTINCT I.NOTA) AS NumeroVendas
            FROM ITEVENDAS I
            INNER JOIN VENDAS V ON V.NOTA = I.NOTA
            INNER JOIN PRODUTOESERVICO P ON P.ID = I.CODIGO
            WHERE V.DATA_SAIDA >= @DataInicio
              AND V.DATA_SAIDA <= @DataFim
              AND V.LANCADO = 'EFETIVADO'
              AND (V.ORIGEM = @OrigemFiltro OR @OrigemFiltro IS NULL)
            GROUP BY I.CODIGO, P.DESCRICAO
            ORDER BY SUM(I.QTD) DESC";

        var produtos = await connection.QueryAsync<ProdutoVendidoDto>(sql, new 
        { 
            DataInicio = dataInicio.Date, 
            DataFim = dataFimLimite,
            Top = top,
            OrigemFiltro = origemFiltro
        });

        var produtosList = produtos.Select((p, index) => 
        {
            p.Posicao = index + 1;
            return p;
        }).ToList();

        _logger.LogInformation("📊 Produtos mais vendidos - Período: {DataInicio} a {DataFim}, Top: {Top}", 
            dataInicio, dataFim, top);

        return new RelatorioProdutosMaisVendidosDto
        {
            DataInicio = dataInicio,
            DataFim = dataFim,
            Produtos = produtosList
        };
    }

    public async Task<RelatorioPeriodoDto> GetRelatorioPeriodoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Buscar itens vendidos no período (comandas e delivery)
        var sqlItens = @"
            SELECT 
                I.NOTA AS Nota,
                V.DATA_SAIDA AS Emissao,
                V.HORA_SAIDA AS Hora,
                I.ITEM AS Item,
                I.CODIGO AS CodigoProduto,
                P.DESCRICAO AS DescricaoProduto,
                I.BARRAS AS Barras,
                I.UND AS Und,
                I.QTD AS Qtd,
                I.PRECO AS Preco,
                I.DESCONTO AS Desconto,
                I.ACRESCIMO AS Acrescimo,
                I.TOTAL AS Total,
                V.CLIENTE AS Cliente,
                C.NOME AS NomeCliente,
                V.MESA AS Mesa,
                V.COMANDA AS Comanda,
                V.ORIGEM AS Origem
            FROM itevendas I
            INNER JOIN VENDAS V ON V.NOTA = I.NOTA
            LEFT JOIN PRODUTOESERVICO P ON P.ID = I.CODIGO
            LEFT JOIN CLIENTES C ON C.ID = V.CLIENTE
            WHERE V.DATA_SAIDA >= @DataInicio
              AND V.DATA_SAIDA <= @DataFim
              AND V.LANCADO = 'EFETIVADO'
              AND CAST(I.CANCELADO AS INTEGER) = 0
            ORDER BY V.DATA_SAIDA DESC, V.HORA_SAIDA DESC, I.ITEM";

        var itens = await connection.QueryAsync<ItemVendidoDto>(sqlItens, new 
        { 
            DataInicio = dataInicio.Date, 
            DataFim = dataFim.Date.AddDays(1).AddSeconds(-1)
        });

        var itensList = itens.ToList();

        // Buscar recebimentos por período
        var sqlRecebimentos = @"
            SELECT 
                RV.ID AS Id,
                RV.ID_FORMA_PAGAMENTO AS IdFormaPagamento,
                FP.DESCRICAO AS FormaPagamento,
                RV.N_CAIXA AS NCaixa,
                RV.NOTA AS Nota,
                RV.VALOR AS Valor,
                RV.TROCO AS Troco,
                V.DATA_SAIDA AS DataVenda,
                V.HORA_SAIDA AS HoraVenda,
                V.CLIENTE AS Cliente,
                C.NOME AS NomeCliente
            FROM RECEBIMENTO_VENDAS RV
            INNER JOIN VENDAS V ON V.NOTA = RV.NOTA
            LEFT JOIN FORMA_PAGAMENTO FP ON FP.ID = RV.ID_FORMA_PAGAMENTO
            LEFT JOIN CLIENTES C ON C.ID = V.CLIENTE
            WHERE V.DATA_SAIDA >= @DataInicio
              AND V.DATA_SAIDA <= @DataFim
              AND V.LANCADO = 'EFETIVADO'
            ORDER BY FP.DESCRICAO, V.DATA_SAIDA DESC, V.HORA_SAIDA DESC";

        var recebimentos = await connection.QueryAsync<dynamic>(sqlRecebimentos, new 
        { 
            DataInicio = dataInicio.Date, 
            DataFim = dataFim.Date.AddDays(1).AddSeconds(-1)
        });

        var recebimentosList = recebimentos.ToList();

        // Agrupar recebimentos por forma de pagamento
        var recebimentosPorForma = recebimentosList
            .GroupBy(r => new { 
                IdFormaPagamento = (int)r.IdFormaPagamento, 
                FormaPagamento = r.FormaPagamento?.ToString() ?? "N/A" 
            })
            .Select(g => new RecebimentoPorFormaPagamentoDto
            {
                IdFormaPagamento = g.Key.IdFormaPagamento,
                FormaPagamento = g.Key.FormaPagamento,
                Quantidade = g.Count(),
                ValorTotal = g.Sum(r => (decimal)r.Valor),
                TrocoTotal = g.Sum(r => (decimal)(r.Troco ?? 0)),
                Detalhes = g.Select(r => new RecebimentoDetalheDto
                {
                    Id = (int)r.Id,
                    Nota = r.Nota?.ToString() ?? string.Empty,
                    DataVenda = (DateTime)r.DataVenda,
                    HoraVenda = (TimeSpan)r.HoraVenda,
                    Valor = (decimal)r.Valor,
                    Troco = (decimal)(r.Troco ?? 0),
                    NCaixa = (int)r.NCaixa,
                    Cliente = (int)(r.Cliente ?? 0),
                    NomeCliente = r.NomeCliente?.ToString()
                }).ToList()
            })
            .OrderBy(r => r.FormaPagamento)
            .ToList();

        var itensBa = itensList.Where(i => string.Equals((i.Origem ?? "BA").Trim(), "BA", StringComparison.OrdinalIgnoreCase)).ToList();
        var itensDl = itensList.Where(i => string.Equals((i.Origem ?? "").Trim(), "DL", StringComparison.OrdinalIgnoreCase)).ToList();

        // Calcular resumo
        var resumo = new ResumoRelatorioPeriodoDto
        {
            TotalItensVendidos = itensList.Count,
            ValorTotalItens = itensList.Sum(i => i.Total),
            TotalDesconto = itensList.Sum(i => i.Desconto),
            TotalAcrescimo = itensList.Sum(i => i.Acrescimo),
            TotalRecebimentos = recebimentosList.Count,
            ValorTotalRecebimentos = recebimentosPorForma.Sum(r => r.ValorTotal),
            ValorTotalTroco = recebimentosPorForma.Sum(r => r.TrocoTotal),
            ValorTotalComandas = itensBa.Sum(i => i.Total),
            ValorTotalDelivery = itensDl.Sum(i => i.Total),
            TotalPorFormaPagamento = recebimentosPorForma.ToDictionary(
                r => r.FormaPagamento, 
                r => r.ValorTotal
            )
        };

        _logger.LogInformation("📊 Relatório período - {DataInicio} a {DataFim} - Itens: {Itens}, Recebimentos: {Recebimentos}", 
            dataInicio, dataFim, itensList.Count, recebimentosList.Count);

        return new RelatorioPeriodoDto
        {
            DataInicio = dataInicio,
            DataFim = dataFim,
            ItensVendidos = itensList,
            RecebimentosPorFormaPagamento = recebimentosPorForma,
            Resumo = resumo
        };
    }

    public async Task<RelatorioCaixaConsolidadoDto> GetRelatorioCaixaConsolidadoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var dataFimFim = dataFim.Date.AddDays(1).AddSeconds(-1);
        var parameters = new DynamicParameters();
        parameters.Add("@DataInicio", dataInicio.Date);
        parameters.Add("@DataFim", dataFimFim);

        // 1. Aberturas de caixa (ORIGEM = 'ABERTURA')
        var sqlAberturas = @"
            SELECT CODIGO AS Codigo, DATA AS Data, HORA AS Hora, TERMINAL AS Terminal,
                   ENTRADA AS Entrada, SAIDA AS Saida, ORIGEM AS Origem, HISTORICO AS Historico
            FROM CAIXA
            WHERE ORIGEM = 'ABERTURA' AND DATA >= @DataInicio AND DATA <= @DataFim
            ORDER BY DATA, HORA, CODIGO";
        var aberturas = (await connection.QueryAsync<MovimentoCaixaDto>(sqlAberturas, parameters)).ToList();

        // 2. Vendas fechadas no período (comandas e delivery)
        var sqlVendas = @"
            SELECT V.NOTA AS Nota, V.DATA_SAIDA AS Data, V.HORA_SAIDA AS Hora,
                   V.COMANDA AS Comanda, V.MESA AS Mesa, V.CLIENTE AS Cliente,
                   C.NOME AS NomeCliente, V.TOTAL AS Total, V.CAIXA AS NCaixa, V.ORIGEM AS Origem
            FROM VENDAS V
            LEFT JOIN CLIENTES C ON C.ID = V.CLIENTE
            WHERE V.DATA_SAIDA >= @DataInicio AND V.DATA_SAIDA <= @DataFim
              AND V.LANCADO = 'EFETIVADO'
            ORDER BY V.DATA_SAIDA DESC, V.HORA_SAIDA DESC";
        var vendas = (await connection.QueryAsync<VendaResumoCaixaDto>(sqlVendas, parameters)).ToList();

        var vendasBa = vendas.Where(v => string.Equals((v.Origem ?? "BA").Trim(), "BA", StringComparison.OrdinalIgnoreCase)).ToList();
        var vendasDl = vendas.Where(v => string.Equals((v.Origem ?? "").Trim(), "DL", StringComparison.OrdinalIgnoreCase)).ToList();

        // 3. Recebimentos de vendas (pagamentos ao fechar comanda - comandas e delivery)
        var sqlRecebVendas = @"
            SELECT RV.ID AS Id, RV.NOTA AS Nota, V.DATA_SAIDA AS DataVenda,
                   FP.DESCRICAO AS FormaPagamento, RV.VALOR AS Valor, RV.TROCO AS Troco, RV.N_CAIXA AS NCaixa
            FROM RECEBIMENTO_VENDAS RV
            INNER JOIN VENDAS V ON V.NOTA = RV.NOTA
            LEFT JOIN FORMA_PAGAMENTO FP ON FP.ID = RV.ID_FORMA_PAGAMENTO
            WHERE V.DATA_SAIDA >= @DataInicio AND V.DATA_SAIDA <= @DataFim
              AND V.LANCADO = 'EFETIVADO'
            ORDER BY V.DATA_SAIDA DESC, RV.ID";
        var recebimentosVendas = (await connection.QueryAsync<RecebimentoVendaResumoDto>(sqlRecebVendas, parameters)).ToList();

        // 4. Recebimentos de contas a receber (RECEBER quitado no período) – separado de vendas
        var sqlRecebContas = @"
            SELECT NUMERO AS Numero, ORDEM AS Ordem, RECEBIMENTO AS DataRecebimento,
                   VALOR_RECEBIDO AS ValorRecebido, HISTORICO AS Historico, TERMINAL AS Terminal,
                   COALESCE(NULLIF(TRIM(ESPECIE), ''), 'OUTROS') AS FormaPagamento
            FROM RECEBER
            WHERE RECEBIMENTO IS NOT NULL AND VALOR_RECEBIDO > 0
              AND CAST(RECEBIMENTO AS DATE) >= CAST(@DataInicio AS DATE)
              AND CAST(RECEBIMENTO AS DATE) <= CAST(@DataFim AS DATE)
            ORDER BY RECEBIMENTO DESC";
        var recebimentosContas = (await connection.QueryAsync<RecebimentoContaReceberDto>(sqlRecebContas, parameters)).ToList();

        // 5. Saídas de caixa
        var sqlSaidas = @"
            SELECT CODIGO AS Codigo, DATA AS Data, HORA AS Hora, TERMINAL AS Terminal,
                   ENTRADA AS Entrada, SAIDA AS Saida, ORIGEM AS Origem, HISTORICO AS Historico
            FROM CAIXA
            WHERE SAIDA > 0 AND DATA >= @DataInicio AND DATA <= @DataFim
            ORDER BY DATA DESC, HORA DESC, CODIGO DESC";
        var saidas = (await connection.QueryAsync<MovimentoCaixaDto>(sqlSaidas, parameters)).ToList();

        // Total por espécie (forma de pagamento) – Receb. Vendas
        var totalPorFormaVendas = recebimentosVendas
            .GroupBy(r => string.IsNullOrWhiteSpace(r.FormaPagamento) ? "OUTROS" : r.FormaPagamento.Trim())
            .Select(g => new TotalPorFormaDto { Forma = g.Key, Valor = g.Sum(x => x.Valor) })
            .OrderByDescending(x => x.Valor)
            .ToList();

        // Total por espécie – Receb. Contas (contas a receber quitadas)
        var totalPorFormaContas = recebimentosContas
            .GroupBy(r => string.IsNullOrWhiteSpace(r.FormaPagamento) ? "OUTROS" : r.FormaPagamento.Trim())
            .Select(g => new TotalPorFormaDto { Forma = g.Key, Valor = g.Sum(x => x.ValorRecebido) })
            .OrderByDescending(x => x.Valor)
            .ToList();

        var resumo = new ResumoCaixaConsolidadoDto
        {
            TotalAbertura = aberturas.Sum(a => a.Entrada),
            TotalVendas = vendas.Sum(v => v.Total),
            TotalRecebimentosVendas = recebimentosVendas.Sum(r => r.Valor),
            TotalRecebimentosContasReceber = recebimentosContas.Sum(r => r.ValorRecebido),
            TotalSaidas = saidas.Sum(s => s.Saida),
            QuantidadeAberturas = aberturas.Count,
            QuantidadeVendas = vendas.Count,
            QuantidadeVendasComandas = vendasBa.Count,
            TotalVendasComandas = vendasBa.Sum(v => v.Total),
            QuantidadeVendasDelivery = vendasDl.Count,
            TotalVendasDelivery = vendasDl.Sum(v => v.Total),
            QuantidadeRecebimentosVendas = recebimentosVendas.Count,
            QuantidadeRecebimentosContasReceber = recebimentosContas.Count,
            QuantidadeSaidas = saidas.Count,
            TotalPorFormaRecebimentosVendas = totalPorFormaVendas,
            TotalPorFormaRecebimentosContas = totalPorFormaContas
        };

        _logger.LogInformation("📊 Relatório caixa consolidado - {DataInicio} a {DataFim} - Aberturas: {A}, Vendas: {V}, Receb: {R}, Saídas: {S}",
            dataInicio, dataFim, aberturas.Count, vendas.Count, recebimentosVendas.Count + recebimentosContas.Count, saidas.Count);

        return new RelatorioCaixaConsolidadoDto
        {
            DataInicio = dataInicio,
            DataFim = dataFim,
            Aberturas = aberturas,
            Vendas = vendas,
            RecebimentosVendas = recebimentosVendas,
            RecebimentosContasReceber = recebimentosContas,
            Saidas = saidas,
            Resumo = resumo
        };
    }
}

