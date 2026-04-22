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

    /// <summary>
    /// Verifica se já existe cliente com o CPF/CNPJ informado
    /// </summary>
    /// <param name="cpfCnpj">CPF ou CNPJ</param>
    /// <returns>True se existe, False se não</returns>
    Task<bool> ExistePorCpfCnpjAsync(string cpfCnpj);

    /// <summary>
    /// Verifica se já existe cliente com o telefone informado
    /// </summary>
    /// <param name="telefone">Telefone</param>
    /// <returns>True se existe, False se não</returns>
    Task<bool> ExistePorTelefoneAsync(string telefone);

    /// <summary>
    /// Obtém um cliente por telefone
    /// </summary>
    /// <param name="telefone">Telefone do cliente</param>
    /// <returns>Cliente encontrado ou null</returns>
    Task<Cliente?> GetByTelefoneAsync(string telefone);

    /// <summary>
    /// Cria um novo cliente (cadastro rápido)
    /// </summary>
    /// <param name="cliente">Dados do cliente</param>
    /// <returns>True se cadastrado com sucesso</returns>
    Task<bool> CriarClienteAsync(Cliente cliente);

    /// <summary>
    /// Cria um novo cliente completo
    /// </summary>
    /// <param name="cliente">Dados do cliente</param>
    /// <returns>ID do cliente criado</returns>
    Task<int> CriarClienteCompletoAsync(Cliente cliente);

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    /// <param name="cliente">Dados do cliente</param>
    /// <returns>True se atualizado com sucesso</returns>
    Task<bool> AtualizarClienteAsync(Cliente cliente);

    /// <summary>
    /// Exclui um cliente pelo ID
    /// </summary>
    /// <param name="id">ID do cliente a excluir</param>
    /// <returns>True se excluído com sucesso</returns>
    Task<bool> ExcluirClienteAsync(int id);
}
