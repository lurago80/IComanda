namespace IComanda.API.Models.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool Bloqueado { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public bool PodeVisualizar { get; set; }
    public bool PodeVerTotal { get; set; }
    public bool PodeCancelar { get; set; }
}

