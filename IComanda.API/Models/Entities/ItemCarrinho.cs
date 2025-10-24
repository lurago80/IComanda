namespace IComanda.API.Models.Entities;

/// <summary>
/// Item do carrinho temporário - tabela FRENTE_TMPITVENDAS
/// </summary>
public class ItemCarrinho
{
    public int Id { get; set; }
    public string Cupom { get; set; } = string.Empty;
    public string NCaixa { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public TimeSpan Hora { get; set; }
    public int Operador { get; set; }
    public int Item { get; set; }
    public int Codigo { get; set; }
    public string Barras { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal Preco { get; set; }
    public string Tributacao { get; set; } = string.Empty;
    public decimal Icms { get; set; }
    public decimal Iss { get; set; }
    public string Und { get; set; } = string.Empty;
    public decimal Desconto { get; set; }
    public decimal Desconto1 { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string Serial { get; set; } = string.Empty;
    public decimal PrecoCusto { get; set; }
    public decimal QtdBaixa { get; set; }
    public int Tipo { get; set; } = 1;
}
