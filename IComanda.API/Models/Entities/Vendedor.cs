namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade Vendedor — tabela VENDEDOR do banco de dados
/// </summary>
public class Vendedor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Celular { get; set; }
    /// <summary>ATIVO no banco é CHAR(1): 'S' = ativo, 'N' = inativo</summary>
    public string Ativo { get; set; } = "S";
    /// <summary>Percentual de comissão padrão do vendedor</summary>
    public decimal ComissaoPerc { get; set; }
    /// <summary>Meta mensal de vendas em R$</summary>
    public decimal MetaMensal { get; set; }
    /// <summary>Região/território de atuação</summary>
    public string? Regiao { get; set; }
    public string? Obs { get; set; }
    public DateTime? DataCadastro { get; set; }

    // Propriedades calculadas
    public bool IsAtivo => Ativo == "S";
}
