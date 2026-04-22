namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de mesa - status e ocupação
/// </summary>
public class Mesa
{
    public int Numero { get; set; }
    public string Status { get; set; } = "LIVRE"; // LIVRE, OCUPADA, RESERVADA
    public int? ComandaAtual { get; set; }
    public string? NotaAtual { get; set; }
    public DateTime? DataOcupacao { get; set; }
    public TimeSpan? HoraOcupacao { get; set; }
    public int? Operador { get; set; }
    public int? NumeroPessoas { get; set; }
    public int? Cliente { get; set; }
    public TimeSpan? TempoOcupacao { get; set; }
}

