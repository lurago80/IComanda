namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para relatório de produtos mais vendidos
/// </summary>
public class RelatorioProdutosMaisVendidosDto
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public List<ProdutoVendidoDto> Produtos { get; set; } = new();
}

/// <summary>
/// DTO para produto vendido
/// </summary>
public class ProdutoVendidoDto
{
    public int Codigo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int QuantidadeVendida { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal ValorMedio { get; set; }
    public int NumeroVendas { get; set; }
    public int Posicao { get; set; }
}

