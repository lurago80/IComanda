namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para item de venda
/// </summary>
public class ItemVendaDto
{
    public string Nota { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Subserie { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateTime Emissao { get; set; }
    public int Item { get; set; }
    public int Codigo { get; set; }
    public string Barras { get; set; } = string.Empty;
    public int Cfop { get; set; }
    public string St { get; set; } = string.Empty;
    public string Und { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal Preco { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string Cancelado { get; set; } = string.Empty;
    public int Sequencia { get; set; }
    public decimal PrecoCusto { get; set; }
    public string Serial { get; set; } = string.Empty;
    public decimal Icms { get; set; }
    public int Sinalm { get; set; }
}
