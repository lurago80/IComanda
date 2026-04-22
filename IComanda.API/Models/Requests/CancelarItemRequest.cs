namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para cancelar um item de venda
/// </summary>
public class CancelarItemRequest
{
    /// <summary>
    /// Número da nota da venda
    /// </summary>
    public string Nota { get; set; } = string.Empty;

    /// <summary>
    /// Número do item a ser cancelado
    /// </summary>
    public int Item { get; set; }

    /// <summary>
    /// ID do operador que está cancelando
    /// </summary>
    public int Operador { get; set; }

    /// <summary>
    /// Motivo do cancelamento (opcional)
    /// </summary>
    public string? Motivo { get; set; }
}

