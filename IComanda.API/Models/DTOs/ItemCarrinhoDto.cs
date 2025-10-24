namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para item do carrinho
/// </summary>
public class ItemCarrinhoDto
{
    public int Id { get; set; }
    public string Cupom { get; set; } = string.Empty;
    public int Operador { get; set; }
    public int Item { get; set; }
    public int Codigo { get; set; }
    public string Barras { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal Preco { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string Und { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public TimeSpan Hora { get; set; }
}
