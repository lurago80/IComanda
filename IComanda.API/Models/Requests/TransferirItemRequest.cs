namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para transferir um item de uma comanda para outra
/// </summary>
public class TransferirItemRequest
{
    /// <summary>
    /// Nota da venda de origem (comanda de onde o item será removido)
    /// </summary>
    public string NotaOrigem { get; set; } = string.Empty;

    /// <summary>
    /// Número do item na venda de origem (Item sequencial)
    /// </summary>
    public int ItemOrigem { get; set; }

    /// <summary>
    /// Nota da venda de destino (comanda para onde o item será transferido)
    /// </summary>
    public string NotaDestino { get; set; } = string.Empty;

    /// <summary>
    /// ID do operador que está realizando a transferência
    /// </summary>
    public int Operador { get; set; } = 1;
}

