namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade Refresh Token armazenada no banco de dados Firebird
/// </summary>
public class RefreshTokenEntity
{
    /// <summary>ID único do refresh token</summary>
    public int Id { get; set; }
    
    /// <summary>Token único (64 bytes codificado em Base64)</summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>ID do usuário proprietário do token</summary>
    public int UsuarioId { get; set; }
    
    /// <summary>Nome do usuário</summary>
    public string? UsuarioNome { get; set; }
    
    /// <summary>Role do usuário (Admin, Garcom, etc)</summary>
    public string? UsuarioRole { get; set; }
    
    /// <summary>Data/hora de criação do token</summary>
    public DateTime DataCriacao { get; set; }
    
    /// <summary>Data/hora de expiração do token</summary>
    public DateTime DataExpiracao { get; set; }
    
    /// <summary>Se o token foi revogado ('1' = sim, '0' = não)</summary>
    public string Revogado { get; set; } = "0";
    
    /// <summary>Motivo da revogação (opcional)</summary>
    public string? MotivoRevogacao { get; set; }
    
    /// <summary>Data/hora da revogação (opcional)</summary>
    public DateTime? DataRevogacao { get; set; }
    
    /// <summary>Verifica se o token está expirado</summary>
    public bool IsExpired => DateTime.UtcNow > DataExpiracao;
    
    /// <summary>Verifica se o token foi revogado</summary>
    public bool IsRevoked => Revogado == "1";
    
    /// <summary>Verifica se o token é válido (não expirado e não revogado)</summary>
    public bool IsValid => !IsExpired && !IsRevoked;
}
