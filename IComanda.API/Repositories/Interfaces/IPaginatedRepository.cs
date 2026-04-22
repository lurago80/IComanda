using IComanda.API.Models;

namespace IComanda.API.Repositories.Interfaces;

/// <summary>
/// Extensão com suporte a paginação para repositories genéricos
/// </summary>
public interface IRepositoryPagination<T> where T : class
{
    /// <summary>Obter todos os registros com paginação</summary>
    Task<PaginationResponse<T>> GetAllPaginatedAsync(PaginationRequest pagination);

    /// <summary>Buscar registros com filtro e paginação</summary>
    Task<PaginationResponse<T>> SearchAsync(string searchTerm, PaginationRequest pagination);

    /// <summary>Contar total de registros</summary>
    Task<int> CountAsync();
}

/// <summary>
/// Interface específica para Produtos com paginação
/// </summary>
public interface IProdutoRepositoryPaginated : IRepositoryPagination<Models.Entities.Produto>
{
    /// <summary>Obter produtos por grupo com paginação</summary>
    Task<PaginationResponse<Models.Entities.Produto>> GetByGroupPaginatedAsync(int groupId, PaginationRequest pagination);

    /// <summary>Obter produtos ativos com paginação</summary>
    Task<PaginationResponse<Models.Entities.Produto>> GetAtivePaginatedAsync(PaginationRequest pagination);
}

/// <summary>
/// Interface específica para Clientes com paginação
/// </summary>
public interface IClienteRepositoryPaginated : IRepositoryPagination<Models.Entities.Cliente>
{
    /// <summary>Obter clientes por nome com paginação</summary>
    Task<PaginationResponse<Models.Entities.Cliente>> GetByNamePaginatedAsync(string name, PaginationRequest pagination);

    /// <summary>Obter clientes ativos com paginação</summary>
    Task<PaginationResponse<Models.Entities.Cliente>> GetAtivePaginatedAsync(PaginationRequest pagination);
}

/// <summary>
/// Interface específica para Vendas com paginação
/// </summary>
public interface IVendaRepositoryPaginated : IRepositoryPagination<Models.Entities.Venda>
{
    /// <summary>Obter vendas abertas com paginação</summary>
    Task<PaginationResponse<Models.Entities.Venda>> GetAbertosPaginatedAsync(PaginationRequest pagination);

    /// <summary>Obter vendas por período com paginação</summary>
    Task<PaginationResponse<Models.Entities.Venda>> GetByPeriodPaginatedAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        PaginationRequest pagination);

    /// <summary>Obter vendas por cliente com paginação</summary>
    Task<PaginationResponse<Models.Entities.Venda>> GetByClientePaginatedAsync(
        int clienteId, 
        PaginationRequest pagination);
}
