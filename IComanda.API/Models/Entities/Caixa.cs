namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de caixa - controle de abertura/fechamento
/// </summary>
public class Caixa
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public int OperadorAbertura { get; set; }
    public int? OperadorFechamento { get; set; }
    public decimal ValorAbertura { get; set; }
    public decimal? ValorFechamento { get; set; }
    public string Status { get; set; } = "ABERTO"; // ABERTO, FECHADO
    public string? Observacoes { get; set; }
}

