namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO de Vendedor para a API
/// </summary>
public class VendedorDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Celular { get; set; }
    public bool Ativo { get; set; }
    public decimal ComissaoPerc { get; set; }
    public decimal MetaMensal { get; set; }
    public string? Regiao { get; set; }
    public string? Obs { get; set; }
    public DateTime? DataCadastro { get; set; }
    /// <summary>Total de vendas do mês atual (calculado)</summary>
    public decimal VendasMesAtual { get; set; }
    /// <summary>Percentual de atingimento da meta (calculado)</summary>
    public decimal PercentualMeta { get; set; }
}
