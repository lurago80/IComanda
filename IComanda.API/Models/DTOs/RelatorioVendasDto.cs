namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para relatório de vendas
/// </summary>
public class RelatorioVendasDto
{
    public DateTime Data { get; set; }
    public int TotalVendas { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TicketMedio { get; set; }
    public int TotalItens { get; set; }
    public decimal TotalDinheiro { get; set; }
    public decimal TotalCartao { get; set; }
    public decimal TotalPix { get; set; }
    public decimal TotalCheque { get; set; }
    public decimal TotalBoleto { get; set; }
    /// <summary>Quantidade de vendas Comanda/Balcão (BA)</summary>
    public int TotalVendasComandas { get; set; }
    /// <summary>Valor total das vendas Comanda/Balcão (BA)</summary>
    public decimal ValorTotalComandas { get; set; }
    /// <summary>Quantidade de vendas Delivery (DL)</summary>
    public int TotalVendasDelivery { get; set; }
    /// <summary>Valor total das vendas Delivery (DL)</summary>
    public decimal ValorTotalDelivery { get; set; }
    public List<VendaResumoDto> Vendas { get; set; } = new();
}

/// <summary>
/// DTO para resumo de venda
/// </summary>
public class VendaResumoDto
{
    public string Nota { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public TimeSpan Hora { get; set; }
    public int Cliente { get; set; }
    public string? NomeCliente { get; set; }
    public decimal Total { get; set; }
    public int QuantidadeItens { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public int Operador { get; set; }
    public int? Mesa { get; set; }
    public int? Comanda { get; set; }
    /// <summary>BA = Comanda/Balcão, DL = Delivery</summary>
    public string? Origem { get; set; }
}

