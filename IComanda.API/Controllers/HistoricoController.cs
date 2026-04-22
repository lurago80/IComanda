using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para histórico de alterações (auditoria)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class HistoricoController : ControllerBase
{
    private readonly IHistoricoService _historicoService;
    private readonly ILogger<HistoricoController> _logger;

    public HistoricoController(
        IHistoricoService historicoService,
        ILogger<HistoricoController> logger)
    {
        _historicoService = historicoService;
        _logger = logger;
    }

    /// <summary>
    /// Busca histórico de alterações
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HistoricoAlteracaoDto>), 200)]
    public async Task<ActionResult<IEnumerable<HistoricoAlteracaoDto>>> GetHistorico(
        [FromQuery] string? tipo = null,
        [FromQuery] string? entidadeId = null,
        [FromQuery] string? entidade = null,
        [FromQuery] string? idEntidade = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var tipoFiltro = !string.IsNullOrWhiteSpace(tipo) ? tipo : entidade;
            var entidadeIdFiltro = !string.IsNullOrWhiteSpace(entidadeId) ? entidadeId : idEntidade;
            var historico = await _historicoService.GetHistoricoAsync(tipoFiltro, entidadeIdFiltro, dataInicio, dataFim);
            return Ok(historico);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar histórico");
            var msg = ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : "");
            if (msg.IndexOf("HISTORICO", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("not exist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("does not exist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                msg.IndexOf("unknown", StringComparison.OrdinalIgnoreCase) >= 0)
                return StatusCode(503, new { mensagem = "Tabela de histórico não encontrada no banco. Execute o script Scripts/criar_tabela_historico_alteracoes.sql no Firebird para ativar o histórico (ex.: exclusões de comanda)." });
            return StatusCode(500, new { mensagem = "Erro ao carregar histórico", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Busca histórico por operador
    /// </summary>
    [HttpGet("operador/{operador}")]
    [ProducesResponseType(typeof(IEnumerable<HistoricoAlteracaoDto>), 200)]
    public async Task<ActionResult<IEnumerable<HistoricoAlteracaoDto>>> GetHistoricoPorOperador(
        int operador,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var historico = await _historicoService.GetHistoricoPorOperadorAsync(operador, dataInicio, dataFim);
            return Ok(historico);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar histórico do operador");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}

