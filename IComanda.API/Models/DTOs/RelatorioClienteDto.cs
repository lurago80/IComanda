namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para relatório de compras do cliente
/// </summary>
public class RelatorioClienteDto
{
    public int CodigoCliente { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public int TotalCompras { get; set; }
    public decimal ValorTotalPago { get; set; }
    public decimal TicketMedio { get; set; }
    public DateTime? PrimeiraCompra { get; set; }
    public DateTime? UltimaCompra { get; set; }
    public List<CompraClienteDto> Compras { get; set; } = new();
}

/// <summary>
/// DTO para compra individual do cliente
/// </summary>
public class CompraClienteDto
{
    public string Nota { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public TimeSpan Hora { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? Mesa { get; set; }
    public int? Comanda { get; set; }
    public int QuantidadeItens { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    /// <summary>BA = Comanda/Balcão, DL = Delivery</summary>
    public string? Origem { get; set; }
}

