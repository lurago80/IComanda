namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para concluir/encerrar visita FV com resultado
/// </summary>
public class ConcluirVisitaFVRequest
{
    /// <summary>Resultado da visita: "PEDIDO_EFETUADO", "SEM_INTERESSE", "RETORNAR", "AUSENTE"</summary>
    public string? Resultado { get; set; }

    public string? Obs { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    /// <summary>ID do pedido que foi gerado durante esta visita (opcional)</summary>
    public int? IdPedidoFV { get; set; }
}
