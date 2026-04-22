using System.ComponentModel.DataAnnotations;

namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para filtrar pedidos FV
/// </summary>
public class BuscarPedidoFVRequest
{
    public int? IdVendedor { get; set; }
    public int? IdCliente { get; set; }
    /// <summary>0=Pendente, 1=Aprovado, 2=Faturado, 3=Cancelado. Null = todos</summary>
    public int? Status { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public int? Pagina { get; set; }
    public int? ItensPorPagina { get; set; }
}
