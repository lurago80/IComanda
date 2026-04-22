namespace IComanda.API.Models.DTOs;

/// <summary>
/// Relatório consolidado: tudo que entrou e saiu do caixa por período (abertura, vendas, recebimentos, saídas).
/// </summary>
public class RelatorioCaixaConsolidadoDto
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    /// <summary>Movimentos de abertura de caixa (valor inicial).</summary>
    public List<MovimentoCaixaDto> Aberturas { get; set; } = new();

    /// <summary>Vendas fechadas (comandas) no período.</summary>
    public List<VendaResumoCaixaDto> Vendas { get; set; } = new();

    /// <summary>Pagamentos ao fechar comandas (RECEBIMENTO_VENDAS) – o que caiu no caixa por venda.</summary>
    public List<RecebimentoVendaResumoDto> RecebimentosVendas { get; set; } = new();

    /// <summary>Quitamentos de contas a receber (RECEBER) no período.</summary>
    public List<RecebimentoContaReceberDto> RecebimentosContasReceber { get; set; } = new();

    /// <summary>Saídas de caixa (movimentos com SAIDA > 0).</summary>
    public List<MovimentoCaixaDto> Saidas { get; set; } = new();

    public ResumoCaixaConsolidadoDto Resumo { get; set; } = new();
}

public class MovimentoCaixaDto
{
    public int Codigo { get; set; }
    public DateTime Data { get; set; }
    public TimeSpan Hora { get; set; }
    public int Terminal { get; set; }
    public decimal Entrada { get; set; }
    public decimal Saida { get; set; }
    public string? Origem { get; set; }
    public string? Historico { get; set; }
}

public class VendaResumoCaixaDto
{
    public string Nota { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public TimeSpan Hora { get; set; }
    public int? Comanda { get; set; }
    public int? Mesa { get; set; }
    public int Cliente { get; set; }
    public string? NomeCliente { get; set; }
    public decimal Total { get; set; }
    public int NCaixa { get; set; }
    /// <summary>BA = Comanda/Balcão, DL = Delivery</summary>
    public string? Origem { get; set; }
}

public class RecebimentoVendaResumoDto
{
    public int Id { get; set; }
    public string Nota { get; set; } = string.Empty;
    public DateTime DataVenda { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public decimal Troco { get; set; }
    public int NCaixa { get; set; }
}

public class RecebimentoContaReceberDto
{
    public string Numero { get; set; } = string.Empty;
    public string Ordem { get; set; } = string.Empty;
    public DateTime DataRecebimento { get; set; }
    public decimal ValorRecebido { get; set; }
    public string? Historico { get; set; }
    public string? FormaPagamento { get; set; }
    public int Terminal { get; set; }
}

/// <summary>Total por forma de pagamento (espécie).</summary>
public class TotalPorFormaDto
{
    public string Forma { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class ResumoCaixaConsolidadoDto
{
    public decimal TotalAbertura { get; set; }
    public decimal TotalVendas { get; set; }
    public decimal TotalRecebimentosVendas { get; set; }
    public decimal TotalRecebimentosContasReceber { get; set; }
    public decimal TotalEntradas => TotalAbertura + TotalRecebimentosVendas + TotalRecebimentosContasReceber;
    public decimal TotalSaidas { get; set; }
    public decimal SaldoPeriodo => TotalEntradas - TotalSaidas;
    public int QuantidadeAberturas { get; set; }
    public int QuantidadeVendas { get; set; }
    /// <summary>Quantidade de vendas Comanda/Balcão (BA)</summary>
    public int QuantidadeVendasComandas { get; set; }
    /// <summary>Valor total vendas Comanda/Balcão (BA)</summary>
    public decimal TotalVendasComandas { get; set; }
    /// <summary>Quantidade de vendas Delivery (DL)</summary>
    public int QuantidadeVendasDelivery { get; set; }
    /// <summary>Valor total vendas Delivery (DL)</summary>
    public decimal TotalVendasDelivery { get; set; }
    public int QuantidadeRecebimentosVendas { get; set; }
    public int QuantidadeRecebimentosContasReceber { get; set; }
    public int QuantidadeSaidas { get; set; }
    /// <summary>Total por forma de pagamento nos recebimentos de vendas (ao fechar comanda).</summary>
    public List<TotalPorFormaDto> TotalPorFormaRecebimentosVendas { get; set; } = new();
    /// <summary>Total por forma/espécie nos recebimentos de contas a receber.</summary>
    public List<TotalPorFormaDto> TotalPorFormaRecebimentosContas { get; set; } = new();
}
