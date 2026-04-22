using IComanda.API.Models.Enums;

namespace IComanda.API.Models.Entities;

/// <summary>
/// Visita de Força de Vendas — tabela VISITAS_FV
/// </summary>
public class VisitaFV
{
    public int Id { get; set; }
    public int IdVendedor { get; set; }
    public int IdCliente { get; set; }
    /// <summary>Data/hora agendada da visita</summary>
    public DateTime DataAgendada { get; set; }
    /// <summary>Data/hora real do check-in</summary>
    public DateTime? DataCheckin { get; set; }
    /// <summary>Data/hora real do check-out</summary>
    public DateTime? DataCheckout { get; set; }
    /// <summary>Latitude do check-in</summary>
    public decimal? LatCheckin { get; set; }
    /// <summary>Longitude do check-in</summary>
    public decimal? LngCheckin { get; set; }
    /// <summary>Latitude do check-out</summary>
    public decimal? LatCheckout { get; set; }
    /// <summary>Longitude do check-out</summary>
    public decimal? LngCheckout { get; set; }
    public StatusVisitaFV Status { get; set; } = StatusVisitaFV.Agendada;
    public string? Obs { get; set; }
    /// <summary>Resultado da visita (pedido efetuado, sem interesse, retornar, etc.)</summary>
    public string? Resultado { get; set; }
    /// <summary>ID do pedido FV gerado nesta visita</summary>
    public int? IdPedidoFV { get; set; }

    // Dados desnormalizados
    public string? NomeCliente { get; set; }
    public string? NomeVendedor { get; set; }
}
