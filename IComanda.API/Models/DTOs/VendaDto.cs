namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para venda
/// </summary>
public class VendaDto
{
    public string Nota { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string Subserie { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public DateTime Emissao { get; set; }
    public TimeSpan Hora { get; set; }
    public int Cliente { get; set; }
    public DateTime DataSaida { get; set; }
    public TimeSpan HoraSaida { get; set; }
    public string FormasPgto { get; set; } = string.Empty;
    public decimal TotProdutos { get; set; }
    public decimal Total { get; set; }
    public int Operador { get; set; }
    public string Sequencia { get; set; } = string.Empty;
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public string Especie { get; set; } = string.Empty;
    public decimal Vale { get; set; }
    public decimal Dinheiro { get; set; }
    public decimal Cheque { get; set; }
    public decimal Cartao { get; set; }
    public decimal Boleto { get; set; }
    public decimal Troco { get; set; }
    public string Quantidade { get; set; } = string.Empty;
    public string Lancado { get; set; } = string.Empty;
    public int Vendedor { get; set; }
    public int Caixa { get; set; }
    public int? Comanda { get; set; }
    public int? Mesa { get; set; }
    public int? NumeroPessoas { get; set; }
    public List<ItemVendaDto> Itens { get; set; } = new();
}
