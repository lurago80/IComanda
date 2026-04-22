using System.ComponentModel.DataAnnotations;

namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para criar um novo Pedido de Força de Vendas
/// </summary>
public class CriarPedidoFVRequest
{
    [Required(ErrorMessage = "Vendedor é obrigatório")]
    public int IdVendedor { get; set; }

    [Required(ErrorMessage = "Cliente é obrigatório")]
    public int IdCliente { get; set; }

    public string? CondicaoPgto { get; set; }

    public int TabelaPreco { get; set; } = 1;

    [Range(0, double.MaxValue)]
    public decimal Desconto { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Acrescimo { get; set; }

    public string? Obs { get; set; }

    [Required(ErrorMessage = "O pedido deve ter ao menos um item")]
    [MinLength(1, ErrorMessage = "O pedido deve ter ao menos um item")]
    public List<ItemPedidoFVRequest> Itens { get; set; } = new();
}
