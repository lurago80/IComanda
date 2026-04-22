namespace IComanda.API.Models;

/// <summary>
/// Requisição de paginação - base para todas as queries paginadas
/// </summary>
public class PaginationRequest
{
    /// <summary>Número da página (1-based), padrão = 1</summary>
    public int Page { get; set; } = 1;

    /// <summary>Quantidade de itens por página, padrão = 20, máximo = 100</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Campo para ordenação (ex: "DATA DESC", "NOME ASC")</summary>
    public string? SortBy { get; set; }

    /// <summary>Validar e normalizar valores</summary>
    public void Validate()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 20;
        if (PageSize > 100) PageSize = 100; // Limite máximo
    }

    /// <summary>Obter offset para SQL (SKIP)</summary>
    public int GetOffset() => (Page - 1) * PageSize;

    /// <summary>Obter limit para SQL</summary>
    public int GetLimit() => PageSize;
}

/// <summary>
/// Resposta de paginação - para retornar dados paginados
/// </summary>
public class PaginationResponse<T>
{
    /// <summary>Dados da página atual</summary>
    public List<T> Data { get; set; } = new();

    /// <summary>Número da página atual</summary>
    public int Page { get; set; }

    /// <summary>Quantidade de itens por página</summary>
    public int PageSize { get; set; }

    /// <summary>Total de registros</summary>
    public int Total { get; set; }

    /// <summary>Total de páginas</summary>
    public int TotalPages => (Total + PageSize - 1) / PageSize;

    /// <summary>Se tem próxima página</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Se tem página anterior</summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>Índice do primeiro item (1-based)</summary>
    public int FirstItemIndex => Total == 0 ? 0 : (Page - 1) * PageSize + 1;

    /// <summary>Índice do último item (1-based)</summary>
    public int LastItemIndex => Math.Min(Page * PageSize, Total);

    public PaginationResponse() { }

    public PaginationResponse(List<T> data, int page, int pageSize, int total)
    {
        Data = data;
        Page = page;
        PageSize = pageSize;
        Total = total;
    }
}

/// <summary>
/// Extensões para aplicar paginação em IQueryable
/// </summary>
public static class PaginationExtensions
{
    /// <summary>Aplicar paginação em IQueryable</summary>
    public static IQueryable<T> ApplyPagination<T>(
        this IQueryable<T> query,
        PaginationRequest pagination) where T : class
    {
        pagination.Validate();
        return query.Skip(pagination.GetOffset()).Take(pagination.GetLimit());
    }

    /// <summary>Aplicar paginação em IEnumerable</summary>
    public static IEnumerable<T> ApplyPagination<T>(
        this IEnumerable<T> query,
        PaginationRequest pagination) where T : class
    {
        pagination.Validate();
        return query.Skip(pagination.GetOffset()).Take(pagination.GetLimit());
    }

    /// <summary>Criar resposta paginada</summary>
    public static PaginationResponse<T> CreatePaginationResponse<T>(
        this List<T> data,
        int page,
        int pageSize,
        int total) where T : class
    {
        return new PaginationResponse<T>(data, page, pageSize, total);
    }
}
