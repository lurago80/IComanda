namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de grupo de produtos - tabela GRUPO
/// </summary>
public class Grupo
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int QuantidadeProdutos { get; set; }
    public bool ImprimirDuasVias { get; set; } = false;
}
