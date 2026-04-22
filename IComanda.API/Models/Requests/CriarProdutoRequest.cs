namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para criar um novo produto
/// </summary>
public class CriarProdutoRequest
{
    // Campos principais obrigatórios
    public string Descricao { get; set; } = string.Empty;
    public int TipoItemId { get; set; }
    public int UnidadeMedidaId { get; set; }
    public int PessoaId { get; set; }

    // Campos opcionais - PRODUTO
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

    // Campos opcionais - PRODUTOEMPRESA
    public decimal? Quantidade { get; set; }
    public decimal? QuantidadeMinima { get; set; }
    public decimal? QuantidadeMaxima { get; set; }
    public string? Localizacao { get; set; }
    public string? Fabricante { get; set; }
    public char? StatusSazional { get; set; }

    // Campos opcionais - PRODUTOESERVICOEMPRESA
    public decimal? PrecoVenda { get; set; }
    public short? Vendavel { get; set; }
    public decimal? PrecoCusto { get; set; }
    public decimal? PrecoCustoMedio { get; set; }
    public decimal? LimiteDesconto { get; set; }
    public decimal? Atacado { get; set; }
    public decimal? Preco3 { get; set; }
    public decimal? PrecoDolar { get; set; }
    public decimal? Percentual { get; set; }

    // Campos opcionais - PRODUTOESERVICO
    public string? Caracteristica { get; set; }
    public string? CodigoInterno { get; set; }
    public short? Ativo { get; set; }
    public string? Observacao { get; set; }
    public int? SubgrupoId { get; set; }
    public int? GeneroItemId { get; set; }
    public int? Grupo { get; set; }
    public string? UnMedida { get; set; }
    public string? Cfop { get; set; }
    public string? Csosn { get; set; }
    public string? CEST { get; set; }
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
    public string? Categoria { get; set; }
    public string? Marca { get; set; }
    public string? Tipo { get; set; }
    public string? Ncm { get; set; }
    public string? Cst { get; set; }
    public string? CstOrigem { get; set; }
    public decimal? Icms { get; set; }
    public decimal? Margem { get; set; }
}
