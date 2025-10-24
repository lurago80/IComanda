namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para busca de produtos
/// </summary>
public class BuscarProdutoRequest
{
    /// <summary>
    /// Termo de busca (código de barras, descrição, característica, código interno)
    /// </summary>
    public string? Q { get; set; }

    /// <summary>
    /// Filtrar apenas produtos ativos (padrão: true)
    /// </summary>
    public bool? Ativo { get; set; } = true;

    /// <summary>
    /// Grupo de produto
    /// </summary>
    public int? Grupo { get; set; }

    /// <summary>
    /// Página para paginação
    /// </summary>
    public int Pagina { get; set; } = 1;

    /// <summary>
    /// Itens por página
    /// </summary>
    public int ItensPorPagina { get; set; } = 50;
}
