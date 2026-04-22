using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de caixas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CaixasController : ControllerBase
{
    private readonly ICaixaService _caixaService;
    private readonly ILogger<CaixasController> _logger;

    public CaixasController(
        ICaixaService caixaService,
        ILogger<CaixasController> logger)
    {
        _caixaService = caixaService;
        _logger = logger;
    }

    /// <summary>
    /// Abre um caixa
    /// </summary>
    [HttpPost("abrir")]
    [ProducesResponseType(typeof(CaixaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CaixaDto>> AbrirCaixa([FromBody] AbrirCaixaRequest request)
    {
        try
        {
            _logger.LogInformation("💰 Abrindo caixa {Numero}", request.Numero);
            var caixa = await _caixaService.AbrirCaixaAsync(request);
            return Ok(caixa);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao abrir caixa");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao abrir caixa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Fecha um caixa
    /// </summary>
    [HttpPost("fechar")]
    [ProducesResponseType(typeof(CaixaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CaixaDto>> FecharCaixa([FromBody] FecharCaixaRequest request)
    {
        try
        {
            _logger.LogInformation("💰 Fechando caixa {Id}", request.Id);
            var caixa = await _caixaService.FecharCaixaAsync(request);
            return Ok(caixa);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao fechar caixa");
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fechar caixa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Busca caixa aberto por número
    /// </summary>
    [HttpGet("aberto/{numero}")]
    [ProducesResponseType(typeof(CaixaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CaixaDto>> GetCaixaAberto(int numero)
    {
        try
        {
            var caixa = await _caixaService.GetCaixaAbertoAsync(numero);
            if (caixa == null)
            {
                return NotFound(new { mensagem = $"Caixa {numero} não encontrado ou não está aberto" });
            }
            return Ok(caixa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar caixa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista caixas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CaixaDto>), 200)]
    public async Task<ActionResult<IEnumerable<CaixaDto>>> GetCaixas(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var caixas = await _caixaService.GetCaixasAsync(dataInicio, dataFim);
            return Ok(caixas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar caixas");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém relatório completo de um caixa
    /// </summary>
    [HttpGet("{id}/relatorio")]
    [ProducesResponseType(typeof(CaixaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CaixaDto>> GetRelatorioCaixa(
        int id,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var caixa = await _caixaService.GetRelatorioCaixaAsync(id, dataInicio, dataFim);
            return Ok(caixa);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de caixa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}

