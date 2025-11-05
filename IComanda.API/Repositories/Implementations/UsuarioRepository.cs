using FirebirdSql.Data.FirebirdClient;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Dapper;

namespace IComanda.API.Repositories.Implementations;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsuarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Usuario?> BuscarPorNomeAsync(string nome)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                ID, 
                NOME, 
                SENHA, 
                ATIVO, 
                BLOQUEIO, 
                VISUALIZAR, 
                TOTAL, 
                TIPO, 
                CANCELAR
            FROM USUARIO
            WHERE UPPER(TRIM(NOME)) = UPPER(@Nome)
            AND ATIVO = '1'";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Nome = nome.Trim() });
    }

    public async Task<Usuario?> BuscarPorIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                ID, 
                NOME, 
                SENHA, 
                ATIVO, 
                BLOQUEIO, 
                VISUALIZAR, 
                TOTAL, 
                TIPO, 
                CANCELAR
            FROM USUARIO
            WHERE ID = @Id";

        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Usuario>> ListarAtivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                ID, 
                NOME, 
                SENHA, 
                ATIVO, 
                BLOQUEIO, 
                VISUALIZAR, 
                TOTAL, 
                TIPO, 
                CANCELAR
            FROM USUARIO
            WHERE ATIVO = '1'
            ORDER BY NOME";

        return await connection.QueryAsync<Usuario>(sql);
    }
}

