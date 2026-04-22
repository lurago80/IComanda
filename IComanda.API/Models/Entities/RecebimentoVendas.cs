namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de recebimento de vendas - tabela RECEBIMENTO_VENDAS
/// </summary>
public class RecebimentoVendas
{
    public int Id { get; set; }
    public int IdFormaPagamento { get; set; }
    public int NCaixa { get; set; }
    public string Nota { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Troco { get; set; } = 0;
}

