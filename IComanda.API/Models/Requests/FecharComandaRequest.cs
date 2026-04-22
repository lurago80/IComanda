namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para fechar comanda com recebimentos
/// </summary>
public class FecharComandaRequest
{
    /// <summary>
    /// Número da comanda (usado quando não for PDV / caixa rápido)
    /// </summary>
    public int Comanda { get; set; }

    /// <summary>
    /// Nota do cupom (usado para Caixa Rápido / PDV - venda sem comanda, cliente 0).
    /// Quando informado, a venda é localizada pela nota em vez da comanda.
    /// </summary>
    public string? Nota { get; set; }

    /// <summary>
    /// Lista de recebimentos (formas de pagamento e valores)
    /// </summary>
    public List<RecebimentoItemRequest> Recebimentos { get; set; } = new();

    /// <summary>
    /// Valor total do troco (se houver)
    /// </summary>
    public decimal Troco { get; set; } = 0;
}

/// <summary>
/// Item de recebimento individual
/// </summary>
public class RecebimentoItemRequest
{
    /// <summary>
    /// ID da forma de pagamento
    /// </summary>
    public int IdFormaPagamento { get; set; }

    /// <summary>
    /// Valor do recebimento
    /// </summary>
    public decimal Valor { get; set; }

    /// <summary>
    /// Troco para esta forma de pagamento (se houver)
    /// </summary>
    public decimal Troco { get; set; } = 0;
}

