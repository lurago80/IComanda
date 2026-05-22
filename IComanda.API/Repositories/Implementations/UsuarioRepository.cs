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

    public async Task AtualizarSenhaAsync(int usuarioId, string novaSenhaHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE USUARIO 
            SET SENHA = @NovaSenhaHash
            WHERE ID = @UsuarioId";

        await connection.ExecuteAsync(sql, new { UsuarioId = usuarioId, NovaSenhaHash = novaSenhaHash });
    }

    public async Task<IEnumerable<Usuario>> ListarTodosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT ID, NOME, SENHA, ATIVO, BLOQUEIO, VISUALIZAR, TOTAL, TIPO, CANCELAR
            FROM USUARIO
            ORDER BY NOME";

        return await connection.QueryAsync<Usuario>(sql);
    }

    public async Task<int> CriarAsync(Usuario usuario)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO USUARIO (NOME, SENHA, ATIVO, BLOQUEIO, VISUALIZAR, TOTAL, TIPO, CANCELAR)
            VALUES (@Nome, @Senha, @Ativo, @Bloqueio, @Visualizar, @Total, @Tipo, @Cancelar)
            RETURNING ID";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            usuario.Nome,
            usuario.Senha,
            usuario.Ativo,
            usuario.Bloqueio,
            usuario.Visualizar,
            usuario.Total,
            usuario.Tipo,
            usuario.Cancelar
        });
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE USUARIO
            SET NOME = @Nome,
                ATIVO = @Ativo,
                BLOQUEIO = @Bloqueio,
                VISUALIZAR = @Visualizar,
                TOTAL = @Total,
                TIPO = @Tipo,
                CANCELAR = @Cancelar
            WHERE ID = @Id";

        await connection.ExecuteAsync(sql, new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Ativo,
            usuario.Bloqueio,
            usuario.Visualizar,
            usuario.Total,
            usuario.Tipo,
            usuario.Cancelar
        });
    }

    public async Task ExcluirAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "UPDATE USUARIO SET ATIVO = '0' WHERE ID = @Id";

        await connection.ExecuteAsync(sql, new { Id = id });
    }
}

