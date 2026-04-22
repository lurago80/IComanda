namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO de item de pedido FV
/// </summary>
public class ItemPedidoFVDto
{
    public int Id { get; set; }
    public int IdPedidoFV { get; set; }
    public int IdProduto { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public string Unidade { get; set; } = "UN";
    public decimal PrecoUnitario { get; set; }
    public decimal Desconto { get; set; }
    public decimal Total { get; set; }
    public string? Obs { get; set; }
}
