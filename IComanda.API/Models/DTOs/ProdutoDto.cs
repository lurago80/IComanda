namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para produto - dados simplificados para o frontend
/// </summary>
public class ProdutoDto
{
    public int Id { get; set; }
    public string CodigoBarra { get; set; } = string.Empty;
    public string CodigoInterno { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Caracteristica { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal Atacado { get; set; }
    public string UnMedida { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public int Grupo { get; set; }
    public bool Pesavel { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
}
