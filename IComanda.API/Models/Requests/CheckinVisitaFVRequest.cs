namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para realizar check-in ou check-out de visita FV
/// </summary>
public class CheckinVisitaFVRequest
{
    /// <summary>Latitude atual do dispositivo (opcional, para geo-validação)</summary>
    public decimal? Latitude { get; set; }

    /// <summary>Longitude atual do dispositivo (opcional, para geo-validação)</summary>
    public decimal? Longitude { get; set; }

    public string? Obs { get; set; }
}
