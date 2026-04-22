using System.Security.Cryptography;
using IComanda.API.Models;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Implementação do serviço de Refresh Token usando banco de dados Firebird
/// 
/// Esta implementação persiste os refresh tokens no banco de dados,
/// permitindo escalabilidade horizontal e sobrevivência a reinicializações.
/// </summary>
public class RefreshTokenDatabaseService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenDatabaseService> _logger;
    private readonly int _refreshExpirationDays;

    public RefreshTokenDatabaseService(
        IRefreshTokenRepository repository,
        IConfiguration configuration,
        ILogger<RefreshTokenDatabaseService> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
        _refreshExpirationDays = int.Parse(configuration["Jwt:RefreshExpirationDays"] ?? "7");
    }

    public RefreshToken GenerateRefreshToken(int userId, string username, UserRole role)
    {
        var tokenString = GenerateSecureRandomToken();
        
        var entity = new RefreshTokenEntity
        {
            Token = tokenString,
            UsuarioId = userId,
            UsuarioNome = username,
            UsuarioRole = role.ToString(),
            DataCriacao = DateTime.UtcNow,
            DataExpiracao = DateTime.UtcNow.AddDays(_refreshExpirationDays),
            Revogado = "0"
        };
        
        try
        {
            var id = _repository.SalvarAsync(entity).GetAwaiter().GetResult();
            entity.Id = id;
            
            _logger.LogInformation(
                "✅ Refresh token gerado e salvo: ID={Id}, Usuario={UserId} ({Username}), Expira={ExpiresAt}",
                id, userId, username, entity.DataExpiracao
            );
            
            // Converter para modelo de domínio
            return new RefreshToken
            {
                Token = entity.Token,
                UserId = entity.UsuarioId,
                Username = entity.UsuarioNome ?? "",
                Role = Enum.Parse<UserRole>(entity.UsuarioRole ?? "Garcom"),
                CreatedAt = entity.DataCriacao,
                ExpiresAt = entity.DataExpiracao,
                IsRevoked = entity.IsRevoked
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao gerar e salvar refresh token");
            throw new InvalidOperationException("Falha ao gerar refresh token", ex);
        }
    }

    public RefreshToken? ValidateRefreshToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("❌ Tentativa de validação com token vazio");
            return null;
        }

        try
        {
            var entity = _repository.BuscarPorTokenAsync(token).GetAwaiter().GetResult();
            
            if (entity == null)
            {
                _logger.LogWarning("❌ Refresh token não encontrado: {Token}", 
                    token.Length > 10 ? token.Substring(0, 10) + "..." : token);
                return null;
            }
            
            // Verificar se está expirado
            if (entity.IsExpired)
            {
                _logger.LogWarning("⏰ Refresh token expirado: ID={Id}, Usuario={UserId}, ExpiradoEm={ExpiresAt}",
                    entity.Id, entity.UsuarioId, entity.DataExpiracao);
                return null;
            }
            
            // Verificar se foi revogado
            if (entity.IsRevoked)
            {
                _logger.LogWarning("🚫 Refresh token revogado: ID={Id}, Usuario={UserId}, Motivo={Motivo}",
                    entity.Id, entity.UsuarioId, entity.MotivoRevogacao ?? "N/A");
                return null;
            }
            
            _logger.LogDebug("✅ Refresh token válido: ID={Id}, Usuario={UserId}",
                entity.Id, entity.UsuarioId);
            
            // Converter para modelo de domínio
            return new RefreshToken
            {
                Token = entity.Token,
                UserId = entity.UsuarioId,
                Username = entity.UsuarioNome ?? "",
                Role = Enum.Parse<UserRole>(entity.UsuarioRole ?? "Garcom"),
                CreatedAt = entity.DataCriacao,
                ExpiresAt = entity.DataExpiracao,
                IsRevoked = entity.IsRevoked
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao validar refresh token");
            return null;
        }
    }

    public void RevokeRefreshToken(string token, string reason)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("❌ Tentativa de revogar token vazio");
            return;
        }

        try
        {
            _repository.RevogarAsync(token, reason).GetAwaiter().GetResult();
            _logger.LogInformation("🚫 Refresh token revogado: {Token}, Motivo: {Reason}",
                token.Length > 10 ? token.Substring(0, 10) + "..." : token, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao revogar refresh token");
            throw;
        }
    }

    public void RevokeAllUserTokens(int userId, string reason)
    {
        try
        {
            _repository.RevogarTodosUsuarioAsync(userId, reason).GetAwaiter().GetResult();
            _logger.LogInformation("🚫 Todos os tokens do usuário {UserId} foram revogados. Motivo: {Reason}",
                userId, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao revogar todos os tokens do usuário {UserId}", userId);
            throw;
        }
    }

    public void CleanupExpiredTokens()
    {
        try
        {
            _repository.LimparExpiradosAsync().GetAwaiter().GetResult();
            _logger.LogInformation("🧹 Limpeza de tokens expirados concluída");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao limpar tokens expirados");
            // Não propagar exceção - limpeza não é crítica
        }
    }

    /// <summary>
    /// Gera um token aleatório criptograficamente seguro
    /// </summary>
    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64]; // 64 bytes = 512 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        
        // Converter para Base64 (URL-safe)
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
