using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para Dashboard e Metas de Força de Vendas
/// </summary>
[ApiController]
[Route("api/forcavendas/dashboard")]
[Produces("application/json")]
[Authorize]
public class DashboardFVController : ControllerBase
{
    private readonly IMetaFVService _metaService;
    private readonly ILogger<DashboardFVController> _logger;

    public DashboardFVController(IMetaFVService metaService, ILogger<DashboardFVController> logger)
    {
        _metaService = metaService;
        _logger      = logger;
    }

    /// <summary>Dashboard completo do vendedor (meta, pedidos, visitas)</summary>
    [HttpGet("vendedor/{idVendedor:int}")]
    [ProducesResponseType(typeof(DashboardVendedorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<DashboardVendedorDto>> GetDashboard(int idVendedor)
    {
        try
        {
            var dashboard = await _metaService.GetDashboardAsync(idVendedor);
            return Ok(dashboard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dashboard do vendedor {Id}", idVendedor);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Ranking de vendedores do mês</summary>
    [HttpGet("ranking")]
    [Authorize(Policy = "GerenteOrAdmin")]
    [ProducesResponseType(typeof(IEnumerable<MetaFVDto>), 200)]
    public async Task<ActionResult<IEnumerable<MetaFVDto>>> GetRanking(
        [FromQuery] int? mes,
        [FromQuery] int? ano)
    {
        try
        {
            var agora = DateTime.Now;
            var m = mes ?? agora.Month;
            var a = ano ?? agora.Year;
            var ranking = await _metaService.GetRankingMesAsync(m, a);
            return Ok(ranking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar ranking FV");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Histórico de metas de um vendedor</summary>
    [HttpGet("vendedor/{idVendedor:int}/metas")]
    [ProducesResponseType(typeof(IEnumerable<MetaFVDto>), 200)]
    public async Task<ActionResult<IEnumerable<MetaFVDto>>> GetMetas(int idVendedor)
    {
        try
        {
            var metas = await _metaService.GetByVendedorAsync(idVendedor);
            return Ok(metas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar metas do vendedor {Id}", idVendedor);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Define ou atualiza a meta mensal de um vendedor</summary>
    [HttpPost("metas")]
    [Authorize(Policy = "GerenteOrAdmin")]
    [ProducesResponseType(typeof(MetaFVDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MetaFVDto>> DefinirMeta([FromBody] DefinirMetaFVRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var meta = await _metaService.DefinirMetaAsync(request);
            return CreatedAtAction(nameof(GetMetas), new { idVendedor = request.IdVendedor }, meta);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir meta FV");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}
