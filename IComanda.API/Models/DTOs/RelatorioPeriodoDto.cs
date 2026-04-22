namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para relatório por período com itens vendidos e recebimentos
/// </summary>
public class RelatorioPeriodoDto
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<ItemVendidoDto> ItensVendidos { get; set; } = new();
    public List<RecebimentoPorFormaPagamentoDto> RecebimentosPorFormaPagamento { get; set; } = new();
    public ResumoRelatorioPeriodoDto Resumo { get; set; } = new();
}

/// <summary>
/// DTO para item vendido no relatório
/// </summary>
public class ItemVendidoDto
{
    public string Nota { get; set; } = string.Empty;
    public DateTime Emissao { get; set; }
    public TimeSpan Hora { get; set; }
    public int Item { get; set; }
    public int CodigoProduto { get; set; }
    public string? DescricaoProduto { get; set; }
    public string Barras { get; set; } = string.Empty;
    public string Und { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal Preco { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public int Cliente { get; set; }
    public string? NomeCliente { get; set; }
    public int? Mesa { get; set; }
    public int? Comanda { get; set; }
    /// <summary>BA = Comanda/Balcão, DL = Delivery</summary>
    public string? Origem { get; set; }
}

/// <summary>
/// DTO para recebimento agrupado por forma de pagamento
/// </summary>
public class RecebimentoPorFormaPagamentoDto
{
    public int IdFormaPagamento { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal TrocoTotal { get; set; }
    public List<RecebimentoDetalheDto> Detalhes { get; set; } = new();
}

/// <summary>
/// DTO para detalhe de recebimento
/// </summary>
public class RecebimentoDetalheDto
{
    public int Id { get; set; }
    public string Nota { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; }
    public TimeSpan HoraVenda { get; set; }
    public decimal Valor { get; set; }
    public decimal Troco { get; set; }
    public int NCaixa { get; set; }
    public int Cliente { get; set; }
    public string? NomeCliente { get; set; }
}

/// <summary>
/// DTO para resumo do relatório
/// </summary>
public class ResumoRelatorioPeriodoDto
{
    public int TotalItensVendidos { get; set; }
    public decimal ValorTotalItens { get; set; }
    public decimal TotalDesconto { get; set; }
    public decimal TotalAcrescimo { get; set; }
    public int TotalRecebimentos { get; set; }
    public decimal ValorTotalRecebimentos { get; set; }
    public decimal ValorTotalTroco { get; set; }
    /// <summary>Valor total itens vendidos Comanda/Balcão (BA)</summary>
    public decimal ValorTotalComandas { get; set; }
    /// <summary>Valor total itens vendidos Delivery (DL)</summary>
    public decimal ValorTotalDelivery { get; set; }
    public Dictionary<string, decimal> TotalPorFormaPagamento { get; set; } = new();
}
