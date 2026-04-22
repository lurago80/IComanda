namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para caixa
/// </summary>
public class CaixaDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }
    public int OperadorAbertura { get; set; }
    public string? NomeOperadorAbertura { get; set; }
    public int? OperadorFechamento { get; set; }
    public string? NomeOperadorFechamento { get; set; }
    public decimal ValorAbertura { get; set; }
    public decimal? ValorFechamento { get; set; }
    public string Status { get; set; } = "ABERTO";
    public string? Observacoes { get; set; }
    
    // Totais calculados
    public decimal TotalVendas { get; set; }
    public decimal TotalRecebimentos { get; set; }
    public decimal TotalDinheiro { get; set; }
    public decimal TotalCartao { get; set; }
    public decimal TotalPix { get; set; }
    public decimal TotalCheque { get; set; }
    public decimal TotalBoleto { get; set; }
    public decimal SaldoEsperado { get; set; }
    public decimal Diferenca { get; set; }

    /// <summary>Total de saídas (sangria, despesa) no período.</summary>
    public decimal TotalSaidas { get; set; }

    /// <summary>Lista de movimentos do caixa (abertura, sangria, suprimento, despesa, recebimentos).</summary>
    public List<CaixaMovimentoDto> Movimentos { get; set; } = new();
}

