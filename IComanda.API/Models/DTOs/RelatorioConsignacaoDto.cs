namespace IComanda.API.Models.DTOs;

public class RelatorioConsignacaoDto
{
    public int GrupoId { get; set; }
    public string GrupoDescricao { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<ItemConsignacaoDto> Itens { get; set; } = new();
    public decimal TotalValor { get; set; }
    public decimal TotalPercentual { get; set; }
}

public class ItemConsignacaoDto
{
    public int ProdutoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal QuantidadeVendida { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorPercentual { get; set; }
}
