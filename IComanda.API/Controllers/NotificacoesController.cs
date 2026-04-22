using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para notificações e alertas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class NotificacoesController : ControllerBase
{
    private readonly INotificacaoService _notificacaoService;
    private readonly ILogger<NotificacoesController> _logger;

    public NotificacoesController(
        INotificacaoService notificacaoService,
        ILogger<NotificacoesController> logger)
    {
        _notificacaoService = notificacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Lista notificações
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<NotificacaoDto>), 200)]
    public async Task<ActionResult<IEnumerable<NotificacaoDto>>> GetNotificacoes(
        [FromQuery] bool? apenasNaoLidas = null,
        [FromQuery] string? categoria = null)
    {
        try
        {
            var notificacoes = await _notificacaoService.GetNotificacoesAsync(apenasNaoLidas, categoria);
            return Ok(notificacoes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar notificações");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém quantidade de notificações não lidas
    /// </summary>
    [HttpGet("nao-lidas/quantidade")]
    [ProducesResponseType(typeof(int), 200)]
    public async Task<ActionResult<int>> GetQuantidadeNaoLidas()
    {
        try
        {
            var quantidade = await _notificacaoService.GetQuantidadeNaoLidasAsync();
            return Ok(new { quantidade });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar quantidade de notificações não lidas");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Marca notificação como lida
    /// </summary>
    [HttpPost("{id}/marcar-lida")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult<bool>> MarcarComoLida(int id)
    {
        try
        {
            var sucesso = await _notificacaoService.MarcarComoLidaAsync(id);
            return Ok(sucesso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar notificação como lida");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Marca todas as notificações como lidas
    /// </summary>
    [HttpPost("marcar-todas-lidas")]
    [ProducesResponseType(typeof(bool), 200)]
    public async Task<ActionResult<bool>> MarcarTodasComoLidas()
    {
        try
        {
            var sucesso = await _notificacaoService.MarcarTodasComoLidasAsync();
            return Ok(sucesso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar todas as notificações como lidas");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica e cria alertas automáticos
    /// </summary>
    [HttpPost("verificar-alertas")]
    [ProducesResponseType(200)]
    public async Task<ActionResult> VerificarAlertas()
    {
        try
        {
            await _notificacaoService.VerificarAlertasAsync();
            return Ok(new { mensagem = "Alertas verificados" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar alertas");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}

