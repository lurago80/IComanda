namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para abrir caixa
/// </summary>
public class AbrirCaixaRequest
{
    /// <summary>
    /// Número do caixa
    /// </summary>
    public int Numero { get; set; }

    /// <summary>
    /// ID do operador que está abrindo o caixa
    /// </summary>
    public int Operador { get; set; }

    /// <summary>
    /// Valor inicial do caixa (dinheiro em espécie)
    /// </summary>
    public decimal ValorAbertura { get; set; } = 0;

    /// <summary>
    /// Observações (opcional)
    /// </summary>
    public string? Observacoes { get; set; }
}

