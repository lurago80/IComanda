namespace IComanda.API.Models.Enums;

/// <summary>
/// Estados possíveis de uma mesa
/// </summary>
public enum MesaStatus
{
    [System.ComponentModel.Description("Mesa disponível")]
    Livre = 0,
    
    [System.ComponentModel.Description("Mesa com cliente")]
    Ocupada = 1,
    
    [System.ComponentModel.Description("Mesa reservada")]
    Reservada = 2,
    
    [System.ComponentModel.Description("Mesa em manutenção")]
    Manutencao = 3,
    
    [System.ComponentModel.Description("Mesa bloqueada")]
    Bloqueada = 4
}
