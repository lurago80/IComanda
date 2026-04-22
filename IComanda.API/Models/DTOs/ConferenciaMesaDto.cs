namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para conferência de mesa/comanda - Visualização de pré-conta
/// </summary>
public class ConferenciaMesaDto
{
    public string Nota { get; set; } = string.Empty;
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
    public ClienteConferenciaDto? Cliente { get; set; }
}

public class ClienteConferenciaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
}

public class ItemConferenciaDto
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Total { get; set; }
    /// <summary>Observação do item (ex: "sem sal", "bem passado")</summary>
    public string? Observacao { get; set; }
    /// <summary>Hora em que o item foi lançado (HH:mm:ss) - para conferência</summary>
    public string? HoraLancamento { get; set; }
}

