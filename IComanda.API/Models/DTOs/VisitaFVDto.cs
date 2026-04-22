using IComanda.API.Models.Enums;

namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO de Visita de Força de Vendas
/// </summary>
public class VisitaFVDto
{
    public int Id { get; set; }
    public int IdVendedor { get; set; }
    public string NomeVendedor { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public DateTime DataAgendada { get; set; }
    public DateTime? DataCheckin { get; set; }
    public DateTime? DataCheckout { get; set; }
    public decimal? LatCheckin { get; set; }
    public decimal? LngCheckin { get; set; }
    public decimal? LatCheckout { get; set; }
    public decimal? LngCheckout { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StatusCodigo { get; set; }
    public string? Obs { get; set; }
    public string? Resultado { get; set; }
    public int? IdPedidoFV { get; set; }
}
