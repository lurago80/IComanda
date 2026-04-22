namespace IComanda.API.Models.Requests;

public class ImprimirPedidoRequest
{
    public List<ItemImpressao> Itens { get; set; } = new();
    public bool ApenasNovosItens { get; set; } // true = apenas novos itens (atualização), false = todo pedido (finalização)
    public string? Comanda { get; set; }
    public string? Mesa { get; set; }
    public string? ClienteNome { get; set; }
    public decimal? Subtotal { get; set; } // Para extrato
    public decimal? Desconto { get; set; } // Para extrato
    public decimal? Acrescimo { get; set; } // Para extrato
    public bool IsExtrato { get; set; } = false; // Indica se é extrato ou pedido

    /// <summary>Indica se é recibo do cliente (fechamento/recebimento) com formas de pagamento e troco.</summary>
    public bool IsReciboCliente { get; set; } = false;
    /// <summary>Formas de pagamento no recibo (ex: DINHEIRO, PIX, CARTÃO).</summary>
    public List<FormaPagamentoRecibo> FormasPagamento { get; set; } = new();
    /// <summary>Troco total a exibir no recibo (se houver).</summary>
    public decimal? TrocoTotal { get; set; }

    /// <summary>Quando true, imprime cupom para o entregador com endereço e observações.</summary>
    public bool IsCupomDelivery { get; set; }

    /// <summary>Endereço completo de entrega (para cupom delivery).</summary>
    public string? EnderecoEntrega { get; set; }

    /// <summary>Ponto de referência do cliente (COMPL1 / Complemento1), impresso separado do endereço.</summary>
    public string? PontoReferencia { get; set; }

    /// <summary>Telefone do cliente para contato na entrega.</summary>
    public string? TelefoneEntrega { get; set; }

    /// <summary>Observações gerais do pedido (para cupom delivery).</summary>
    public string? ObservacoesPedido { get; set; }

    /// <summary>Forma de pagamento do delivery (ex: DINHEIRO, PIX, CARTÃO).</summary>
    public string? FormaPgtoDelivery { get; set; }

    /// <summary>Indica se o pedido delivery já foi pago (true) ou cobrar na entrega (false).</summary>
    public bool JaPagoDelivery { get; set; } = false;

    /// <summary>
    /// Quando true e a configuração global HabilitarImprimirDuasVias estiver ativa,
    /// imprime 2 vias automaticamente sem perguntar ao usuário.
    /// </summary>
    public bool ImprimirDuasVias { get; set; } = false;

    /// <summary>Título de seção a exibir no cabeçalho do cupom (ex: "**** COZINHA ****"). Quando preenchido, substitui "COMANDA EM ABERTO".</summary>
    public string? TituloSecao { get; set; }
}

public class FormaPagamentoRecibo
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

public class ItemImpressao
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
    public string? Observacao { get; set; }
}

