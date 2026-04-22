using System.ComponentModel.DataAnnotations;

namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para definir/atualizar a meta mensal de um vendedor
/// </summary>
public class DefinirMetaFVRequest
{
    [Required]
    public int IdVendedor { get; set; }

    [Required]
    [Range(1, 12, ErrorMessage = "Mês deve estar entre 1 e 12")]
    public int Mes { get; set; }

    [Required]
    [Range(2020, 2100)]
    public int Ano { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor de meta deve ser maior que zero")]
    public decimal ValorMeta { get; set; }
}
