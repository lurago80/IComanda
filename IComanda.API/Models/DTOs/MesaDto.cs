namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para mesa
/// </summary>
public class MesaDto
{
    public int Numero { get; set; }
    public string Status { get; set; } = "LIVRE";
    public int? ComandaAtual { get; set; }
    public string? NotaAtual { get; set; }
    public DateTime? DataOcupacao { get; set; }
    public TimeSpan? HoraOcupacao { get; set; }
    public int? Operador { get; set; }
    public string? NomeOperador { get; set; }
    public int? NumeroPessoas { get; set; }
    public int? Cliente { get; set; }
    public string? NomeCliente { get; set; }
    public TimeSpan? TempoOcupacao { get; set; }
    public decimal? TotalAtual { get; set; }
    public int? QuantidadeItens { get; set; }
}

