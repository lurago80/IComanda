using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class GrupoRepository : IGrupoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GrupoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Grupo>> GetAllGruposAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT id, descricao, codgrupo
            FROM grupo
            ORDER BY descricao";

        return await connection.QueryAsync<Grupo>(sql);
    }

    public async Task<Grupo?> GetGrupoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT id, descricao, codgrupo
            FROM grupo
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Grupo>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Grupo>> GetGruposComQuantidadeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT g.id, g.descricao, g.codgrupo,
                   COUNT(p3.id) as QuantidadeProdutos
            FROM grupo g
            LEFT JOIN produtoeservico p3 ON g.id = p3.grupo AND p3.ativo = 1
            GROUP BY g.id, g.descricao, g.codgrupo
            HAVING COUNT(p3.id) > 0
            ORDER BY g.descricao";

        return await connection.QueryAsync<Grupo>(sql);
    }

    public async Task<IEnumerable<Grupo>> GetGruposComQuantidadeTodosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT g.id, g.descricao, g.codgrupo,
                   COUNT(p3.id) as QuantidadeProdutos
            FROM grupo g
            LEFT JOIN produtoeservico p3 ON g.id = p3.grupo AND p3.ativo = 1
            GROUP BY g.id, g.descricao, g.codgrupo
            ORDER BY g.descricao";

        var result = await connection.QueryAsync<dynamic>(sql);

        return result.Select(r => new Grupo
        {
            Id = (int)r.id,
            Descricao = r.descricao?.ToString() ?? "",
            CodGrupo = (short)(r.codgrupo ?? 0),
            QuantidadeProdutos = (int)r.QuantidadeProdutos
        });
    }
}
