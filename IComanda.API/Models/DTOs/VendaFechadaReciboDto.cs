namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para listar vendas fechadas/recebidas no período (reimpressão de recibos).
/// </summary>
public class VendaFechadaReciboDto
{
    public string Nota { get; set; } = string.Empty;
    public int? Comanda { get; set; }
    public decimal Total { get; set; }
    public DateTime Emissao { get; set; }
    public string? NomeCliente { get; set; }
    public List<RecebimentoVendasDto> Recebimentos { get; set; } = new();
}
