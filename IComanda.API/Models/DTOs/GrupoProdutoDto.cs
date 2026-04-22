namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para Grupo de Produto
/// </summary>
public class GrupoProdutoDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public int? Ordem { get; set; }
}
