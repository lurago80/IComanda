namespace IComanda.API.Models.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Senha { get; set; }
    public string? Ativo { get; set; }
    public string? Bloqueio { get; set; }
    public string? Visualizar { get; set; }
    public string? Total { get; set; }
    public string? Tipo { get; set; }
    public string? Cancelar { get; set; }
}

