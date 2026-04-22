namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO de Meta de Força de Vendas
/// </summary>
public class MetaFVDto
{
    public int Id { get; set; }
    public int IdVendedor { get; set; }
    public string NomeVendedor { get; set; } = string.Empty;
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal ValorMeta { get; set; }
    public decimal ValorRealizado { get; set; }
    public decimal PercentualAtingido { get; set; }
    public bool MetaAtingida { get; set; }
}
