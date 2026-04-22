namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade Produto baseada na tabela PRODUTO
/// </summary>
public class ProdutoEntity
{
    public int Id { get; set; }
    public string? CodigoBarra { get; set; }
    public string? PadraoBarra { get; set; }
    public byte[]? Imagem { get; set; }
    public string? Classificacao { get; set; }
    public int? DiasValidade { get; set; }
    public decimal? PesoLiquido { get; set; }
    public decimal? PesoBruto { get; set; }
    public string? TipoValidade { get; set; }
    public short? Pesavel { get; set; }
    public int? ComposicaoId { get; set; }
    public int? MarcaId { get; set; }
    public int? MedicamentoId { get; set; }
    public int? CombustivelId { get; set; }
    public string? Impressao { get; set; }
    public string? CodigoBarras1 { get; set; }
}
