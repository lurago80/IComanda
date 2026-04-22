using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IComanda.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace IComanda.API.Services;

/// <summary>
/// Serviço responsável pela geração e validação de tokens JWT
/// </summary>
public interface IJwtTokenProvider
{
    /// <summary>Gera um token JWT para um usuário</summary>
    string GenerateToken(int userId, string username, UserRole role, DateTime? expiresAt = null);
    
    /// <summary>Valida um token JWT e retorna os claims</summary>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>Obtém o userId do token</summary>
    int? GetUserIdFromToken(string token);
    
    /// <summary>Obtém o username do token</summary>
    string? GetUsernameFromToken(string token);
    
    /// <summary>Obtém o role do token</summary>
    UserRole? GetRoleFromToken(string token);
}

public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationHours;

    public JwtTokenProvider(IConfiguration configuration)
    {
        _configuration = configuration;
        
        var jwtKey = configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("Jwt:Key não está configurada em appsettings.json");
        
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        _issuer = configuration["Jwt:Issuer"] ?? "IComanda.API";
        _audience = configuration["Jwt:Audience"] ?? "IComanda.Client";
        _expirationHours = int.Parse(configuration["Jwt:ExpirationHours"] ?? "8");
    }

    /// <summary>Gera um token JWT para um usuário</summary>
    public string GenerateToken(int userId, string username, UserRole role, DateTime? expiresAt = null)
    {
        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("UserId", userId.ToString()),
            new Claim("Role", role.ToString()),
            new Claim("RoleValue", ((int)role).ToString())
        };
        
        // Adicionar permissões como claims
        var permissions = RolePermissionMapping.GetPermissions(role);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission.ToString()));
        }
        
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt ?? DateTime.UtcNow.AddHours(_expirationHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Valida um token JWT e retorna os claims</summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Obtém o userId do token</summary>
    public int? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }

        // Fallback para NameIdentifier
        var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdClaim != null && int.TryParse(nameIdClaim.Value, out int id))
        {
            return id;
        }

        return null;
    }

    /// <summary>Obtém o username do token</summary>
    public string? GetUsernameFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>Obtém o role do token</summary>
    public UserRole? GetRoleFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        var roleClaim = principal.FindFirst("Role");
        if (roleClaim != null && Enum.TryParse<UserRole>(roleClaim.Value, out var role))
        {
            return role;
        }

        return null;
    }
}
