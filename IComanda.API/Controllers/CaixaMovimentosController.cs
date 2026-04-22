using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de movimentos de caixa
/// </summary>
[ApiController]
[Route("api/caixas/movimentos")]
[Produces("application/json")]
[Authorize]
public class CaixaMovimentosController : ControllerBase
{
    private readonly ICaixaMovimentoService _caixaMovimentoService;
    private readonly ILogger<CaixaMovimentosController> _logger;

    public CaixaMovimentosController(
        ICaixaMovimentoService caixaMovimentoService,
        ILogger<CaixaMovimentosController> logger)
    {
        _caixaMovimentoService = caixaMovimentoService;
        _logger = logger;
    }

    /// <summary>
    /// Registra abertura de caixa (entrada)
    /// </summary>
    [HttpPost("abertura")]
    [ProducesResponseType(typeof(CaixaMovimentoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CaixaMovimentoDto>> AbrirCaixa([FromBody] CaixaMovimentoRequest request)
    {
        try
        {
            _logger.LogInformation("💰 Registrando abertura de caixa - Terminal: {Terminal}", request.Terminal);
            var movimento = await _caixaMovimentoService.AbrirCaixaAsync(request);
            return Ok(movimento);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para abertura de caixa");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao abrir caixa");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar abertura de caixa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Registra suprimento (entrada)
    /// </summary>
    [HttpPost("suprimento")]
    [ProducesResponseType(typeof(CaixaMovimentoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CaixaMovimentoDto>> RegistrarSuprimento([FromBody] CaixaMovimentoRequest request)
    {
        try
        {
            _logger.LogInformation("💰 Registrando suprimento - Terminal: {Terminal}", request.Terminal);
            var movimento = await _caixaMovimentoService.RegistrarSuprimentoAsync(request);
            return Ok(movimento);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para suprimento");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar suprimento");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Registra sangria (saída)
    /// </summary>
    [HttpPost("sangria")]
    [ProducesResponseType(typeof(CaixaMovimentoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CaixaMovimentoDto>> RegistrarSangria([FromBody] CaixaMovimentoRequest request)
    {
        try
        {
            _logger.LogInformation("💰 Registrando sangria - Terminal: {Terminal}", request.Terminal);
            var movimento = await _caixaMovimentoService.RegistrarSangriaAsync(request);
            return Ok(movimento);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para sangria");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao registrar sangria");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar sangria");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Registra pagamento de despesas (saída)
    /// </summary>
    [HttpPost("despesa")]
    [ProducesResponseType(typeof(CaixaMovimentoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CaixaMovimentoDto>> RegistrarPagamentoDespesa([FromBody] CaixaMovimentoRequest request)
    {
        try
        {
            _logger.LogInformation("💰 Registrando pagamento de despesa - Terminal: {Terminal}", request.Terminal);
            var movimento = await _caixaMovimentoService.RegistrarPagamentoDespesaAsync(request);
            return Ok(movimento);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para pagamento de despesa");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao registrar pagamento de despesa");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar pagamento de despesa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Busca resumo do caixa
    /// </summary>
    [HttpGet("resumo/{terminal}")]
    [ProducesResponseType(typeof(CaixaResumoDto), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<CaixaResumoDto>> GetResumo(
        int terminal,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando resumo de caixa - Terminal: {Terminal}", terminal);
            var resumo = await _caixaMovimentoService.GetResumoAsync(terminal, dataInicio, dataFim);
            return Ok(resumo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar resumo de caixa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Busca movimentos do caixa
    /// </summary>
    [HttpGet("{terminal}")]
    [ProducesResponseType(typeof(IEnumerable<CaixaMovimentoDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<CaixaMovimentoDto>>> GetMovimentos(
        int terminal,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando movimentos de caixa - Terminal: {Terminal}", terminal);
            var movimentos = await _caixaMovimentoService.GetMovimentosAsync(terminal, dataInicio, dataFim);
            return Ok(movimentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar movimentos de caixa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }
}
