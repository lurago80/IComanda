using System.ComponentModel.DataAnnotations;

namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para agendar uma visita FV
/// </summary>
public class AgendarVisitaFVRequest
{
    [Required(ErrorMessage = "Vendedor é obrigatório")]
    public int IdVendedor { get; set; }

    [Required(ErrorMessage = "Cliente é obrigatório")]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "Data agendada é obrigatória")]
    public DateTime DataAgendada { get; set; }

    public string? Obs { get; set; }
}
