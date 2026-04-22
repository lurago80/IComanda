namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para movimento de caixa
/// </summary>
public class CaixaMovimentoDto
{
    public int Codigo { get; set; }
    public DateTime Data { get; set; }
    public TimeSpan Hora { get; set; }
    public string? Documento { get; set; }
    public int? Custo { get; set; }
    public int? Conta { get; set; }
    public decimal Entrada { get; set; }
    public decimal Saida { get; set; }
    public decimal Saldo { get; set; }
    public string? Origem { get; set; }
    public int Operador { get; set; }
    public string? Historico { get; set; }
    public DateTime Gravacao { get; set; }
    public int? CodProf { get; set; }
    public int Terminal { get; set; }
    public int Vendedor { get; set; }
}

/// <summary>
/// DTO para resumo de caixa
/// </summary>
public class CaixaResumoDto
{
    public int Terminal { get; set; }
    public decimal SaldoAtual { get; set; }
    public decimal TotalEntradas { get; set; }
    public decimal TotalSaidas { get; set; }
    public int QuantidadeMovimentos { get; set; }
    public List<CaixaMovimentoDto> Movimentos { get; set; } = new();
}
