using System.Collections.Concurrent;
using System.Security.Cryptography;
using IComanda.API.Models;

namespace IComanda.API.Services;

/// <summary>
/// Implementação em memória do serviço de Refresh Token (LEGADO)
/// 
/// ⚠️ ATENÇÃO: Esta implementação mantém tokens em memória
/// Tokens são perdidos ao reiniciar a aplicação
/// 
/// ✅ PRODUÇÃO: Use RefreshTokenDatabaseService (Firebird)
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenService> _logger;
    
    // Storage em memória (para produção, usar banco ou Redis)
    private static readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();
    
    private readonly int _refreshExpirationDays;
    
    public RefreshTokenService(IConfiguration configuration, ILogger<RefreshTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _refreshExpirationDays = int.Parse(configuration["Jwt:RefreshExpirationDays"] ?? "7");
    }
    
    public RefreshToken GenerateRefreshToken(int userId, string username, UserRole role)
    {
        var token = GenerateSecureRandomToken();
        
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            Username = username,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshExpirationDays),
            IsRevoked = false
        };
        
        _tokens[token] = refreshToken;
        
        _logger.LogInformation(
            "Refresh token gerado para usuário {UserId} ({Username}), expira em {ExpiresAt}",
            userId, username, refreshToken.ExpiresAt
        );
        
        return refreshToken;
    }
    
    public RefreshToken? ValidateRefreshToken(string token)
    {
        if (!_tokens.TryGetValue(token, out var refreshToken))
        {
            _logger.LogWarning("Refresh token não encontrado: {Token}", token.Substring(0, 8) + "...");
            return null;
        }
        
        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning(
                "Refresh token revogado: {Token}, Razão: {Reason}",
                token.Substring(0, 8) + "...",
                refreshToken.RevokedReason
            );
            return null;
        }
        
        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token expirado: {Token}", token.Substring(0, 8) + "...");
            return null;
        }
        
        return refreshToken;
    }
    
    public void RevokeRefreshToken(string token, string reason)
    {
        if (_tokens.TryGetValue(token, out var refreshToken))
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedReason = reason;
            
            _logger.LogInformation(
                "Refresh token revogado: Usuário {UserId}, Razão: {Reason}",
                refreshToken.UserId,
                reason
            );
        }
    }
    
    public void RevokeAllUserTokens(int userId, string reason)
    {
        var userTokens = _tokens.Values.Where(t => t.UserId == userId && !t.IsRevoked).ToList();
        
        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedReason = reason;
        }
        
        _logger.LogInformation(
            "Todos os refresh tokens revogados para usuário {UserId}: {Count} tokens, Razão: {Reason}",
            userId,
            userTokens.Count,
            reason
        );
    }
    
    public void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = _tokens
            .Where(kvp => kvp.Value.ExpiresAt < now || kvp.Value.IsRevoked)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var token in expiredTokens)
        {
            _tokens.TryRemove(token, out _);
        }
        
        if (expiredTokens.Count > 0)
        {
            _logger.LogInformation("Limpeza de refresh tokens: {Count} tokens removidos", expiredTokens.Count);
        }
    }
    
    /// <summary>Gera token aleatório criptograficamente seguro</summary>
    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
