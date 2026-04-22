using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para Pedidos de Força de Vendas
/// </summary>
[ApiController]
[Route("api/forcavendas/pedidos")]
[Produces("application/json")]
[Authorize]
public class PedidosFVController : ControllerBase
{
    private readonly IPedidoFVService _pedidoService;
    private readonly ILogger<PedidosFVController> _logger;

    public PedidosFVController(IPedidoFVService pedidoService, ILogger<PedidosFVController> logger)
    {
        _pedidoService = pedidoService;
        _logger        = logger;
    }

    /// <summary>Busca pedidos FV com filtros</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PedidoFVDto>), 200)]
    public async Task<ActionResult<IEnumerable<PedidoFVDto>>> BuscarPedidos([FromQuery] BuscarPedidoFVRequest request)
    {
        try
        {
            var pedidos = await _pedidoService.BuscarAsync(request);
            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos FV");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Lista todos os pedidos pendentes de aprovação</summary>
    [HttpGet("pendentes")]
    [Authorize(Policy = "GerenteOrAdmin")]
    [ProducesResponseType(typeof(IEnumerable<PedidoFVDto>), 200)]
    public async Task<ActionResult<IEnumerable<PedidoFVDto>>> GetPendentes()
    {
        try
        {
            var pedidos = await _pedidoService.GetPendentesAsync();
            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar pedidos pendentes");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Lista pedidos de um vendedor específico</summary>
    [HttpGet("vendedor/{idVendedor:int}")]
    [ProducesResponseType(typeof(IEnumerable<PedidoFVDto>), 200)]
    public async Task<ActionResult<IEnumerable<PedidoFVDto>>> GetByVendedor(int idVendedor, [FromQuery] int? status)
    {
        try
        {
            var pedidos = await _pedidoService.GetByVendedorAsync(idVendedor, status);
            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar pedidos do vendedor {Id}", idVendedor);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Busca um pedido FV por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoFVDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PedidoFVDto>> GetPedido(int id)
    {
        try
        {
            var pedido = await _pedidoService.GetByIdAsync(id);
            if (pedido == null) return NotFound(new { mensagem = $"Pedido FV {id} não encontrado" });
            return Ok(pedido);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedido FV {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Cria um novo pedido de força de vendas</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoFVDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<PedidoFVDto>> CriarPedido([FromBody] CriarPedidoFVRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var pedido = await _pedidoService.CriarAsync(request);
            return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, pedido);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao criar pedido FV");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido FV");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza o status de um pedido FV (aprovar, faturar, cancelar).
    /// Regras: Pendente→Aprovado, Pendente/Aprovado→Cancelado, Aprovado→Faturado.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(PedidoFVDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PedidoFVDto>> AtualizarStatus(int id, [FromBody] AtualizarStatusPedidoFVRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var pedido = await _pedidoService.AtualizarStatusAsync(id, request);
            return Ok(pedido);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de negócio ao atualizar status do pedido FV {Id}", id);
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status do pedido FV {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}
