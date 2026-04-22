using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using FirebirdSql.Data.FirebirdClient;
using System.Data;

namespace IComanda.API.Repositories.Implementations;

/// <summary>
/// Implementação do repository de Refresh Tokens usando Firebird
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<RefreshTokenRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>
    /// Verifica se a tabela REFRESH_TOKEN existe; se não, cria tabela, sequence, índices e trigger.
    /// Segue o mesmo padrão do HistoricoRepository.EnsureTableExistsAsync.
    /// </summary>
    public async Task EnsureTableExistsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        if (connection is not FbConnection fbConn)
        {
            _logger.LogWarning("[RefreshToken] Conexão não é FbConnection — verificação de tabela ignorada.");
            return;
        }

        if (connection.State != ConnectionState.Open)
            connection.Open();

        // Verificar se a tabela já existe via catálogo do Firebird
        var exists = await connection.ExecuteScalarAsync<int?>(
            "SELECT 1 FROM RDB$RELATIONS WHERE UPPER(TRIM(RDB$RELATION_NAME)) = 'REFRESH_TOKEN' AND RDB$SYSTEM_FLAG = 0");

        if (exists.HasValue && exists.Value == 1)
        {
            _logger.LogInformation("✅ [RefreshToken] Tabela REFRESH_TOKEN já existe no banco Firebird.");
            Console.WriteLine("✅ Tabela REFRESH_TOKEN: já existe.");
            return;
        }

        _logger.LogInformation("⚠️ [RefreshToken] Tabela REFRESH_TOKEN não encontrada. Criando...");
        Console.WriteLine("⚠️ Tabela REFRESH_TOKEN não encontrada — criando automaticamente...");

        var cmd = fbConn.CreateCommand();

        // 1. Criar tabela principal
        cmd.CommandText = @"
CREATE TABLE REFRESH_TOKEN (
    ID               INTEGER NOT NULL,
    TOKEN            VARCHAR(100) NOT NULL,
    USUARIO_ID       INTEGER NOT NULL,
    USUARIO_NOME     VARCHAR(100),
    USUARIO_ROLE     VARCHAR(50),
    DATA_CRIACAO     TIMESTAMP NOT NULL,
    DATA_EXPIRACAO   TIMESTAMP NOT NULL,
    REVOGADO         CHAR(1) DEFAULT '0',
    MOTIVO_REVOGACAO VARCHAR(200),
    DATA_REVOGACAO   TIMESTAMP,
    CONSTRAINT PK_REFRESH_TOKEN PRIMARY KEY (ID),
    CONSTRAINT UQ_REFRESH_TOKEN_TOKEN UNIQUE (TOKEN)
)";
        cmd.ExecuteNonQuery();
        _logger.LogInformation("  ✅ Tabela REFRESH_TOKEN criada.");

        // 2. Criar sequence (generator) para auto-incremento
        cmd.CommandText = "CREATE SEQUENCE GEN_REFRESH_TOKEN_ID";
        try { cmd.ExecuteNonQuery(); _logger.LogInformation("  ✅ Sequence GEN_REFRESH_TOKEN_ID criada."); }
        catch { _logger.LogWarning("  ⚠️ Sequence GEN_REFRESH_TOKEN_ID pode já existir — ignorado."); }

        // 3. Criar índices para performance
        cmd.CommandText = "CREATE INDEX IDX_REFRESH_TOKEN_USUARIO ON REFRESH_TOKEN (USUARIO_ID)";
        try { cmd.ExecuteNonQuery(); } catch { }

        cmd.CommandText = "CREATE INDEX IDX_REFRESH_TOKEN_EXPIRACAO ON REFRESH_TOKEN (DATA_EXPIRACAO)";
        try { cmd.ExecuteNonQuery(); } catch { }

        cmd.CommandText = "CREATE INDEX IDX_REFRESH_TOKEN_REVOGADO ON REFRESH_TOKEN (REVOGADO)";
        try { cmd.ExecuteNonQuery(); } catch { }

        // 4. Criar trigger de auto-incremento do ID
        cmd.CommandText = @"
CREATE TRIGGER TRG_REFRESH_TOKEN_BI FOR REFRESH_TOKEN
ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
 IF (NEW.ID IS NULL) THEN NEW.ID = GEN_ID(GEN_REFRESH_TOKEN_ID, 1);
END";
        try
        {
            cmd.ExecuteNonQuery();
            _logger.LogInformation("  ✅ Trigger TRG_REFRESH_TOKEN_BI criada.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "  ⚠️ Trigger TRG_REFRESH_TOKEN_BI pode já existir — ignorado.");
        }

        _logger.LogInformation("✅ [RefreshToken] Tabela REFRESH_TOKEN criada com sucesso no Firebird.");
        Console.WriteLine("✅ Tabela REFRESH_TOKEN criada com sucesso!");
    }

    public async Task<int> SalvarAsync(RefreshTokenEntity token)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO REFRESH_TOKEN (
                TOKEN,
                USUARIO_ID,
                USUARIO_NOME,
                USUARIO_ROLE,
                DATA_CRIACAO,
                DATA_EXPIRACAO,
                REVOGADO
            ) VALUES (
                @Token,
                @UsuarioId,
                @UsuarioNome,
                @UsuarioRole,
                @DataCriacao,
                @DataExpiracao,
                @Revogado
            )
            RETURNING ID";

        try
        {
            var id = await connection.ExecuteScalarAsync<int>(sql, token);
            _logger.LogDebug($"Refresh token salvo: ID={id}, UsuarioId={token.UsuarioId}");
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar refresh token");
            throw;
        }
    }

    public async Task<RefreshTokenEntity?> BuscarPorTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                ID,
                TOKEN,
                USUARIO_ID AS UsuarioId,
                USUARIO_NOME AS UsuarioNome,
                USUARIO_ROLE AS UsuarioRole,
                DATA_CRIACAO AS DataCriacao,
                DATA_EXPIRACAO AS DataExpiracao,
                REVOGADO,
                MOTIVO_REVOGACAO AS MotivoRevogacao,
                DATA_REVOGACAO AS DataRevogacao
            FROM REFRESH_TOKEN
            WHERE TOKEN = @Token";

        try
        {
            var result = await connection.QueryFirstOrDefaultAsync<RefreshTokenEntity>(
                sql, 
                new { Token = token });
            
            if (result != null)
            {
                _logger.LogDebug($"Refresh token encontrado: ID={result.Id}, Válido={result.IsValid}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar refresh token");
            throw;
        }
    }

    public async Task<IEnumerable<RefreshTokenEntity>> BuscarPorUsuarioAsync(int usuarioId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT 
                ID,
                TOKEN,
                USUARIO_ID AS UsuarioId,
                USUARIO_NOME AS UsuarioNome,
                USUARIO_ROLE AS UsuarioRole,
                DATA_CRIACAO AS DataCriacao,
                DATA_EXPIRACAO AS DataExpiracao,
                REVOGADO,
                MOTIVO_REVOGACAO AS MotivoRevogacao,
                DATA_REVOGACAO AS DataRevogacao
            FROM REFRESH_TOKEN
            WHERE USUARIO_ID = @UsuarioId
            ORDER BY DATA_CRIACAO DESC";

        try
        {
            var tokens = await connection.QueryAsync<RefreshTokenEntity>(
                sql, 
                new { UsuarioId = usuarioId });
            
            _logger.LogDebug($"Encontrados {tokens.Count()} tokens para usuário {usuarioId}");
            return tokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar tokens do usuário {usuarioId}");
            throw;
        }
    }

    public async Task RevogarAsync(string token, string motivo)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE REFRESH_TOKEN
            SET REVOGADO = '1',
                MOTIVO_REVOGACAO = @Motivo,
                DATA_REVOGACAO = CURRENT_TIMESTAMP
            WHERE TOKEN = @Token";

        try
        {
            var affected = await connection.ExecuteAsync(
                sql, 
                new { Token = token, Motivo = motivo });
            
            if (affected > 0)
            {
                _logger.LogInformation($"Refresh token revogado: {token.Substring(0, 10)}... - Motivo: {motivo}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar refresh token");
            throw;
        }
    }

    public async Task RevogarTodosUsuarioAsync(int usuarioId, string motivo)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE REFRESH_TOKEN
            SET REVOGADO = '1',
                MOTIVO_REVOGACAO = @Motivo,
                DATA_REVOGACAO = CURRENT_TIMESTAMP
            WHERE USUARIO_ID = @UsuarioId
              AND REVOGADO = '0'";

        try
        {
            var affected = await connection.ExecuteAsync(
                sql, 
                new { UsuarioId = usuarioId, Motivo = motivo });
            
            _logger.LogInformation($"Revogados {affected} tokens do usuário {usuarioId} - Motivo: {motivo}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao revogar tokens do usuário {usuarioId}");
            throw;
        }
    }

    public async Task LimparExpiradosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Revogar tokens expirados primeiro (para histórico)
        const string sqlRevogar = @"
            UPDATE REFRESH_TOKEN
            SET REVOGADO = '1',
                MOTIVO_REVOGACAO = 'Expirado automaticamente',
                DATA_REVOGACAO = CURRENT_TIMESTAMP
            WHERE DATA_EXPIRACAO < CURRENT_TIMESTAMP
              AND REVOGADO = '0'";

        // Depois deletar tokens revogados há mais de 30 dias
        const string sqlDeletar = @"
            DELETE FROM REFRESH_TOKEN
            WHERE REVOGADO = '1'
              AND DATA_REVOGACAO < DATEADD(-30 DAY TO CURRENT_TIMESTAMP)";

        try
        {
            var revogados = await connection.ExecuteAsync(sqlRevogar);
            var deletados = await connection.ExecuteAsync(sqlDeletar);
            
            _logger.LogInformation($"Limpeza de tokens: {revogados} expirados, {deletados} deletados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar tokens expirados");
            throw;
        }
    }

    public async Task<int> ContarTokensValidosUsuarioAsync(int usuarioId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COUNT(*)
            FROM REFRESH_TOKEN
            WHERE USUARIO_ID = @UsuarioId
              AND REVOGADO = '0'
              AND DATA_EXPIRACAO > CURRENT_TIMESTAMP";

        try
        {
            var count = await connection.ExecuteScalarAsync<int>(
                sql, 
                new { UsuarioId = usuarioId });
            
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao contar tokens válidos do usuário {usuarioId}");
            throw;
        }
    }
}
