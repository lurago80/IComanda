namespace IComanda.API.Models.Entities;

/// <summary>
/// Taxa de entrega - tabela TAXA_ENTREGA
/// </summary>
public class TaxaEntrega
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
