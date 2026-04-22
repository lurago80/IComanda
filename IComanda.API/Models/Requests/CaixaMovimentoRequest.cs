namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para criar movimento de caixa
/// </summary>
public class CaixaMovimentoRequest
{
    public int Terminal { get; set; }
    public decimal Valor { get; set; }
    public string? Documento { get; set; }
    public string? Historico { get; set; }
    public int Operador { get; set; }
    public int? Custo { get; set; }
    public int? Conta { get; set; }
    public int? CodProf { get; set; }
    public int? Vendedor { get; set; }
}
