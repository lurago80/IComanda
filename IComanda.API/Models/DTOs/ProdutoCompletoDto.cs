namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO completo para produto - combina dados das 4 tabelas
/// </summary>
public class ProdutoCompletoDto
{
    // Campos da tabela PRODUTO
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

    // Campos da tabela PRODUTOEMPRESA
    public decimal? Quantidade { get; set; }
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

    // Campos da tabela PRODUTOESERVICOEMPRESA
    public decimal? PrecoVenda { get; set; }
    public short? Vendavel { get; set; }
    public decimal? PrecoCusto { get; set; }
    public decimal? PrecoCustoMedio { get; set; }
    public decimal? LimiteDesconto { get; set; }
    public string? NumeroFci { get; set; }
    public string? ChavePafEmpresa { get; set; }
    public int? ProdutoEmpresaId { get; set; }
    public int? ServicoEmpresaId { get; set; }
    public int? PessoaId { get; set; }
    public int? IndiceCalcularPrecoVendaId { get; set; }
    public int? IndiceCalcularPrecoCustoId { get; set; }
    public int? ProdutoServicoId { get; set; }
    public decimal? PrecoDolar { get; set; }
    public decimal? Percentual { get; set; }
    public decimal? Atacado { get; set; }
    public decimal? Preco3 { get; set; }
    public DateTime? DataAlteracaoPreco { get; set; }
    public short? SituacaoRpl { get; set; }
    public decimal? MargemSeguranca { get; set; }
    public decimal? CustoSeguranca { get; set; }
    public decimal? VarejoPlus { get; set; }
    public decimal? AtacadoPlus { get; set; }

    // Campos da tabela PRODUTOESERVICO
    public string Descricao { get; set; } = string.Empty;
    public string? Caracteristica { get; set; }
    public string? CodigoInterno { get; set; }
    public DateTime? DataInclusao { get; set; }
    public short? Ativo { get; set; }
    public string? Observacao { get; set; }
    public string? Iat { get; set; }
    public string? Ippt { get; set; }
    public string? CodigoContabil { get; set; }
    public decimal? PercentualComissao { get; set; }
    public string? ChavePafP2 { get; set; }
    public string? ChavePafE2 { get; set; }
    public int? SubgrupoId { get; set; }
    public int? GeneroItemId { get; set; }
    public int? TipoItemId { get; set; }
    public int? UnidadeMedidaId { get; set; }
    public int? ProdutoId { get; set; }
    public int? ServicoId { get; set; }
    public int? TributacaoFederalId { get; set; }
    public int? NcmId { get; set; }
    public int? NaturezaReceitaId { get; set; }
    public string? UnMedida { get; set; }
    public int? Grupo { get; set; }
    public char? Grade { get; set; }
    public decimal? CustoMedio { get; set; }
    public decimal? UltimaCompra { get; set; }
    public DateTime? DtCadastro { get; set; }
    public string? Cfop { get; set; }
    public string? Csosn { get; set; }
    public string? CEST { get; set; }
    public decimal? Despesas { get; set; }
    public decimal? TotCusto { get; set; }
    public decimal? Cf { get; set; }
    public decimal? Frete { get; set; }
    public decimal? IcmsSt { get; set; }
    public decimal? Ipi { get; set; }
    public string? RefCor { get; set; }
    public string? Cor { get; set; }
    public string? Tamanho { get; set; }
    public string? Composicao { get; set; }
    public string? Titulo { get; set; }
    public string? Partida { get; set; }
    public string? Tipo { get; set; }
    public string? Categoria { get; set; }
    public string? NTam { get; set; }
    public string? Marca { get; set; }
    public decimal? SacoKg { get; set; }
    public int? CodRastreavel { get; set; }
    public string? TipoProduto { get; set; }
    public string? CstOrigem { get; set; }
    public string? Cst { get; set; }
    public string? Ncm { get; set; }
    public decimal? Icms { get; set; }
    public decimal? Margem { get; set; }
    public int? CodEstacao { get; set; }
    public string? CstPis { get; set; }
    public decimal? AliqPis { get; set; }
    public string? CstCofins { get; set; }
    public decimal? AliqCofins { get; set; }
    public string? CstIpi { get; set; }
    public decimal? AliqIpi { get; set; }
    public string? EnquadraIpi { get; set; }
    public decimal? Fcp { get; set; }
    public decimal? Mva { get; set; }
    public DateTime? DataAlteracaoProduto { get; set; }
    public string? NaturezaReceita { get; set; }
    public string? CfopInter { get; set; }
    public decimal? RedBaseCalculo { get; set; }
    public decimal? FracaoMl { get; set; }
    public int? Monofasico { get; set; }
    public short? SituacaoRplProduto { get; set; }
    public decimal? CaixaM2 { get; set; }
    public string? GrupoFiscal { get; set; }
    public decimal? PesoBobina { get; set; }
    public decimal? Corte { get; set; }
    public int? Defensivo { get; set; }
}
