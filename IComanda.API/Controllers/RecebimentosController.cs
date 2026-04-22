using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de recebimentos e fechamento de comandas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class RecebimentosController : ControllerBase
{
    private readonly IRecebimentoService _recebimentoService;
    private readonly IReceberService _receberService;
    private readonly ILogger<RecebimentosController> _logger;

    public RecebimentosController(
        IRecebimentoService recebimentoService,
        IReceberService receberService,
        ILogger<RecebimentosController> logger)
    {
        _recebimentoService = recebimentoService;
        _receberService = receberService;
        _logger = logger;
    }

    /// <summary>
    /// Fecha uma comanda aberta processando os recebimentos
    /// </summary>
    /// <param name="request">Dados do fechamento da comanda</param>
    /// <returns>Resultado do fechamento</returns>
    /// <response code="200">Comanda fechada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Comanda não encontrada ou não possui venda aberta</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("fechar-comanda")]
    [ProducesResponseType(typeof(FecharComandaResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<FecharComandaResponseDto>> FecharComanda([FromBody] FecharComandaRequest request)
    {
        try
        {
            _logger.LogInformation("🔄 [RecebimentosController] Endpoint /api/recebimentos/fechar-comanda chamado");
            _logger.LogInformation("🔄 [RecebimentosController] Fechando comanda {Comanda}", request?.Comanda ?? 0);
            
            if (request == null)
            {
                _logger.LogError("❌ [RecebimentosController] Request é null");
                return BadRequest(new { mensagem = "Request não pode ser nulo" });
            }

            if (string.IsNullOrWhiteSpace(request.Nota) && request.Comanda <= 0)
            {
                return BadRequest(new { mensagem = "Informe o número da comanda ou a nota (Caixa Rápido)." });
            }

            if (request.Recebimentos == null || request.Recebimentos.Count == 0)
            {
                return BadRequest(new { mensagem = "É necessário informar pelo menos uma forma de pagamento" });
            }

            var resultado = await _recebimentoService.FecharComandaAsync(request);

            _logger.LogInformation("✅ Comanda {Comanda} fechada com sucesso - Nota: {Nota}", 
                request.Comanda, resultado.Nota);

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "⚠️ Erro ao fechar comanda {Comanda}", request.Comanda);
            return NotFound(new { mensagem = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Dados inválidos para fechar comanda {Comanda}", request.Comanda);
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao fechar comanda {Comanda}", request.Comanda);
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Lista todas as formas de pagamento ativas
    /// </summary>
    /// <returns>Lista de formas de pagamento</returns>
    /// <response code="200">Formas de pagamento encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("formas-pagamento")]
    [ProducesResponseType(typeof(IEnumerable<FormaPagamentoDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<FormaPagamentoDto>>> GetFormasPagamento()
    {
        try
        {
            _logger.LogInformation("🔍 Buscando formas de pagamento ativas");

            var formas = await _recebimentoService.GetFormasPagamentoAtivasAsync();

            _logger.LogInformation("✅ Encontradas {Count} formas de pagamento", formas.Count());

            return Ok(formas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar formas de pagamento");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca recebimentos por nota
    /// </summary>
    /// <param name="nota">Número da nota</param>
    /// <returns>Lista de recebimentos</returns>
    /// <response code="200">Recebimentos encontrados</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("nota/{nota}")]
    [ProducesResponseType(typeof(IEnumerable<RecebimentoVendasDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<RecebimentoVendasDto>>> GetRecebimentosPorNota(string nota)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando recebimentos da nota {Nota}", nota);

            var recebimentos = await _recebimentoService.GetRecebimentosPorNotaAsync(nota);

            _logger.LogInformation("✅ Encontrados {Count} recebimentos para a nota {Nota}", 
                recebimentos.Count(), nota);

            return Ok(recebimentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar recebimentos da nota {Nota}", nota);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca recebimentos por período
    /// </summary>
    /// <param name="dataInicio">Data inicial (opcional)</param>
    /// <param name="dataFim">Data final (opcional)</param>
    /// <returns>Lista de recebimentos</returns>
    /// <response code="200">Recebimentos encontrados</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("periodo")]
    [ProducesResponseType(typeof(IEnumerable<RecebimentoVendasDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<RecebimentoVendasDto>>> GetRecebimentosPorPeriodo(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando recebimentos por período - Data Inicio: {DataInicio}, Data Fim: {DataFim}",
                dataInicio, dataFim);

            var recebimentos = await _recebimentoService.GetRecebimentosPorPeriodoAsync(dataInicio, dataFim);

            _logger.LogInformation("✅ Encontrados {Count} recebimentos no período", recebimentos.Count());

            return Ok(recebimentos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar recebimentos por período");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}

