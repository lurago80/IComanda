namespace IComanda.API.Models.Entities;

/// <summary>
/// Grupo/Categoria de Produto
/// </summary>
public class GrupoProduto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public int? Ordem { get; set; }
}
