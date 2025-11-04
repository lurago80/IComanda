using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class ItemVendaTemporarioRepository : IItemVendaTemporarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ItemVendaTemporarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CriarItensTemporariosAsync(List<ItemVendaTemporario> itens)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            INSERT INTO frente_tmpitvendas (
                cupom, n_caixa, data, hora, operador, item, codigo, barras, 
                descricao, qtd, preco, tributacao, icms, iss, und, 
                desconto, acrescimo, total, serial, tipo
            ) VALUES (
                @Cupom, @NCaixa, @Data, @Hora, @Operador, @Item, @Codigo, @Barras,
                @Descricao, @Qtd, @Preco, @Tributacao, @Icms, @Iss, @Und,
                @Desconto, @Acrescimo, @Total, @Serial, @Tipo
            )";

        var linhasAfetadas = await connection.ExecuteAsync(sql, itens);
        return linhasAfetadas > 0;
    }

    public async Task<IEnumerable<ItemVendaTemporario>> GetItensPorCupomAsync(string cupom, int operador)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT cupom, n_caixa as NCaixa, data, hora, operador, item, codigo, 
                   barras, descricao, qtd, preco, tributacao, icms, iss, und,
                   desconto, acrescimo, total, serial, tipo
            FROM frente_tmpitvendas 
            WHERE cupom = @Cupom AND operador = @Operador AND tipo = 1
            ORDER BY item";

        return await connection.QueryAsync<ItemVendaTemporario>(sql, new { Cupom = cupom, Operador = operador });
    }

    public async Task<bool> LimparItensCupomAsync(string cupom, int operador)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            DELETE FROM frente_tmpitvendas 
            WHERE cupom = @Cupom AND operador = @Operador";

        await connection.ExecuteAsync(sql, new { Cupom = cupom, Operador = operador });
        return true;
    }
}

