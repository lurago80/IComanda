namespace IComanda.API.Models.Entities;

/// <summary>
/// Comissão de operador/garçom por período
/// </summary>
public class OperadorComissao
{
    public int Id { get; set; }
    public int OperadorId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public decimal ValorVendas { get; set; }
    public decimal Percentual { get; set; }
    public decimal ValorComissao { get; set; }
}
