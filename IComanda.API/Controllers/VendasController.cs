using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de vendas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VendasController : ControllerBase
{
    private readonly IVendaService _vendaService;
    private readonly ILogger<VendasController> _logger;

    public VendasController(IVendaService vendaService, ILogger<VendasController> logger)
    {
        _vendaService = vendaService;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova venda
    /// </summary>
    /// <param name="request">Dados da venda</param>
    /// <returns>Venda criada</returns>
    /// <response code="201">Venda criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(VendaDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<VendaDto>> CriarVenda([FromBody] CriarVendaRequest request)
    {
        try
        {
            _logger.LogInformation("Criando nova venda para cliente: {Cliente}", request.Cliente);

            var venda = await _vendaService.CriarVendaAsync(request);

            _logger.LogInformation("Venda criada com sucesso: {Nota}", venda.Nota);

            return CreatedAtAction(nameof(GetVenda), new { nota = venda.Nota }, venda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar venda");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém uma venda por nota
    /// </summary>
    /// <param name="nota">Número da nota</param>
    /// <returns>Dados da venda</returns>
    /// <response code="200">Venda encontrada</response>
    /// <response code="404">Venda não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{nota}")]
    [ProducesResponseType(typeof(VendaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<VendaDto>> GetVenda(string nota)
    {
        try
        {
            _logger.LogInformation("Buscando venda: {Nota}", nota);

            var venda = await _vendaService.GetVendaAsync(nota);

            if (venda == null)
            {
                _logger.LogWarning("Venda {Nota} não encontrada", nota);
                return NotFound($"Venda {nota} não encontrada");
            }

            _logger.LogInformation("Venda encontrada: {Nota} - Total: {Total}", venda.Nota, venda.Total);
            return Ok(venda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar venda: {Nota}", nota);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista vendas do dia atual
    /// </summary>
    /// <returns>Lista de vendas de hoje</returns>
    /// <response code="200">Vendas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("hoje")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasHoje()
    {
        try
        {
            _logger.LogInformation("Buscando vendas de hoje");

            var vendas = await _vendaService.GetVendasHojeAsync();

            _logger.LogInformation("Encontradas {Count} vendas hoje", vendas.Count());

            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas de hoje");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista vendas por comanda
    /// </summary>
    /// <param name="comanda">Número da comanda</param>
    /// <returns>Lista de vendas da comanda</returns>
    /// <response code="200">Vendas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("comanda/{comanda}")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorComanda(int comanda)
    {
        try
        {
            _logger.LogInformation("Buscando vendas da comanda: {Comanda}", comanda);

            var vendas = await _vendaService.GetVendasPorComandaAsync(comanda);

            _logger.LogInformation("Encontradas {Count} vendas na comanda {Comanda}", vendas.Count(), comanda);

            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas da comanda: {Comanda}", comanda);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista vendas por mesa
    /// </summary>
    /// <param name="mesa">Número da mesa</param>
    /// <returns>Lista de vendas da mesa</returns>
    /// <response code="200">Vendas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("mesa/{mesa}")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorMesa(int mesa)
    {
        try
        {
            _logger.LogInformation("Buscando vendas da mesa: {Mesa}", mesa);

            var vendas = await _vendaService.GetVendasPorMesaAsync(mesa);

            _logger.LogInformation("Encontradas {Count} vendas na mesa {Mesa}", vendas.Count(), mesa);

            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas da mesa: {Mesa}", mesa);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
