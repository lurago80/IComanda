using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

/// <summary>
/// Repository para gerenciamento de Refresh Tokens no banco de dados Firebird
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Salva um novo refresh token</summary>
    Task<int> SalvarAsync(RefreshTokenEntity token);
    
    /// <summary>Busca um refresh token pelo valor do token</summary>
    Task<RefreshTokenEntity?> BuscarPorTokenAsync(string token);
    
    /// <summary>Busca todos os tokens de um usuário</summary>
    Task<IEnumerable<RefreshTokenEntity>> BuscarPorUsuarioAsync(int usuarioId);
    
    /// <summary>Revoga um refresh token específico</summary>
    Task RevogarAsync(string token, string motivo);
    
    /// <summary>Revoga todos os tokens de um usuário</summary>
    Task RevogarTodosUsuarioAsync(int usuarioId, string motivo);
    
    /// <summary>Remove tokens expirados (limpeza)</summary>
    Task LimparExpiradosAsync();
    
    /// <summary>Conta quantos tokens válidos um usuário possui</summary>
    Task<int> ContarTokensValidosUsuarioAsync(int usuarioId);

    /// <summary>
    /// Verifica se a tabela REFRESH_TOKEN existe no banco Firebird.
    /// Se não existir, cria automaticamente a tabela, sequence, índices e trigger.
    /// Deve ser chamado na inicialização do serviço.
    /// </summary>
    Task EnsureTableExistsAsync();
}
