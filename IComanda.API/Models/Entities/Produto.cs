namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de produto baseado nas 4 tabelas do sistema legado:
/// PRODUTO + PRODUTOEMPRESA + PRODUTOESERVICO + PRODUTOESERVICOEMPRESA
/// </summary>
public class Produto
{
    public int Id { get; set; }
    public string? CodigoBarra { get; set; }
    public string? CodigoInterno { get; set; }
    public string? Descricao { get; set; }
    public string? Caracteristica { get; set; }
    public decimal? Quantidade { get; set; }
    public decimal? QuantidadeMinima { get; set; }
    public string? Localizacao { get; set; }
    public decimal? PrecoCusto { get; set; }
    public decimal? PrecoVenda { get; set; }
    public decimal? Atacado { get; set; }
    public decimal? Preco3 { get; set; }
    public string? UnMedida { get; set; }
    public short? Ativo { get; set; }
    public int? Grupo { get; set; }
    public short? Pesavel { get; set; }
    public string? Marca { get; set; }
    public string? Categoria { get; set; }
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
    public DateTime? DataInclusao { get; set; }
    public DateTime? DataUltimaVenda { get; set; }
    public DateTime? DataUltimaCompra { get; set; }
}
