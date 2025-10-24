using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class ProdutoRepository : IProdutoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProdutoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Produto>> BuscarProdutosAsync(string? termo, bool? ativo = true, int? grupo = null, int pagina = 1, int itensPorPagina = 50)
    {
        using var connection = _connectionFactory.CreateConnection();

        var offset = (pagina - 1) * itensPorPagina;

        var sql = @"
            SELECT FIRST @ItensPorPagina SKIP @Offset
                p1.id, p1.codigobarra, p3.codigointerno, p3.descricao, p3.caracteristica,
                p2.quantidade, p2.quantidademinima, p2.localizacao,
                p4.precocusto, p4.precovenda, p4.atacado, p4.preco3,
                p3.un_medida, p3.ativo, p3.grupo, p1.pesavel, p3.marca,
                p3.categoria, p3.cor, p3.tamanho, p3.datainclusao,
                p2.dataultimavenda, p2.dataultimacompra
            FROM produto p1
            INNER JOIN produtoempresa p2 ON p1.id = p2.id
            INNER JOIN produtoeservico p3 ON p1.id = p3.id
            INNER JOIN produtoeservicoempresa p4 ON p1.id = p4.id
            WHERE 1=1";

        var parameters = new DynamicParameters();
        parameters.Add("@ItensPorPagina", itensPorPagina);
        parameters.Add("@Offset", offset);

        if (!string.IsNullOrEmpty(termo))
        {
            sql += @" AND (
                p1.codigobarra = @Termo OR
                p3.descricao CONTAINING @Termo OR
                p3.caracteristica CONTAINING @Termo OR
                p3.codigointerno = @Termo OR
                p1.id = @TermoNum
            )";
            parameters.Add("@Termo", termo);
            if (int.TryParse(termo, out var termoNum))
                parameters.Add("@TermoNum", termoNum);
            else
                parameters.Add("@TermoNum", 0);
        }

        if (ativo.HasValue)
        {
            sql += " AND p3.ativo = @Ativo";
            parameters.Add("@Ativo", ativo.Value);
        }

        if (grupo.HasValue)
        {
            sql += " AND p3.grupo = @Grupo";
            parameters.Add("@Grupo", grupo.Value);
        }

        sql += " ORDER BY p3.descricao";

        return await connection.QueryAsync<Produto>(sql, parameters);
    }

    public async Task<Produto?> GetProdutoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT p1.id, p1.codigobarra, p3.codigointerno, p3.descricao, p3.caracteristica,
                   p2.quantidade, p2.quantidademinima, p2.localizacao,
                   p4.precocusto, p4.precovenda, p4.atacado, p4.preco3,
                   p3.un_medida, p3.ativo, p3.grupo, p1.pesavel, p3.marca,
                   p3.categoria, p3.cor, p3.tamanho, p3.datainclusao,
                   p2.dataultimavenda, p2.dataultimacompra
            FROM produto p1
            INNER JOIN produtoempresa p2 ON p1.id = p2.id
            INNER JOIN produtoeservico p3 ON p1.id = p3.id
            INNER JOIN produtoeservicoempresa p4 ON p1.id = p4.id
            WHERE p1.id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Produto>(sql, new { Id = id });
    }

    public async Task<Produto?> GetProdutoPorCodigoBarraAsync(string codigoBarra)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT p1.id, p1.codigobarra, p3.codigointerno, p3.descricao, p3.caracteristica,
                   p2.quantidade, p2.quantidademinima, p2.localizacao,
                   p4.precocusto, p4.precovenda, p4.atacado, p4.preco3,
                   p3.un_medida, p3.ativo, p3.grupo, p1.pesavel, p3.marca,
                   p3.categoria, p3.cor, p3.tamanho, p3.datainclusao,
                   p2.dataultimavenda, p2.dataultimacompra
            FROM produto p1
            INNER JOIN produtoempresa p2 ON p1.id = p2.id
            INNER JOIN produtoeservico p3 ON p1.id = p3.id
            INNER JOIN produtoeservicoempresa p4 ON p1.id = p4.id
            WHERE p1.codigobarra = @CodigoBarra AND p3.ativo = 1";

        return await connection.QueryFirstOrDefaultAsync<Produto>(sql, new { CodigoBarra = codigoBarra });
    }

    public async Task<Produto?> GetProdutoPorCodigoInternoAsync(string codigoInterno)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT p1.id, p1.codigobarra, p3.codigointerno, p3.descricao, p3.caracteristica,
                   p2.quantidade, p2.quantidademinima, p2.localizacao,
                   p4.precocusto, p4.precovenda, p4.atacado, p4.preco3,
                   p3.un_medida, p3.ativo, p3.grupo, p1.pesavel, p3.marca,
                   p3.categoria, p3.cor, p3.tamanho, p3.datainclusao,
                   p2.dataultimavenda, p2.dataultimacompra
            FROM produto p1
            INNER JOIN produtoempresa p2 ON p1.id = p2.id
            INNER JOIN produtoeservico p3 ON p1.id = p3.id
            INNER JOIN produtoeservicoempresa p4 ON p1.id = p4.id
            WHERE p3.codigointerno = @CodigoInterno AND p3.ativo = 1";

        return await connection.QueryFirstOrDefaultAsync<Produto>(sql, new { CodigoInterno = codigoInterno });
    }

    public async Task<int> ContarProdutosAsync(string? termo, bool? ativo = true, int? grupo = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT COUNT(*)
            FROM produto p1
            INNER JOIN produtoempresa p2 ON p1.id = p2.id
            INNER JOIN produtoeservico p3 ON p1.id = p3.id
            INNER JOIN produtoeservicoempresa p4 ON p1.id = p4.id
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(termo))
        {
            sql += @" AND (
                p1.codigobarra = @Termo OR
                p3.descricao CONTAINING @Termo OR
                p3.caracteristica CONTAINING @Termo OR
                p3.codigointerno = @Termo OR
                p1.id = @TermoNum
            )";
            parameters.Add("@Termo", termo);
            if (int.TryParse(termo, out var termoNum))
                parameters.Add("@TermoNum", termoNum);
            else
                parameters.Add("@TermoNum", 0);
        }

        if (ativo.HasValue)
        {
            sql += " AND p3.ativo = @Ativo";
            parameters.Add("@Ativo", ativo.Value);
        }

        if (grupo.HasValue)
        {
            sql += " AND p3.grupo = @Grupo";
            parameters.Add("@Grupo", grupo.Value);
        }

        return await connection.QuerySingleAsync<int>(sql, parameters);
    }
}
