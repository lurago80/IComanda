namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade ProdutoServicoEmpresa baseada na tabela PRODUTOESERVICOEMPRESA
/// </summary>
public class ProdutoServicoEmpresa
{
    public int Id { get; set; }
    public decimal? PrecoVenda { get; set; }
    public short? Vendavel { get; set; }
    public decimal? PrecoCusto { get; set; }
    public decimal? PrecoCustoMedio { get; set; }
    public decimal? LimiteDesconto { get; set; }
    public string? NumeroFci { get; set; }
    public string? ChavePaf { get; set; }
    public int? ProdutoEmpresaId { get; set; }
    public int? ServicoEmpresaId { get; set; }
    public int PessoaId { get; set; }
    public int? IndiceCalcularPrecoVendaId { get; set; }
    public int? IndiceCalcularPrecoCustoId { get; set; }
    public int? ProdutoServicoId { get; set; }
    public decimal? PrecoDolar { get; set; }
    public decimal? Percentual { get; set; }
    public decimal? Atacado { get; set; }
    public decimal? Preco3 { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public short? SituacaoRpl { get; set; }
    public decimal? MargemSeguranca { get; set; }
    public decimal? CustoSeguranca { get; set; }
    public decimal? VarejoPlus { get; set; }
    public decimal? AtacadoPlus { get; set; }
}
