using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de clientes
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(IClienteService clienteService, ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Busca clientes com filtros
    /// </summary>
    /// <param name="request">Parâmetros de busca</param>
    /// <returns>Lista de clientes</returns>
    /// <response code="200">Clientes encontrados</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IEnumerable<ClienteDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> BuscarClientes([FromQuery] BuscarClienteRequest request)
    {
        try
        {
            _logger.LogInformation("Buscando clientes com filtros: {Filtros}", request);

            var clientes = await _clienteService.BuscarClientesAsync(request);

            _logger.LogInformation("Encontrados {Count} clientes", clientes.Count());

            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um cliente por ID
    /// </summary>
    /// <param name="id">ID do cliente</param>
    /// <returns>Dados do cliente</returns>
    /// <response code="200">Cliente encontrado</response>
    /// <response code="404">Cliente não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ClienteDto>> GetCliente(int id)
    {
        try
        {
            _logger.LogInformation("Buscando cliente: {Id}", id);

            var cliente = await _clienteService.GetByIdAsync(id);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente {Id} não encontrado", id);
                return NotFound($"Cliente {id} não encontrado");
            }

            _logger.LogInformation("Cliente encontrado: {Id} - {Nome}", cliente.Id, cliente.Nome);
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um cliente por CPF/CNPJ
    /// </summary>
    /// <param name="cpfCnpj">CPF ou CNPJ do cliente</param>
    /// <returns>Dados do cliente</returns>
    /// <response code="200">Cliente encontrado</response>
    /// <response code="404">Cliente não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("cpf-cnpj/{cpfCnpj}")]
    [ProducesResponseType(typeof(ClienteDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ClienteDto>> GetClientePorCpfCnpj(string cpfCnpj)
    {
        try
        {
            _logger.LogInformation("Buscando cliente por CPF/CNPJ: {CpfCnpj}", cpfCnpj);

            var cliente = await _clienteService.GetByCpfCnpjAsync(cpfCnpj);

            if (cliente == null)
            {
                _logger.LogWarning("Cliente com CPF/CNPJ {CpfCnpj} não encontrado", cpfCnpj);
                return NotFound($"Cliente com CPF/CNPJ {cpfCnpj} não encontrado");
            }

            _logger.LogInformation("Cliente encontrado: {Id} - {Nome}", cliente.Id, cliente.Nome);
            return Ok(cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por CPF/CNPJ: {CpfCnpj}", cpfCnpj);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista clientes por vendedor
    /// </summary>
    /// <param name="idVendedor">ID do vendedor</param>
    /// <returns>Lista de clientes do vendedor</returns>
    /// <response code="200">Clientes encontrados</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("vendedor/{idVendedor}")]
    [ProducesResponseType(typeof(IEnumerable<ClienteDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientesPorVendedor(int idVendedor)
    {
        try
        {
            _logger.LogInformation("Buscando clientes do vendedor: {IdVendedor}", idVendedor);

            var clientes = await _clienteService.GetByVendedorAsync(idVendedor);

            _logger.LogInformation("Encontrados {Count} clientes para o vendedor {IdVendedor}", clientes.Count(), idVendedor);

            return Ok(clientes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes do vendedor: {IdVendedor}", idVendedor);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Conta o total de clientes que atendem aos critérios de busca
    /// </summary>
    /// <param name="request">Parâmetros de busca</param>
    /// <returns>Total de clientes</returns>
    /// <response code="200">Total calculado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("contar")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<int>> ContarClientes([FromQuery] BuscarClienteRequest request)
    {
        try
        {
            _logger.LogInformation("Contando clientes com filtros: {Filtros}", request);

            var total = await _clienteService.ContarClientesAsync(request);

            _logger.LogInformation("Total de clientes: {Total}", total);

            return Ok(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar clientes");
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
