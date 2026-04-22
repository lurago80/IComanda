using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para Rotas e Visitas de Força de Vendas
/// </summary>
[ApiController]
[Route("api/forcavendas/rotas")]
[Produces("application/json")]
[Authorize]
public class RotasFVController : ControllerBase
{
    private readonly IRotaFVService _rotaService;
    private readonly ILogger<RotasFVController> _logger;

    public RotasFVController(IRotaFVService rotaService, ILogger<RotasFVController> logger)
    {
        _rotaService = rotaService;
        _logger      = logger;
    }

    /// <summary>Lista visitas de um vendedor com filtro de período</summary>
    [HttpGet("vendedor/{idVendedor:int}")]
    [ProducesResponseType(typeof(IEnumerable<VisitaFVDto>), 200)]
    public async Task<ActionResult<IEnumerable<VisitaFVDto>>> GetVisitas(
        int idVendedor,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        try
        {
            var visitas = await _rotaService.GetVisitasVendedorAsync(idVendedor, dataInicio, dataFim);
            return Ok(visitas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar visitas do vendedor {Id}", idVendedor);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Lista a agenda de visitas de hoje para um vendedor</summary>
    [HttpGet("vendedor/{idVendedor:int}/hoje")]
    [ProducesResponseType(typeof(IEnumerable<VisitaFVDto>), 200)]
    public async Task<ActionResult<IEnumerable<VisitaFVDto>>> GetAgendaHoje(int idVendedor)
    {
        try
        {
            var visitas = await _rotaService.GetAgendaHojeAsync(idVendedor);
            return Ok(visitas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar agenda de hoje do vendedor {Id}", idVendedor);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Busca uma visita por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VisitaFVDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VisitaFVDto>> GetVisita(int id)
    {
        try
        {
            var visita = await _rotaService.GetByIdAsync(id);
            if (visita == null) return NotFound(new { mensagem = $"Visita FV {id} não encontrada" });
            return Ok(visita);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar visita FV {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Agenda uma nova visita</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VisitaFVDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<VisitaFVDto>> AgendarVisita([FromBody] AgendarVisitaFVRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var visita = await _rotaService.AgendarVisitaAsync(request);
            return CreatedAtAction(nameof(GetVisita), new { id = visita.Id }, visita);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao agendar visita FV");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Realiza check-in na visita (vendedor chegou ao cliente)</summary>
    [HttpPost("{id:int}/checkin")]
    [ProducesResponseType(typeof(VisitaFVDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VisitaFVDto>> Checkin(int id, [FromBody] CheckinVisitaFVRequest request)
    {
        try
        {
            var visita = await _rotaService.CheckinAsync(id, request);
            return Ok(visita);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar check-in da visita {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Conclui uma visita (checkout) com resultado</summary>
    [HttpPost("{id:int}/concluir")]
    [ProducesResponseType(typeof(VisitaFVDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VisitaFVDto>> ConcluirVisita(int id, [FromBody] ConcluirVisitaFVRequest request)
    {
        try
        {
            var visita = await _rotaService.ConcluirAsync(id, request);
            return Ok(visita);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao concluir visita {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Marca uma visita como não realizada</summary>
    [HttpPost("{id:int}/nao-realizada")]
    [ProducesResponseType(typeof(VisitaFVDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VisitaFVDto>> MarcarNaoRealizada(int id, [FromQuery] string? obs)
    {
        try
        {
            var visita = await _rotaService.MarcarNaoRealizadaAsync(id, obs);
            return Ok(visita);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar visita {Id} como não realizada", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}
