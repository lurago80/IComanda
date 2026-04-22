namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de movimento de caixa - tabela CAIXA
/// </summary>
public class CaixaMovimento
{
    public int Codigo { get; set; }
    public DateTime Data { get; set; } = DateTime.Now;
    public TimeSpan Hora { get; set; } = DateTime.Now.TimeOfDay;
    public string? Documento { get; set; }
    public int? Custo { get; set; }
    public int? Conta { get; set; }
    public decimal Entrada { get; set; } = 0;
    public decimal Saida { get; set; } = 0;
    public decimal Saldo { get; set; } = 0;
    public string? Origem { get; set; }
    public int Operador { get; set; } = 0;
    public string? Historico { get; set; }
    public DateTime Gravacao { get; set; } = DateTime.Now;
    public int? CodProf { get; set; }
    public int Terminal { get; set; } = 0;
    public int Vendedor { get; set; } = 0;
}
