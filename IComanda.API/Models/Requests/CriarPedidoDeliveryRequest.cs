namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para criar um novo pedido de delivery (variante de CriarVendaRequest com validações específicas)
/// </summary>
public class CriarPedidoDeliveryRequest
{
    /// <summary>
    /// ID do cliente - OBRIGATÓRIO para delivery
    /// </summary>
    public int Cliente { get; set; }

    /// <summary>
    /// Forma de pagamento (ex: "DINHEIRO", "CARTAO", "PIX", "A PRAZO")
    /// </summary>
    public string FormasPgto { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da forma (ex: "DINHEIRO", "CARTAO", etc.)
    /// </summary>
    public string Especie { get; set; } = "DINHEIRO";

    /// <summary>
    /// Valor em dinheiro (se pago na entrega)
    /// </summary>
    public decimal Dinheiro { get; set; }

    /// <summary>
    /// Valor em cartão
    /// </summary>
    public decimal Cartao { get; set; }

    /// <summary>
    /// Valor em PIX
    /// </summary>
    public decimal Pix { get; set; }

    /// <summary>
    /// Desconto aplicado ao pedido (ex: cupom, promoção)
    /// </summary>
    public decimal Desconto { get; set; }

    /// <summary>
    /// Acréscimo (ex: taxa de entrega, embalagem)
    /// </summary>
    public decimal Acrescimo { get; set; }

    /// <summary>
    /// Troco (se aplicável)
    /// </summary>
    public decimal Troco { get; set; }

    /// <summary>
    /// ID do vendedor/operador que está criando o pedido
    /// </summary>
    public int Vendedor { get; set; } = 1;

    /// <summary>
    /// ID do operador
    /// </summary>
    public int Operador { get; set; } = 1;

    /// <summary>
    /// Itens do pedido
    /// </summary>
    public List<CriarItemVendaRequest> Itens { get; set; } = new();
}
