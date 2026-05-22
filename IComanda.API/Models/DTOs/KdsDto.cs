namespace IComanda.API.Models.DTOs;

public class KdsPedidoDto
{
    public string Nota { get; set; } = string.Empty;
    public string Origem { get; set; } = string.Empty;
    public int? Comanda { get; set; }
    public int? Mesa { get; set; }
    public string? NomeCliente { get; set; }
    public string StatusCozinha { get; set; } = "PENDENTE";
    public string Lancado { get; set; } = string.Empty;
    public DateTime Emissao { get; set; }
    public TimeSpan Hora { get; set; }
    public int MinutosEspera { get; set; }
    public List<KdsItemDto> Itens { get; set; } = new();
}

public class KdsItemDto
{
    public int Item { get; set; }
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public string? Observacao { get; set; }
}

public class AtualizarStatusKdsRequest
{
    public string StatusCozinha { get; set; } = string.Empty;
}
