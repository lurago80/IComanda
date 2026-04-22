namespace IComanda.API.Models.Enums;

/// <summary>
/// Tipos de grupos de produto
/// </summary>
public enum TipoGrupo
{
    [System.ComponentModel.Description("Bebidas")]
    Bebidas = 1,
    
    [System.ComponentModel.Description("Petiscos")]
    Petiscos = 2,
    
    [System.ComponentModel.Description("Pratos Principais")]
    PratosPrincipais = 3,
    
    [System.ComponentModel.Description("Sobremesas")]
    Sobremesas = 4,
    
    [System.ComponentModel.Description("Extras")]
    Extras = 5
}
