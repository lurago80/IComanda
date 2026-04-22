namespace IComanda.API.Models.Enums;

/// <summary>
/// Status da visita de Força de Vendas
/// </summary>
public enum StatusVisitaFV
{
    /// <summary>Visita agendada, ainda não realizada</summary>
    Agendada = 0,

    /// <summary>Vendedor realizou check-in no cliente</summary>
    EmAndamento = 1,

    /// <summary>Visita concluída (checkout realizado)</summary>
    Concluida = 2,

    /// <summary>Visita não realizada (cliente ausente / cancelamento)</summary>
    NaoRealizada = 3
}
