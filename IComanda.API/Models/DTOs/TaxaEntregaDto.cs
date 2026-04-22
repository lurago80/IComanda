namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para taxa de entrega
/// </summary>
public class TaxaEntregaDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
