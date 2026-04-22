using System.ComponentModel.DataAnnotations;

namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para criar item dentro de um pedido FV
/// </summary>
public class ItemPedidoFVRequest
{
    [Required]
    public int IdProduto { get; set; }

    [Required]
    [Range(0.001, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
    public decimal Quantidade { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Preço unitário não pode ser negativo")]
    public decimal PrecoUnitario { get; set; }

    [Range(0, 100, ErrorMessage = "Desconto deve estar entre 0 e 100%")]
    public decimal Desconto { get; set; }

    public string? Obs { get; set; }
}
