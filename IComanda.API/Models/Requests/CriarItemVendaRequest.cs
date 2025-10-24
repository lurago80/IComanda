namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para criar um item de venda
/// </summary>
public class CriarItemVendaRequest
{
    /// <summary>
    /// Código do produto
    /// </summary>
    public int Codigo { get; set; }

    /// <summary>
    /// Código de barras do produto
    /// </summary>
    public string Barras { get; set; } = string.Empty;

    /// <summary>
    /// Unidade de medida
    /// </summary>
    public string Und { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade
    /// </summary>
    public decimal Qtd { get; set; }

    /// <summary>
    /// Preço unitário
    /// </summary>
    public decimal Preco { get; set; }

    /// <summary>
    /// Desconto aplicado
    /// </summary>
    public decimal Desconto { get; set; }

    /// <summary>
    /// Acréscimo aplicado
    /// </summary>
    public decimal Acrescimo { get; set; }

    /// <summary>
    /// Total do item
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Preço de custo
    /// </summary>
    public decimal PrecoCusto { get; set; }

    /// <summary>
    /// Serial do produto (se aplicável)
    /// </summary>
    public string Serial { get; set; } = string.Empty;

    /// <summary>
    /// ICMS
    /// </summary>
    public decimal Icms { get; set; }
}
