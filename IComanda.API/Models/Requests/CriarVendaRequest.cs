namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para criar uma nova venda
/// </summary>
public class CriarVendaRequest
{
    /// <summary>
    /// ID do cliente
    /// </summary>
    public int Cliente { get; set; }

    /// <summary>
    /// Forma de pagamento
    /// </summary>
    public string FormasPgto { get; set; } = string.Empty;

    /// <summary>
    /// Total dos produtos
    /// </summary>
    public decimal TotProdutos { get; set; }

    /// <summary>
    /// Total da venda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// ID do operador
    /// </summary>
    public int Operador { get; set; }

    /// <summary>
    /// Desconto aplicado
    /// </summary>
    public decimal Desconto { get; set; }

    /// <summary>
    /// Acréscimo aplicado
    /// </summary>
    public decimal Acrescimo { get; set; }

    /// <summary>
    /// Espécie do pagamento (DINHEIRO, CARTAO, CHEQUE, BOLETO)
    /// </summary>
    public string Especie { get; set; } = string.Empty;

    /// <summary>
    /// Valor em dinheiro
    /// </summary>
    public decimal Dinheiro { get; set; }

    /// <summary>
    /// Valor em cheque
    /// </summary>
    public decimal Cheque { get; set; }

    /// <summary>
    /// Valor em cartão
    /// </summary>
    public decimal Cartao { get; set; }

    /// <summary>
    /// Valor em boleto
    /// </summary>
    public decimal Boleto { get; set; }

    /// <summary>
    /// Troco
    /// </summary>
    public decimal Troco { get; set; }

    /// <summary>
    /// ID do vendedor
    /// </summary>
    public int Vendedor { get; set; }

    /// <summary>
    /// ID do caixa
    /// </summary>
    public int Caixa { get; set; }

    /// <summary>
    /// Número da comanda
    /// </summary>
    public int? Comanda { get; set; }

    /// <summary>
    /// Número da mesa
    /// </summary>
    public int? Mesa { get; set; }

    /// <summary>
    /// Número de pessoas
    /// </summary>
    public int? NumeroPessoas { get; set; }

    /// <summary>
    /// Itens da venda
    /// </summary>
    public List<CriarItemVendaRequest> Itens { get; set; } = new();
}
