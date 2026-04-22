namespace IComanda.API.Models.Entities;

/// <summary>
/// Item de Pedido de Força de Vendas — tabela ITENS_PEDIDO_FV
/// </summary>
public class ItemPedidoFV
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
