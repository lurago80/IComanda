using IComanda.API.Models.Enums;

namespace IComanda.API.Models.Entities;

/// <summary>
/// Pedido de Força de Vendas — tabela PEDIDOS_FV
/// </summary>
public class PedidoFV
{
    public int Id { get; set; }
    public int IdVendedor { get; set; }
    public int IdCliente { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.Now;
    public TimeSpan HoraPedido { get; set; } = DateTime.Now.TimeOfDay;
    public StatusPedidoFV Status { get; set; } = StatusPedidoFV.Pendente;
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string? Obs { get; set; }
    /// <summary>Condição de pagamento negociada (Ex: 30/60/90, À VISTA)</summary>
    public string? CondicaoPgto { get; set; }
    /// <summary>Tabela de preço utilizada</summary>
    public int TabelaPreco { get; set; } = 1;
    /// <summary>Data em que o gestor aprovou/recusou</summary>
    public DateTime? DataAprovacao { get; set; }
    /// <summary>ID do usuário que aprovou</summary>
    public int? IdAprovador { get; set; }
    /// <summary>Justificativa de cancelamento</summary>
    public string? MotivoCancel { get; set; }
    /// <summary>Nota fiscal gerada após faturamento</summary>
    public string? NotaFiscal { get; set; }
    public DateTime? DataFaturamento { get; set; }

    // Dados desnormalizados para exibição rápida
    public string? NomeCliente { get; set; }
    public string? NomeVendedor { get; set; }

    public List<ItemPedidoFV> Itens { get; set; } = new();
}
