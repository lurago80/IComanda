namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para fechar caixa
/// </summary>
public class FecharCaixaRequest
{
    /// <summary>
    /// ID do caixa
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID do operador que está fechando o caixa
    /// </summary>
    public int Operador { get; set; }

    /// <summary>
    /// Valor final contado no caixa (dinheiro em espécie)
    /// </summary>
    public decimal ValorFechamento { get; set; }

    /// <summary>
    /// Observações (opcional)
    /// </summary>
    public string? Observacoes { get; set; }
}

