namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade ProdutoEmpresa baseada na tabela PRODUTOEMPRESA
/// </summary>
public class ProdutoEmpresa
{
    public int Id { get; set; }
    public decimal Quantidade { get; set; }
    public decimal? QuantidadeMinima { get; set; }
    public decimal? QuantidadeMaxima { get; set; }
    public string? Localizacao { get; set; }
    public DateTime? DataUltimaVenda { get; set; }
    public DateTime? DataUltimaCompra { get; set; }
    public string? ChavePaf { get; set; }
    public int? ProdutoTributacaoEstadualId { get; set; }
    public string? Fabricante { get; set; }
    public DateTime? UltimoReajuste { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public char? StatusSazional { get; set; }
}
