namespace IComanda.API.Models.Entities;

/// <summary>
/// Meta mensal de vendas por vendedor — tabela METAS_FV
/// </summary>
public class MetaFV
{
    public int Id { get; set; }
    public int IdVendedor { get; set; }
    public int Mes { get; set; }
    public int Ano { get; set; }
    public decimal ValorMeta { get; set; }
    public decimal ValorRealizado { get; set; }
    public string? NomeVendedor { get; set; }

    // Propriedades calculadas
    public decimal PercentualAtingido => ValorMeta > 0
        ? Math.Round((ValorRealizado / ValorMeta) * 100, 2)
        : 0;

    public bool MetaAtingida => ValorRealizado >= ValorMeta;
}
