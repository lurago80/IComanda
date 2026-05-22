using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KdsController : ControllerBase
{
    private readonly IKdsService _kdsService;
    private readonly IKdsRepository _kdsRepository;
    private readonly ILogger<KdsController> _logger;

    public KdsController(IKdsService kdsService, IKdsRepository kdsRepository, ILogger<KdsController> logger)
    {
        _kdsService = kdsService;
        _kdsRepository = kdsRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retorna todos os pedidos ativos para exibição no KDS (Kitchen Display System).
    /// Pedidos com STATUS_COZINHA = ENTREGUE são omitidos.
    /// </summary>
    [HttpGet("pedidos")]
    public async Task<ActionResult<IEnumerable<KdsPedidoDto>>> GetPedidos()
    {
        try
        {
            // Garante que a coluna existe (auto-migration segura)
            await _kdsRepository.EnsureStatusCozinhaColumnAsync();

            var pedidos = await _kdsService.GetPedidosAtivosAsync();
            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KDS: erro ao buscar pedidos ativos");
            return StatusCode(500, new { mensagem = "Erro ao carregar pedidos da cozinha" });
        }
    }

    /// <summary>
    /// Atualiza o status de cozinha de um pedido.
    /// Status válidos: PENDENTE, EM_PREPARO, PRONTO, ENTREGUE
    /// </summary>
    [HttpPut("pedidos/{nota}/status")]
    public async Task<IActionResult> AtualizarStatus(string nota, [FromBody] AtualizarStatusKdsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.StatusCozinha))
                return BadRequest(new { mensagem = "Status inválido" });

            var ok = await _kdsService.AtualizarStatusAsync(nota, request.StatusCozinha);
            if (!ok)
                return BadRequest(new { mensagem = "Status inválido ou pedido não encontrado" });

            _logger.LogInformation("KDS: pedido {Nota} atualizado para {Status}", nota, request.StatusCozinha);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KDS: erro ao atualizar status do pedido {Nota}", nota);
            return StatusCode(500, new { mensagem = "Erro ao atualizar status" });
        }
    }
}
