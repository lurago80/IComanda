using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IClienteService
{
    /// <summary>
    /// Busca clientes com filtros
    /// </summary>
    /// <param name="request">Parâmetros de busca</param>
    /// <returns>Lista de clientes</returns>
    Task<IEnumerable<ClienteDto>> BuscarClientesAsync(BuscarClienteRequest request);

    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <returns>Cliente encontrado ou null</returns>
    Task<ClienteDto?> GetByIdAsync(int id);

    /// <summary>
    /// Obtém um cliente por CPF/CNPJ
    /// </summary>
    /// <param name="cpfCnpj">CPF ou CNPJ do cliente</param>
    /// <returns>Cliente encontrado ou null</returns>
    Task<ClienteDto?> GetByCpfCnpjAsync(string cpfCnpj);

    /// <summary>
    /// Obtém clientes por vendedor
    /// </summary>
    /// <param name="idVendedor">ID do vendedor</param>
    /// <returns>Lista de clientes do vendedor</returns>
    Task<IEnumerable<ClienteDto>> GetByVendedorAsync(int idVendedor);

    /// <summary>
    /// Conta o total de clientes que atendem aos critérios de busca
    /// </summary>
    /// <param name="request">Parâmetros de busca</param>
    /// <returns>Total de clientes</returns>
    Task<int> ContarClientesAsync(BuscarClienteRequest request);

    /// <summary>
    /// Verifica se cliente existe por CPF/CNPJ ou Telefone
    /// </summary>
    /// <param name="cpfCnpjOuTelefone">CPF/CNPJ ou Telefone</param>
    /// <returns>Response com status e dados do cliente</returns>
    Task<VerificarClienteResponse> VerificarClienteAsync(string cpfCnpjOuTelefone);

    /// <summary>
    /// Cadastro rápido de cliente na abertura de comanda
    /// </summary>
    /// <param name="request">Dados do cliente</param>
    /// <returns>Cliente cadastrado</returns>
    Task<ClienteDto> CadastroRapidoAsync(CadastroRapidoClienteRequest request);

    /// <summary>
    /// Cria um novo cliente completo
    /// </summary>
    /// <param name="request">Dados do cliente</param>
    /// <returns>Cliente criado</returns>
    Task<ClienteDto> CriarClienteAsync(CriarClienteRequest request);

    /// <summary>
    /// Atualiza um cliente existente
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <param name="request">Dados do cliente</param>
    /// <returns>Cliente atualizado</returns>
    Task<ClienteDto> AtualizarClienteAsync(int id, CriarClienteRequest request);

    /// <summary>
    /// Exclui um cliente pelo ID
    /// </summary>
    /// <param name="id">ID do cliente</param>
    Task ExcluirClienteAsync(int id);
}
