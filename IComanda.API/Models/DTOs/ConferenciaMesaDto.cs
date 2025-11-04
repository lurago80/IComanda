namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para conferência de mesa/comanda - Visualização de pré-conta
/// </summary>
public class ConferenciaMesaDto
{
    public int? Mesa { get; set; }
    public int? Comanda { get; set; }
    public string? Garcom { get; set; }
    public DateTime DataHora { get; set; }
    public List<ItemConferenciaDto> Itens { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public int TotalItens { get; set; }
}

public class ItemConferenciaDto
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Total { get; set; }
}

