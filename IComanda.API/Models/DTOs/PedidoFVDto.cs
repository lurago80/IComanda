using IComanda.API.Models.Enums;

namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO de Pedido de Força de Vendas
/// </summary>
public class PedidoFVDto
{
    public int Id { get; set; }
    public int IdVendedor { get; set; }
    public string NomeVendedor { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public TimeSpan HoraPedido { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StatusCodigo { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string? Obs { get; set; }
    public string? CondicaoPgto { get; set; }
    public int TabelaPreco { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public string? NomeAprovador { get; set; }
    public string? MotivoCancel { get; set; }
    public string? NotaFiscal { get; set; }
    public DateTime? DataFaturamento { get; set; }
    public List<ItemPedidoFVDto> Itens { get; set; } = new();
}
