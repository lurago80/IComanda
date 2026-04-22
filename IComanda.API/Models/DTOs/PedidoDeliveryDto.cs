namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para pedido de delivery - visão simplificada focada em entrega
/// </summary>
public class PedidoDeliveryDto
{
    /// <summary>
    /// Número da nota/pedido
    /// </summary>
    public string Nota { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora do pedido
    /// </summary>
    public DateTime DataHora { get; set; }

    /// <summary>
    /// ID do cliente (obrigatório para delivery)
    /// </summary>
    public int Cliente { get; set; }

    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string? NomeCliente { get; set; }

    /// <summary>
    /// Telefone/celular do cliente para contato
    /// </summary>
    public string? TelefoneCliente { get; set; }

    /// <summary>
    /// Endereço completo de entrega (rua, número, bairro, cidade, CEP)
    /// </summary>
    public string? EnderecoEntrega { get; set; }

    /// <summary>
    /// Total dos produtos (sem desconto/acréscimo)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Desconto aplicado
    /// </summary>
    public decimal Desconto { get; set; }

    /// <summary>
    /// Acréscimo (ex: taxa de entrega)
    /// </summary>
    public decimal Acrescimo { get; set; }

    /// <summary>
    /// Total do pedido (Subtotal - Desconto + Acréscimo)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Forma de pagamento acordada
    /// </summary>
    public string? FormasPgto { get; set; }

    /// <summary>
    /// Quantidae de itens no pedido
    /// </summary>
    public int TotalItens { get; set; }

    /// <summary>
    /// Status do pedido (ABERTO, EFETIVADO, CANCELADO)
    /// </summary>
    public string Lancado { get; set; } = "ABERTO";

    /// <summary>
    /// Operador que criou o pedido
    /// </summary>
    public int Operador { get; set; }

    /// <summary>
    /// Itens do pedido
    /// </summary>
    public List<ItemPedidoDeliveryDto> Itens { get; set; } = new();
}

/// <summary>
/// DTO para item de pedido delivery - visão simplificada
/// </summary>
public class ItemPedidoDeliveryDto
{
    /// <summary>
    /// Código do produto
    /// </summary>
    public int Codigo { get; set; }

    /// <summary>
    /// Descrição do produto
    /// </summary>
    public string? Descricao { get; set; }

    /// <summary>
    /// Quantidade do item
    /// </summary>
    public decimal Quantidade { get; set; }

    /// <summary>
    /// Preço unitário
    /// </summary>
    public decimal Preco { get; set; }

    /// <summary>
    /// Total do item (Quantidade * Preco)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Observações especiais do item (ex: "sem cebola", "bem passado")
    /// </summary>
    public string? Observacao { get; set; }
}
