namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para quitar uma conta a receber
/// </summary>
public class QuitarReceberRequest
{
    /// <summary>
    /// Número da conta
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Ordem da conta
    /// </summary>
    public string Ordem { get; set; } = string.Empty;

    /// <summary>
    /// Valor a receber (pode ser parcial)
    /// </summary>
    public decimal ValorRecebido { get; set; }

    /// <summary>
    /// Data do recebimento (opcional, usa data atual se não informado)
    /// </summary>
    public DateTime? DataRecebimento { get; set; }

    /// <summary>
    /// ID do operador que está recebendo (opcional)
    /// </summary>
    public int? Operador { get; set; }

    /// <summary>
    /// Desconto aplicado (opcional)
    /// </summary>
    public decimal Desconto { get; set; } = 0;

    /// <summary>
    /// Juros aplicados (opcional)
    /// </summary>
    public decimal Juros { get; set; } = 0;

    /// <summary>
    /// Forma de pagamento do recebimento (obsoleto - usar FormasPagamento)
    /// </summary>
    public int? IdFormaPagamento { get; set; }

    /// <summary>
    /// Lista de formas de pagamento (cada uma com seu valor)
    /// </summary>
    public List<FormaPagamentoRecebimento>? FormasPagamento { get; set; }
}

/// <summary>
/// Forma de pagamento com valor específico
/// </summary>
public class FormaPagamentoRecebimento
{
    /// <summary>
    /// ID da forma de pagamento
    /// </summary>
    public int IdFormaPagamento { get; set; }

    /// <summary>
    /// Valor recebido nesta forma de pagamento
    /// </summary>
    public decimal Valor { get; set; }
}

