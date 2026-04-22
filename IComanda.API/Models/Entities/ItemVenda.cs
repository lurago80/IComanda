namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de item de venda - tabela ITEVENDAS
/// </summary>
public class ItemVenda
{
    public string Nota { get; set; } = string.Empty;
    public string Modelo { get; set; } = "D2";
    public string Serie { get; set; } = string.Empty;
    public string Subserie { get; set; } = string.Empty;
    public string Origem { get; set; } = "BA";
    public DateTime Emissao { get; set; } = DateTime.Now;
    public int Item { get; set; }
    public int Codigo { get; set; }
    public string Barras { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Cfop { get; set; } = 5102;
    public string St { get; set; } = "0000";
    public string Und { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal Preco { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string Cancelado { get; set; } = "0";
    public int Sequencia { get; set; }
    public decimal PrecoCusto { get; set; }
    public string Serial { get; set; } = string.Empty;
    public decimal Icms { get; set; }
    public int Sinalm { get; set; } = -1; // -1 para baixa de estoque
}
