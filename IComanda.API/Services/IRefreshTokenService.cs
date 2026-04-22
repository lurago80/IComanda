using IComanda.API.Models;

namespace IComanda.API.Services;

/// <summary>
/// Interface para gerenciamento de Refresh Tokens
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Gera um novo refresh token para o usuário</summary>
    RefreshToken GenerateRefreshToken(int userId, string username, UserRole role);
    
    /// <summary>Valida um refresh token</summary>
    RefreshToken? ValidateRefreshToken(string token);
    
    /// <summary>Revoga um refresh token</summary>
    void RevokeRefreshToken(string token, string reason);
    
    /// <summary>Revoga todos os refresh tokens de um usuário</summary>
    void RevokeAllUserTokens(int userId, string reason);
    
    /// <summary>Remove tokens expirados (limpeza)</summary>
    void CleanupExpiredTokens();
}
