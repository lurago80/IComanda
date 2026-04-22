namespace IComanda.API.Models.DTOs;

public class LoginResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public bool PodeVisualizar { get; set; }
    public bool PodeVerTotal { get; set; }
    public bool PodeCancelar { get; set; }
    public int ExpiresIn { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
}

