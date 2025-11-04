namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para busca de clientes
/// </summary>
public class BuscarClienteRequest
{
    /// <summary>
    /// Termo de busca (nome, CPF/CNPJ, telefone)
    /// </summary>
    public string? Q { get; set; }

    /// <summary>
    /// Filtrar apenas clientes ativos
    /// </summary>
    public bool? Ativo { get; set; }

    /// <summary>
    /// Filtrar apenas clientes não bloqueados
    /// </summary>
    public bool? NaoBloqueado { get; set; }

    /// <summary>
    /// Página para paginação
    /// </summary>
    public int? Pagina { get; set; }

    /// <summary>
    /// Itens por página
    /// </summary>
    public int? ItensPorPagina { get; set; }

    /// <summary>
    /// ID do vendedor para filtrar clientes
    /// </summary>
    public int? IdVendedor { get; set; }

    /// <summary>
    /// Classificação do cliente (A, B, C, etc.)
    /// </summary>
    public string? Classificacao { get; set; }
}
