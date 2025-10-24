namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de produto baseado nas 4 tabelas do sistema legado:
/// PRODUTO + PRODUTOEMPRESA + PRODUTOESERVICO + PRODUTOESERVICOEMPRESA
/// </summary>
public class Produto
{
    public int Id { get; set; }
    public string CodigoBarra { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Caracteristica { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal QuantidadeMinima { get; set; }
    public string Localizacao { get; set; } = string.Empty;
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal Atacado { get; set; }
    public decimal Preco3 { get; set; }
    public string UnMedida { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public int Grupo { get; set; }
    public bool Pesavel { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public DateTime DataInclusao { get; set; }
    public DateTime? DataUltimaVenda { get; set; }
    public DateTime? DataUltimaCompra { get; set; }
}
