using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class VendaRepository : IVendaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VendaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string> GerarProximaNotaAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = "SELECT GEN_ID(VENDAS_GEN, 1) AS GEN FROM RDB$DATABASE";
        var resultado = await connection.QuerySingleAsync<dynamic>(sql);
        return resultado.GEN.ToString().PadLeft(6, '0');
    }

    public async Task<bool> CriarVendaAsync(Venda venda)
    {
        using var connection = _connectionFactory.CreateConnection();

        // DEBUG
        Console.WriteLine($"=== DEBUG VENDA ===");
        Console.WriteLine($"CFOPS: [{venda.Cfops}]");
        Console.WriteLine($"Natureza: [{venda.Natureza}]");
        Console.WriteLine($"Saida: [{venda.Saida}]");
        Console.WriteLine($"Loja: [{venda.Loja}]");
        Console.WriteLine($"==================");

        var sql = @"
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

        var parametros = new
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

        var linhasAfetadas = await connection.ExecuteAsync(sql, parametros);
        return linhasAfetadas > 0;
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

    public async Task<Venda?> GetVendaAsync(string nota)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, hora, cliente,
                   data_saida, hora_saida, formas_pgto, tot_produtos, total, operador,
                   sequencia, avista, desconto, acrescimo, especie, loja, vale,
                   dinheiro, cheque, cartao, boleto, troco, quantidade, lancado,
                   vendedor, caixa, comanda, mesa, numero_pessoas
            FROM vendas 
            WHERE nota = @Nota AND origem = 'BA'";

        return await connection.QueryFirstOrDefaultAsync<Venda>(sql, new { Nota = nota });
    }

    public async Task<IEnumerable<ItemVenda>> GetItensVendaAsync(string nota)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT nota, modelo, serie, subserie, origem, emissao, item, codigo,
                   barras, cfop, st, und, qtd, preco, desconto, acrescimo, total,
                   cancelado, sequencia, preco_custo, serial, icms, sinalm
            FROM itevendas 
            WHERE nota = @Nota AND origem = 'BA'
            ORDER BY item";

        return await connection.QueryAsync<ItemVenda>(sql, new { Nota = nota });
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
}
