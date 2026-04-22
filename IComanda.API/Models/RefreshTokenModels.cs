namespace IComanda.API.Models;

/// <summary>
/// Request para renovar o token JWT usando um refresh token
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>Token JWT expirado</summary>
    public required string Token { get; set; }
    
    /// <summary>Refresh token válido</summary>
    public required string RefreshToken { get; set; }
}

/// <summary>
/// Response com novo token JWT e refresh token
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>Novo token JWT</summary>
    public required string Token { get; set; }
    
    /// <summary>Novo refresh token</summary>
    public required string RefreshToken { get; set; }
    
    /// <summary>Data de expiração do token</summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>Data de expiração do refresh token</summary>
    public DateTime RefreshExpiresAt { get; set; }
}

/// <summary>
/// Armazena refresh tokens ativos no banco/cache
/// Para produção, mover para banco de dados ou Redis
/// </summary>
public class RefreshToken
{
    public required string Token { get; set; }
    public required int UserId { get; set; }
    public required string Username { get; set; }
    public required UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedReason { get; set; }
}
