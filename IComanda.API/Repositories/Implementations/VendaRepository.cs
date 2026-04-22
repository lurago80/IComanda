using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class VendaRepository : IVendaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<VendaRepository> _logger;
    /// <summary>Se true, a coluna nome_cliente não existe na tabela vendas; usar SQL sem ela para evitar 500.</summary>
    internal static bool NomeClienteColumnMissing = false;

    public VendaRepository(IDbConnectionFactory connectionFactory, ILogger<VendaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    private static bool IsColumnMissingException(Exception ex)
    {
        var msg = ex.Message?.ToUpperInvariant() ?? "";
        return msg.Contains("NOME_CLIENTE") || msg.Contains("COLUMN") || msg.Contains("INVALID") || msg.Contains("DYNAMIC SQL");
    }

    public async Task<string> GerarProximaNotaAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = "SELECT GEN_ID(VENDAS_GEN, 1) AS GEN FROM RDB$DATABASE";
        var resultado = await connection.QuerySingleAsync<dynamic>(sql);
        return resultado.GEN.ToString().PadLeft(6, '0');
    }

    public async Task<int> GerarProximoNumeroComandaAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Buscar o maior número de comanda e incrementar
        // Considera apenas comandas que realmente existem (não null e > 0)
        var sql = @"
            SELECT COALESCE(MAX(comanda), 0) + 1 AS PROXIMA_COMANDA
            FROM vendas
            WHERE comanda IS NOT NULL AND comanda > 0";

        var proximaComanda = await connection.QuerySingleAsync<int>(sql);
        return proximaComanda > 0 ? proximaComanda : 1;
    }

    public async Task<bool> CriarVendaAsync(Venda venda, System.Data.IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        _logger.LogDebug("CriarVendaAsync: CFOPS={Cfops} Natureza={Natureza} Saida={Saida} Loja={Loja}", venda.Cfops, venda.Natureza, venda.Saida, venda.Loja);

        const string sqlComNomeCliente = @"
            INSERT INTO vendas (
                nota, modelo, serie, subserie, origem, emissao, hora, saida, cfops, natureza,
                cliente, data_saida, hora_saida, formas_pgto, tot_produtos, total, operador, 
                sequencia, avista, desconto, acrescimo, especie, loja, vale, 
                dinheiro, cheque, cartao, boleto, troco, quantidade, lancado, 
                vendedor, caixa, comanda, mesa, numero_pessoas, nome_cliente
            ) VALUES (
                @Nota, @Modelo, @Serie, @Subserie, @Origem, @Emissao, @Hora, @Saida, @Cfops, @Natureza,
                @Cliente, @DataSaida, @HoraSaida, @FormasPgto, @TotProdutos, @Total, @Operador,
                @Sequencia, @Avista, @Desconto, @Acrescimo, @Especie, @Loja, @Vale,
                @Dinheiro, @Cheque, @Cartao, @Boleto, @Troco, @Quantidade, @Lancado,
                @Vendedor, @Caixa, @Comanda, @Mesa, @NumeroPessoas, @NomeCliente
            )";
        const string sqlSemNomeCliente = @"
            INSERT INTO vendas (
                nota, modelo, serie, subserie, origem, emissao, hora, saida, cfops, natureza,
                cliente, data_saida, hora_saida, formas_pgto, tot_produtos, total, operador, 
                sequencia, avista, desconto, acrescimo, especie, loja, vale, 
                dinheiro, cheque, cartao, boleto, troco, quantidade, lancado, 
                vendedor, caixa, comanda, mesa, numero_pessoas
            ) VALUES (
                @Nota, @Modelo, @Serie, @Subserie, @Origem, @Emissao, @Hora, @Saida, @Cfops, @Natureza,
                @Cliente, @DataSaida, @HoraSaida, @FormasPgto, @TotProdutos, @Total, @Operador,
                @Sequencia, @Avista, @Desconto, @Acrescimo, @Especie, @Loja, @Vale,
                @Dinheiro, @Cheque, @Cartao, @Boleto, @Troco, @Quantidade, @Lancado,
                @Vendedor, @Caixa, @Comanda, @Mesa, @NumeroPessoas
            )";

        _logger.LogDebug("Salvando venda Nota={Nota} Status={Status}", venda.Nota, venda.Lancado);
        
        var parametrosComNome = new
        {
            venda.Nota,
            venda.Modelo,
            venda.Serie,
            venda.Subserie,
            venda.Origem,
            venda.Emissao,
            venda.Hora,
            venda.Saida,
            venda.Cfops,
            venda.Natureza,
            venda.Cliente,
            venda.DataSaida,
            venda.HoraSaida,
            venda.FormasPgto,
            venda.TotProdutos,
            venda.Total,
            venda.Operador,
            venda.Sequencia,
            venda.Avista,
            venda.Desconto,
            venda.Acrescimo,
            venda.Especie,
            venda.Loja,
            venda.Vale,
            venda.Dinheiro,
            venda.Cheque,
            venda.Cartao,
            venda.Boleto,
            venda.Troco,
            venda.Quantidade,
            venda.Lancado,
            venda.Vendedor,
            venda.Caixa,
            venda.Comanda,
            venda.Mesa,
            venda.NumeroPessoas,
            NomeCliente = venda.NomeCliente ?? (string?)null
        };
        var parametrosSemNome = new
        {
            venda.Nota,
            venda.Modelo,
            venda.Serie,
            venda.Subserie,
            venda.Origem,
            venda.Emissao,
            venda.Hora,
            venda.Saida,
            venda.Cfops,
            venda.Natureza,
            venda.Cliente,
            venda.DataSaida,
            venda.HoraSaida,
            venda.FormasPgto,
            venda.TotProdutos,
            venda.Total,
            venda.Operador,
            venda.Sequencia,
            venda.Avista,
            venda.Desconto,
            venda.Acrescimo,
            venda.Especie,
            venda.Loja,
            venda.Vale,
            venda.Dinheiro,
            venda.Cheque,
            venda.Cartao,
            venda.Boleto,
            venda.Troco,
            venda.Quantidade,
            venda.Lancado,
            venda.Vendedor,
            venda.Caixa,
            venda.Comanda,
            venda.Mesa,
            venda.NumeroPessoas
        };

        try
        {
            int linhasAfetadas;
            if (NomeClienteColumnMissing)
            {
                linhasAfetadas = await connection.ExecuteAsync(sqlSemNomeCliente, parametrosSemNome, transaction: transaction);
            }
            else
            {
                linhasAfetadas = await connection.ExecuteAsync(sqlComNomeCliente, parametrosComNome, transaction: transaction);
            }
            if (linhasAfetadas > 0)
            {
                _logger.LogInformation("Venda {Nota} salva com sucesso", venda.Nota);
                return true;
            }
            _logger.LogWarning("Nenhuma linha afetada ao salvar venda {Nota}", venda.Nota);
            return false;
        }
        catch (Exception ex) when (IsColumnMissingException(ex))
        {
            NomeClienteColumnMissing = true;
            _logger.LogWarning("Coluna nome_cliente inexistente; salvando sem ela. Execute Scripts/ADD_NOME_CLIENTE_VENDAS.sql para habilitar.");
            var linhasAfetadas = await connection.ExecuteAsync(sqlSemNomeCliente, parametrosSemNome, transaction: transaction);
            if (linhasAfetadas > 0)
                _logger.LogInformation("Venda {Nota} salva com sucesso (sem nome_cliente)", venda.Nota);
            return linhasAfetadas > 0;
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<bool> CriarItensVendaAsync(List<ItemVenda> itens)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            INSERT INTO itevendas (
                nota, modelo, serie, subserie, origem, emissao, item, codigo, 
                barras, cfop, st, und, qtd, preco, desconto, acrescimo, total, 
                cancelado, sequencia, preco_custo, serial, icms, sinalm
            ) VALUES (
                @Nota, @Modelo, @Serie, @Subserie, @Origem, @Emissao, @Item, @Codigo,
                @Barras, @Cfop, @St, @Und, @Qtd, @Preco, @Desconto, @Acrescimo, @Total,
                @Cancelado, @Sequencia, @PrecoCusto, @Serial, @Icms, @Sinalm
            )";

        var linhasAfetadas = await connection.ExecuteAsync(sql, itens);
        return linhasAfetadas > 0;
    }

    public async Task<bool> CriarItensVendaAsync(List<ItemVenda> itens, System.Data.IDbTransaction? transaction)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();

        var sql = @"
            INSERT INTO itevendas (
                nota, modelo, serie, subserie, origem, emissao, item, codigo, 
                barras, cfop, st, und, qtd, preco, desconto, acrescimo, total, 
                cancelado, sequencia, preco_custo, serial, icms, sinalm
            ) VALUES (
                @Nota, @Modelo, @Serie, @Subserie, @Origem, @Emissao, @Item, @Codigo,
                @Barras, @Cfop, @St, @Und, @Qtd, @Preco, @Desconto, @Acrescimo, @Total,
                @Cancelado, @Sequencia, @PrecoCusto, @Serial, @Icms, @Sinalm
            )";

        var linhasAfetadas = await connection.ExecuteAsync(sql, itens, transaction: transaction);
        return linhasAfetadas > 0;
    }

    public async Task<Venda?> GetVendaAsync(string nota)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Normalizar a nota - remover zeros à esquerda para comparação
        // O Firebird pode armazenar com ou sem zeros à esquerda
        var notaNormalizada = nota.TrimStart('0');
        if (string.IsNullOrEmpty(notaNormalizada))
        {
            notaNormalizada = "0";
        }

        // Preparar notas formatadas (com e sem zeros)
        var notaComZeros = nota.PadLeft(6, '0');
        var notaSemZeros = notaNormalizada;

        _logger.LogDebug("GetVendaAsync: Nota={Nota} ComZeros={NotaComZeros} SemZeros={NotaSemZeros}", nota, notaComZeros, notaSemZeros);

        var selectCols = NomeClienteColumnMissing ? SelectVendasBase : SelectVendasComNomeCliente;

        Venda? venda;
        try
        {
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @Nota AND lancado IN ('ABERTO', 'SAINDO')", new { Nota = nota });
        }
        catch (Exception ex) when (IsColumnMissingException(ex))
        {
            NomeClienteColumnMissing = true;
            selectCols = SelectVendasBase;
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @Nota AND lancado IN ('ABERTO', 'SAINDO')", new { Nota = nota });
        }

        if (venda != null)
            return venda;

        _logger.LogDebug("Venda não encontrada com nota original {Nota}; tentando variações", nota);

        if (nota != notaComZeros)
        {
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @NotaComZeros AND lancado IN ('ABERTO', 'SAINDO')", new { NotaComZeros = notaComZeros });
            if (venda != null)
                return venda;
        }

        if (nota != notaSemZeros)
        {
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @NotaSemZeros AND lancado IN ('ABERTO', 'SAINDO')", new { NotaSemZeros = notaSemZeros });
            if (venda != null)
                return venda;
        }

        venda = await connection.QueryFirstOrDefaultAsync<Venda>(
            selectCols + " FROM vendas WHERE TRIM(LEADING '0' FROM nota) = @NotaSemZeros AND lancado IN ('ABERTO', 'SAINDO')", new { NotaSemZeros = notaSemZeros });

        if (venda == null)
            _logger.LogDebug("Venda não encontrada após todas as tentativas: Nota={Nota}", nota);

        return venda;
    }

    public async Task<Venda?> GetVendaSemFiltroStatusAsync(string nota)
    {
        using var connection = _connectionFactory.CreateConnection();

        var notaNormalizada = nota.TrimStart('0');
        if (string.IsNullOrEmpty(notaNormalizada))
            notaNormalizada = "0";

        var notaComZeros = nota.PadLeft(6, '0');
        var notaSemZeros = notaNormalizada;

        var selectCols = NomeClienteColumnMissing ? SelectVendasBase : SelectVendasComNomeCliente;

        Venda? venda;
        try
        {
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @Nota", new { Nota = nota });
        }
        catch (Exception ex) when (IsColumnMissingException(ex))
        {
            NomeClienteColumnMissing = true;
            selectCols = SelectVendasBase;
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @Nota", new { Nota = nota });
        }

        if (venda != null) return venda;
        if (nota != notaComZeros)
        {
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @NotaComZeros", new { NotaComZeros = notaComZeros });
            if (venda != null) return venda;
        }
        if (nota != notaSemZeros)
        {
            venda = await connection.QueryFirstOrDefaultAsync<Venda>(
                selectCols + " FROM vendas WHERE nota = @NotaSemZeros", new { NotaSemZeros = notaSemZeros });
        }

        return venda;
    }

    public async Task<IEnumerable<ItemVenda>> GetItensVendaAsync(string nota)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Normalizar a nota (tentar com e sem zeros)
        var notaNormalizada = nota.TrimStart('0');
        if (string.IsNullOrEmpty(notaNormalizada))
        {
            notaNormalizada = "0";
        }
        var notaComZeros = nota.PadLeft(6, '0');

        var sql = @"
            SELECT i.nota, i.modelo, i.serie, i.subserie, i.origem, i.emissao, i.item, i.codigo,
                   i.barras, i.cfop, i.st, i.und, i.qtd, i.preco, i.desconto, i.acrescimo, i.total,
                   i.cancelado, i.sequencia, i.preco_custo, i.serial, i.icms, i.sinalm,
                   COALESCE(p3.DESCRICAO, '') as Descricao
            FROM itevendas i
            LEFT JOIN PRODUTOESERVICO p3 ON p3.ID = i.codigo
            WHERE (i.nota = @Nota OR i.nota = @NotaComZeros OR i.nota = @NotaSemZeros) 
              AND CAST(i.cancelado AS INTEGER) = 0
            ORDER BY i.item";

        var itens = await connection.QueryAsync<ItemVenda>(sql, new { 
            Nota = nota, 
            NotaComZeros = notaComZeros,
            NotaSemZeros = notaNormalizada
        });
        
        var itensList = itens.ToList();
        _logger.LogDebug("GetItensVendaAsync Nota={Nota}: {Count} itens", nota, itensList.Count);
        return itensList;
    }

    public async Task<IEnumerable<Venda>> GetVendasHojeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida, hora_saida, formas_pgto, tot_produtos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas
            FROM vendas 
            WHERE data_saida = CURRENT_DATE AND origem = 'BA'
            ORDER BY hora_saida DESC";

        return await connection.QueryAsync<Venda>(sql);
    }

    public async Task<IEnumerable<Venda>> GetVendasFechadasPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string> { "lancado = 'EFETIVADO'", "origem = 'BA'" };
        var parameters = new DynamicParameters();
        if (dataInicio.HasValue)
        {
            whereConditions.Add("data_saida >= @DataInicio");
            parameters.Add("DataInicio", dataInicio.Value.Date);
        }
        if (dataFim.HasValue)
        {
            whereConditions.Add("data_saida <= @DataFim");
            parameters.Add("DataFim", dataFim.Value.Date.AddDays(1).AddSeconds(-1));
        }
        var whereClause = string.Join(" AND ", whereConditions);

        var sql = $@"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida, hora_saida, formas_pgto, tot_produtos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas
            FROM vendas 
            WHERE {whereClause}
            ORDER BY data_saida DESC, hora_saida DESC";

        return await connection.QueryAsync<Venda>(sql, parameters);
    }

    public async Task<IEnumerable<Venda>> GetVendasPorComandaAsync(int comanda)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida, hora_saida, formas_pgto, tot_produtos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas
            FROM vendas 
            WHERE comanda = @Comanda AND origem = 'BA'
            ORDER BY data_saida DESC, hora_saida DESC";

        return await connection.QueryAsync<Venda>(sql, new { Comanda = comanda });
    }

    public async Task<IEnumerable<Venda>> GetVendasPorMesaAsync(int mesa)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida, hora_saida, formas_pgto, tot_produtos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas
            FROM vendas 
            WHERE mesa = @Mesa AND origem = 'BA'
            ORDER BY data_saida DESC, hora_saida DESC";

        return await connection.QueryAsync<Venda>(sql, new { Mesa = mesa });
    }

    public async Task<Venda?> GetVendaAbertaPorMesaAsync(int mesa)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida AS DataSaida, hora_saida AS HoraSaida, 
                   formas_pgto AS FormasPgto, tot_produtos AS TotProdutos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas AS NumeroPessoas
            FROM vendas 
            WHERE mesa = @Mesa AND lancado = 'ABERTO' AND origem = 'BA'
            ORDER BY data_saida DESC, hora_saida DESC
            ROWS 1";

        return await connection.QueryFirstOrDefaultAsync<Venda>(sql, new { Mesa = mesa });
    }

    public async Task<Venda?> GetVendaAbertaPorComandaAsync(int comanda)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida AS DataSaida, hora_saida AS HoraSaida, 
                   formas_pgto AS FormasPgto, tot_produtos AS TotProdutos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas AS NumeroPessoas
            FROM vendas 
            WHERE comanda = @Comanda AND lancado IN ('ABERTO', 'SAINDO') AND origem = 'BA'
            ORDER BY data_saida DESC, hora_saida DESC
            ROWS 1";

        return await connection.QueryFirstOrDefaultAsync<Venda>(sql, new { Comanda = comanda });
    }

    public async Task<Venda?> GetVendaAbertaPorNotaAsync(string nota)
    {
        if (string.IsNullOrWhiteSpace(nota)) return null;
        using var connection = _connectionFactory.CreateConnection();
        var notaNorm = nota.Trim().PadLeft(6, '0');
        var cols = NomeClienteColumnMissing
            ? "nota, modelo, serie, subserie, origem, emissao, hora, cliente, data_saida AS DataSaida, hora_saida AS HoraSaida, formas_pgto AS FormasPgto, tot_produtos AS TotProdutos, total, operador, sequencia, avista, desconto, acrescimo, especie, loja, vale, dinheiro, cheque, cartao, boleto, troco, quantidade, lancado, vendedor, caixa, comanda, mesa, numero_pessoas AS NumeroPessoas"
            : "nota, modelo, serie, subserie, origem, emissao, hora, cliente, data_saida AS DataSaida, hora_saida AS HoraSaida, formas_pgto AS FormasPgto, tot_produtos AS TotProdutos, total, operador, sequencia, avista, desconto, acrescimo, especie, loja, vale, dinheiro, cheque, cartao, boleto, troco, quantidade, lancado, vendedor, caixa, comanda, mesa, numero_pessoas AS NumeroPessoas, nome_cliente AS NomeCliente";
        var sql = $"SELECT {cols} FROM vendas WHERE nota = @Nota AND lancado IN ('ABERTO', 'SAINDO')";
        try
        {
            return await connection.QueryFirstOrDefaultAsync<Venda>(sql, new { Nota = notaNorm });
        }
        catch (Exception ex) when (IsColumnMissingException(ex))
        {
            NomeClienteColumnMissing = true;
            return await connection.QueryFirstOrDefaultAsync<Venda>(
                "SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente, data_saida AS DataSaida, hora_saida AS HoraSaida, formas_pgto AS FormasPgto, tot_produtos AS TotProdutos, total, operador, sequencia, avista, desconto, acrescimo, especie, loja, vale, dinheiro, cheque, cartao, boleto, troco, quantidade, lancado, vendedor, caixa, comanda, mesa, numero_pessoas AS NumeroPessoas FROM vendas WHERE nota = @Nota AND lancado IN ('ABERTO', 'SAINDO')",
                new { Nota = notaNorm });
        }
    }

    private const string SelectVendasBase = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida, hora_saida, formas_pgto, tot_produtos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas";
    // Alias sem aspas: Firebird retorna NOMECLIENTE; Dapper mapeia case-insensitive para NomeCliente
    private const string SelectVendasComNomeCliente = SelectVendasBase + ", nome_cliente AS NomeCliente";

    public async Task<IEnumerable<Venda>> GetVendasAbertasAsync(string? origem = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var origemFiltro = string.IsNullOrWhiteSpace(origem) ? "BA" : origem.Trim().ToUpperInvariant();
        var sqlWith = SelectVendasComNomeCliente + @"
            FROM vendas 
            WHERE lancado IN ('ABERTO', 'SAINDO') AND origem = @OrigemFiltro
            ORDER BY data_saida DESC, hora_saida DESC";
        var sqlWithout = SelectVendasBase + @"
            FROM vendas 
            WHERE lancado IN ('ABERTO', 'SAINDO') AND origem = @OrigemFiltro
            ORDER BY data_saida DESC, hora_saida DESC";
        try
        {
            if (NomeClienteColumnMissing)
                return await connection.QueryAsync<Venda>(sqlWithout, new { OrigemFiltro = origemFiltro });
            return await connection.QueryAsync<Venda>(sqlWith, new { OrigemFiltro = origemFiltro });
        }
        catch (Exception ex) when (IsColumnMissingException(ex))
        {
            NomeClienteColumnMissing = true;
            _logger.LogWarning("Coluna nome_cliente inexistente; use Scripts/ADD_NOME_CLIENTE_VENDAS.sql para habilitar.");
            return await connection.QueryAsync<Venda>(sqlWithout, new { OrigemFiltro = origemFiltro });
        }
    }

    public async Task<IEnumerable<Venda>> GetVendasCanceladasAsync(string? origem = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var origemFiltro = string.IsNullOrWhiteSpace(origem) ? "BA" : origem.Trim().ToUpperInvariant();
        var conditions = new List<string> { "lancado = 'CANCELADO'", "origem = @OrigemFiltro" };
        if (dataInicio.HasValue)
            conditions.Add("CAST(emissao AS DATE) >= @DataInicio");
        if (dataFim.HasValue)
            conditions.Add("CAST(emissao AS DATE) <= @DataFim");
        var whereClause = string.Join(" AND ", conditions);
        var sql = $@"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida, hora_saida, formas_pgto, tot_produtos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas
            FROM vendas 
            WHERE {whereClause}
            ORDER BY emissao DESC, hora DESC";
        var param = new { OrigemFiltro = origemFiltro, DataInicio = dataInicio?.Date, DataFim = dataFim?.Date };
        return await connection.QueryAsync<Venda>(sql, param);
    }

    public async Task<bool> AtualizarTotalVendaAsync(string nota, decimal totalProdutos, decimal total)
    {
        using var connection = _connectionFactory.CreateConnection();

        // Primeiro, buscar a venda para obter a nota exata como está no banco
        var vendaNoBanco = await GetVendaAsync(nota);
        if (vendaNoBanco == null)
        {
            _logger.LogWarning("Venda {Nota} não encontrada para atualizar total", nota);
            return false;
        }

        var notaExata = vendaNoBanco.Nota;
        _logger.LogDebug("AtualizarTotal: Nota={Nota} TotProdutos={TotProdutos} Total={Total}", notaExata, totalProdutos, total);

        // Atualizar usando a nota exata do banco (nota é única; BA e DL)
        var sql = @"
            UPDATE vendas 
            SET tot_produtos = @TotalProdutos, total = @Total
            WHERE nota = @Nota AND lancado = 'ABERTO'";

        var linhasAfetadas = await connection.ExecuteAsync(sql, new 
        { 
            Nota = notaExata, 
            TotalProdutos = totalProdutos, 
            Total = total 
        });

        if (linhasAfetadas > 0)
        {
            _logger.LogInformation("Total da venda {Nota} atualizado: R$ {Total}", notaExata, total);
        }
        else
        {
            _logger.LogWarning("Nenhuma linha atualizada para total da nota {Nota}", notaExata);
        }

        return linhasAfetadas > 0;
    }

    public async Task<bool> AtualizarStatusVendaAsync(string nota, string status, System.Data.IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            // Buscar a venda para obter a nota exata como está no banco
            var vendaNoBanco = await GetVendaSemFiltroStatusAsync(nota);
            if (vendaNoBanco == null)
            {
                _logger.LogWarning("Venda {Nota} não encontrada para atualizar status", nota);
                return false;
            }

            var notaExata = vendaNoBanco.Nota;
            _logger.LogDebug("AtualizarStatus: Nota={Nota} Status={Status}", notaExata, status);

            // Ao fechar a venda (EFETIVADO), gravar data e hora do fechamento para reimpressão (nota é única; BA e DL)
            var sql = status == "EFETIVADO"
                ? @"
                UPDATE vendas 
                SET lancado = @Status, data_saida = CURRENT_DATE, hora_saida = CURRENT_TIME
                WHERE nota = @Nota"
                : @"
                UPDATE vendas 
                SET lancado = @Status
                WHERE nota = @Nota";

            var linhasAfetadas = await connection.ExecuteAsync(sql, new
            {
                Nota = notaExata,
                Status = status
            }, transaction: transaction);

            if (linhasAfetadas > 0)
                _logger.LogInformation("Status da venda {Nota} atualizado para {Status}", notaExata, status);
            else
                _logger.LogWarning("Nenhuma linha atualizada para status da nota {Nota}", notaExata);

            return linhasAfetadas > 0;
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public async Task<string?> GetNomeClientePorNotaAsync(string nota)
    {
        if (string.IsNullOrWhiteSpace(nota)) return null;
        using var connection = _connectionFactory.CreateConnection();
        var notaComZeros = nota.Trim().PadLeft(6, '0');
        var notaSemZeros = nota.TrimStart('0');
        if (string.IsNullOrEmpty(notaSemZeros)) notaSemZeros = "0";
        try
        {
            var sql = "SELECT nome_cliente FROM vendas WHERE nota = @Nota";
            var nome = await connection.QueryFirstOrDefaultAsync<string>(sql, new { Nota = notaComZeros });
            if (!string.IsNullOrWhiteSpace(nome)) return nome.Trim();
            if (notaComZeros != notaSemZeros)
                nome = await connection.QueryFirstOrDefaultAsync<string>(sql, new { Nota = notaSemZeros });
            return string.IsNullOrWhiteSpace(nome) ? null : nome.Trim();
        }
        catch (Exception ex) when (IsColumnMissingException(ex))
        {
            return null;
        }
    }

    /// <summary>
    /// Cancela a venda gravando também a justificativa no campo JUSTIFICATIVA da tabela VENDAS.
    /// </summary>
    public async Task<bool> CancelarComComandaAsync(string nota, string justificativa, System.Data.IDbTransaction? transaction = null)
    {
        var connection = transaction?.Connection ?? _connectionFactory.CreateConnection();
        var shouldDispose = transaction == null;

        try
        {
            var vendaNoBanco = await GetVendaSemFiltroStatusAsync(nota);
            if (vendaNoBanco == null)
            {
                _logger.LogWarning("Venda {Nota} não encontrada para cancelar", nota);
                return false;
            }

            var notaExata = vendaNoBanco.Nota;

            // Tentar gravar justificativa; se o campo não existir na tabela, cancelar sem ele
            int linhasAfetadas;
            try
            {
                var sql = @"
                    UPDATE vendas
                    SET lancado = 'CANCELADO', justificativa = @Justificativa
                    WHERE nota = @Nota";
                linhasAfetadas = await connection.ExecuteAsync(sql,
                    new { Nota = notaExata, Justificativa = justificativa.Trim() },
                    transaction: transaction);
            }
            catch (Exception ex) when (ex.Message.ToUpperInvariant().Contains("JUSTIFICATIVA") ||
                                       ex.Message.ToUpperInvariant().Contains("COLUMN") ||
                                       ex.Message.ToUpperInvariant().Contains("INVALID"))
            {
                _logger.LogWarning("Coluna JUSTIFICATIVA inexistente; cancelando sem ela.");
                var sql = @"
                    UPDATE vendas
                    SET lancado = 'CANCELADO'
                    WHERE nota = @Nota";
                linhasAfetadas = await connection.ExecuteAsync(sql,
                    new { Nota = notaExata },
                    transaction: transaction);
            }

            _logger.LogInformation("Venda {Nota} cancelada. Linhas afetadas: {Linhas}", notaExata, linhasAfetadas);
            return linhasAfetadas > 0;
        }
        finally
        {
            if (shouldDispose && connection is IDisposable disposable)
                disposable.Dispose();
        }
    }

    /// <summary>
    /// Lê o campo SENHAD da tabela PARAMETROS — senha de cancelamento configurada no sistema.
    /// Retorna null se a tabela não existir ou o campo estiver vazio.
    /// </summary>
    public async Task<string?> GetSenhaCancelamentoAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        try
        {
            var sql = "SELECT FIRST 1 VALOR FROM PARAMETROS WHERE PARAMETRO = 'SENHAD'";
            var senha = await connection.QueryFirstOrDefaultAsync<string?>(sql);
            return string.IsNullOrWhiteSpace(senha) ? null : senha.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível ler SENHAD da tabela PARAMETROS.");
            return null;
        }
    }
}
