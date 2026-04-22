namespace IComanda.API.Models.DTOs;

/// <summary>
/// Dashboard resumido para vendedor em campo
/// </summary>
public class DashboardVendedorDto
{
    public int IdVendedor { get; set; }
    public string NomeVendedor { get; set; } = string.Empty;

    // Meta do mês
    public decimal MetaMes { get; set; }
    public decimal VendasMes { get; set; }
    public decimal PercentualMeta { get; set; }
    public decimal ComissaoEstimada { get; set; }

    // Pedidos FV
    public int TotalPedidosPendentes { get; set; }
    public int TotalPedidosAprovados { get; set; }
    public int TotalPedidosFaturados { get; set; }
    public decimal ValorPedidosPendentes { get; set; }
    public decimal ValorPedidosAprovados { get; set; }

    // Agenda do dia
    public int VisitasAgendadasHoje { get; set; }
    public int VisitasRealizadasHoje { get; set; }
    public List<VisitaFVDto> ProximasVisitas { get; set; } = new();

    // Últimos pedidos
    public List<PedidoFVDto> UltimosPedidos { get; set; } = new();
}
