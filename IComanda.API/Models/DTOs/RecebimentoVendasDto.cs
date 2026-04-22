namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para recebimento de vendas
/// </summary>
public class RecebimentoVendasDto
{
    public int Id { get; set; }
    public int IdFormaPagamento { get; set; }
    public string FormaPagamentoDescricao { get; set; } = string.Empty;
    public int NCaixa { get; set; }
    public string Nota { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Troco { get; set; }
}

