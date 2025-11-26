namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para grupo de produtos
/// </summary>
public class GrupoDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int QuantidadeProdutos { get; set; }
}
