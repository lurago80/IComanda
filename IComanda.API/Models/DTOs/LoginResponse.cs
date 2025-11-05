namespace IComanda.API.Models.DTOs;

public class LoginResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public bool PodeVisualizar { get; set; }
    public bool PodeVerTotal { get; set; }
    public bool PodeCancelar { get; set; }
}

