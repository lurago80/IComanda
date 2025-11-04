using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;

namespace IComanda.API.Repositories.Interfaces;

public interface IClienteRepository
{
    /// <summary>
    /// Busca clientes com filtros
    /// </summary>
    /// <param name="request">Parâmetros de busca</param>
    /// <returns>Lista de clientes</returns>
    Task<IEnumerable<Cliente>> BuscarClientesAsync(BuscarClienteRequest request);

    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <returns>Cliente encontrado ou null</returns>
    Task<Cliente?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém um cliente por CPF/CNPJ
    /// </summary>
    /// <param name="cpfCnpj">CPF ou CNPJ do cliente</param>
    /// <returns>Cliente encontrado ou null</returns>
    Task<Cliente?> GetByCpfCnpjAsync(string cpfCnpj);

    /// <summary>
    /// Obtém clientes por vendedor
    /// </summary>
    /// <param name="idVendedor">ID do vendedor</param>
    /// <returns>Lista de clientes do vendedor</returns>
    Task<IEnumerable<Cliente>> GetByVendedorAsync(int idVendedor);

    /// <summary>
    /// Conta o total de clientes que atendem aos critérios de busca
    /// </summary>
    /// <param name="request">Parâmetros de busca</param>
    /// <returns>Total de clientes</returns>
    Task<int> ContarClientesAsync(BuscarClienteRequest request);
}
