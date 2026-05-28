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
    /// <summary>NORMAL = lista simples, PIZZA = montagem de pizza com sabores e bordas</summary>
    public string Tipo { get; set; } = "NORMAL";
    /// <summary>Percentual a pagar ao fornecedor (consignação). 0 = sem percentual.</summary>
    public decimal Percentual { get; set; } = 0;
}
