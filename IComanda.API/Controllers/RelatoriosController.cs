using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para relatórios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;
    private readonly IVendaRepository _vendaRepository;
    private readonly ILogger<RelatoriosController> _logger;

    public RelatoriosController(
        IRelatorioService relatorioService,
        IVendaRepository vendaRepository,
        ILogger<RelatoriosController> logger)
    {
        _relatorioService = relatorioService;
        _vendaRepository = vendaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Relatório completo de compras de um cliente
    /// </summary>
    /// <param name="codigoCliente">Código do cliente</param>
    /// <param name="dataInicio">Data inicial (opcional)</param>
    /// <param name="dataFim">Data final (opcional)</param>
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    /// <returns>Relatório de compras do cliente</returns>
    [HttpGet("cliente/{codigoCliente}")]
    [ProducesResponseType(typeof(RelatorioClienteDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<RelatorioClienteDto>> GetRelatorioCliente(
        int codigoCliente,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] string? origem = null)
    {
        try
        {
            _logger.LogInformation("📊 Gerando relatório de cliente {CodigoCliente}, Origem: {Origem}", codigoCliente, origem ?? "todos");
            var relatorio = await _relatorioService.GetRelatorioClienteAsync(codigoCliente, dataInicio, dataFim, origem);
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de cliente");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Relatório de vendas por período (comandas/pedidos fechados)
    /// </summary>
    /// <param name="dataInicio">Data inicial do período</param>
    /// <param name="dataFim">Data final do período</param>
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    /// <returns>Relatório de vendas (comandas/pedidos fechados no período)</returns>
    [HttpGet("vendas")]
    [ProducesResponseType(typeof(RelatorioVendasDto), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<RelatorioVendasDto>> GetRelatorioVendas(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null,
        [FromQuery] string? origem = null)
    {
        try
        {
            var inicio = dataInicio ?? DateTime.Today;
            var fim = dataFim ?? DateTime.Today;
            if (inicio > fim)
            {
                return BadRequest(new { mensagem = "Data inicial não pode ser maior que data final" });
            }
            _logger.LogInformation("📊 Gerando relatório de vendas - Período: {DataInicio} a {DataFim}, Origem: {Origem}", inicio, fim, origem ?? "todos");
            var relatorio = await _relatorioService.GetRelatorioVendasPorPeriodoAsync(inicio, fim, origem);
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de vendas");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Relatório de produtos mais vendidos
    /// </summary>
    /// <param name="dataInicio">Data inicial</param>
    /// <param name="dataFim">Data final</param>
    /// <param name="top">Quantidade de produtos (default: 10)</param>
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    [HttpGet("produtos-mais-vendidos")]
    [ProducesResponseType(typeof(RelatorioProdutosMaisVendidosDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<RelatorioProdutosMaisVendidosDto>> GetProdutosMaisVendidos(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] int top = 10,
        [FromQuery] string? origem = null)
    {
        try
        {
            if (dataInicio > dataFim)
            {
                return BadRequest(new { mensagem = "Data inicial não pode ser maior que data final" });
            }

            _logger.LogInformation("📊 Gerando relatório de produtos mais vendidos - Período: {DataInicio} a {DataFim}, Origem: {Origem}", 
                dataInicio, dataFim, origem ?? "todos");
            
            var relatorio = await _relatorioService.GetProdutosMaisVendidosAsync(dataInicio, dataFim, top, origem);
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de produtos mais vendidos");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Relatório por período com itens vendidos e recebimentos por forma de pagamento
    /// </summary>
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    /// <returns>Relatório completo por período</returns>
    [HttpGet("periodo")]
    [ProducesResponseType(typeof(RelatorioPeriodoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<RelatorioPeriodoDto>> GetRelatorioPeriodo(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] string? origem = null)
    {
        try
        {
            if (dataInicio > dataFim)
            {
                return BadRequest(new { mensagem = "Data inicial não pode ser maior que data final" });
            }

            _logger.LogInformation("📊 Gerando relatório por período - Data Inicio: {DataInicio}, Data Fim: {DataFim}, Origem: {Origem}", 
                dataInicio, dataFim, origem ?? "todos");
            
            var relatorio = await _relatorioService.GetRelatorioPeriodoAsync(dataInicio, dataFim, origem);
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório por período");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// <param name="dataInicio">Data inicial</param>
    /// <param name="dataFim">Data final</param>
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    [HttpGet("caixa-consolidado")]
    [ProducesResponseType(typeof(RelatorioCaixaConsolidadoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<RelatorioCaixaConsolidadoDto>> GetRelatorioCaixaConsolidado(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] string? origem = null)
    {
        try
        {
            if (dataInicio > dataFim)
                return BadRequest(new { mensagem = "Data inicial não pode ser maior que data final" });
            var relatorio = await _relatorioService.GetRelatorioCaixaConsolidadoAsync(dataInicio, dataFim, origem);
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório caixa consolidado");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(RelatorioDashboardDto), 200)]
    public async Task<ActionResult<RelatorioDashboardDto>> GetDashboard(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        [FromQuery] string? origem = null)
    {
        try
        {
            if (dataInicio > dataFim)
                return BadRequest(new { mensagem = "Data inicial não pode ser maior que data final" });
            var relatorio = await _relatorioService.GetDashboardAsync(dataInicio, dataFim, origem);
            return Ok(relatorio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar dashboard");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Relatório de vendas canceladas com filtro por período
    /// </summary>
    [HttpGet("cancelamentos")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetCancelamentos(
        [FromQuery] DateTime? de = null,
        [FromQuery] DateTime? ate = null,
        [FromQuery] string origem = "BA")
    {
        try
        {
            _logger.LogInformation("📋 Gerando relatório de cancelamentos. De: {De}, Até: {Ate}, Origem: {Origem}", de, ate, origem);
            var vendas = await _vendaRepository.GetVendasCanceladasAsync(origem, de, ate);
            var lista = vendas.Select(v => new
            {
                nota          = v.Nota,
                emissao       = v.Emissao,
                hora          = v.Hora,
                comanda       = v.Comanda,
                mesa          = v.Mesa,
                nomeCliente   = v.NomeCliente ?? (v.Cliente > 0 ? v.Cliente.ToString() : null),
                operador      = v.Operador,
                total         = v.Total,
                justificativa = v.Justificativa
            });
            return Ok(lista);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de cancelamentos");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }
}

