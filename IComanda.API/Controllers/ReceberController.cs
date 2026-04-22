using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de contas a receber (carteira)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ReceberController : ControllerBase
{
    private readonly IReceberService _receberService;
    private readonly ILogger<ReceberController> _logger;

    public ReceberController(
        IReceberService receberService,
        ILogger<ReceberController> logger)
    {
        _receberService = receberService;
        _logger = logger;
    }

    /// <summary>
    /// Busca uma conta a receber por número e ordem
    /// </summary>
    /// <param name="numero">Número da conta</param>
    /// <param name="ordem">Ordem da conta</param>
    /// <returns>Conta a receber</returns>
    /// <response code="200">Conta encontrada</response>
    /// <response code="404">Conta não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{numero}/{ordem}")]
    [ProducesResponseType(typeof(ReceberDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ReceberDto>> GetReceber(string numero, string ordem)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando conta a receber - Numero: {Numero}, Ordem: {Ordem}", numero, ordem);

            var receber = await _receberService.GetReceberPorNumeroOrdemAsync(numero, ordem);

            if (receber == null)
            {
                _logger.LogWarning("❌ Conta a receber {Numero}/{Ordem} não encontrada", numero, ordem);
                return NotFound(new { mensagem = $"Conta a receber {numero}/{ordem} não encontrada" });
            }

            _logger.LogInformation("✅ Conta a receber encontrada - Valor: {Valor}, Pendente: {Pendente}", 
                receber.Valor, receber.ValorPendente);

            return Ok(receber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar conta a receber {Numero}/{Ordem}", numero, ordem);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca contas a receber pendentes
    /// </summary>
    /// <param name="codigoCliente">Código do cliente (opcional)</param>
    /// <param name="dataVencimentoInicio">Data de vencimento inicial (opcional)</param>
    /// <param name="dataVencimentoFim">Data de vencimento final (opcional)</param>
    /// <returns>Lista de contas a receber pendentes</returns>
    /// <response code="200">Contas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("pendentes")]
    [ProducesResponseType(typeof(IEnumerable<ReceberDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ReceberDto>>> GetReceberPendentes(
        [FromQuery] int? codigoCliente = null,
        [FromQuery] DateTime? dataVencimentoInicio = null,
        [FromQuery] DateTime? dataVencimentoFim = null)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando contas a receber pendentes - Cliente: {Cliente}, Data Inicio: {DataInicio}, Data Fim: {DataFim}",
                codigoCliente, dataVencimentoInicio, dataVencimentoFim);

            var receber = await _receberService.GetReceberPendentesAsync(codigoCliente, dataVencimentoInicio, dataVencimentoFim);

            _logger.LogInformation("✅ Encontradas {Count} contas a receber pendentes", receber.Count());

            return Ok(receber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar contas a receber pendentes");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se o cliente possui contas a receber em aberto (valor devido)
    /// </summary>
    /// <param name="codigoCliente">Código do cliente</param>
    /// <returns>Informações sobre contas em aberto (temContasAberto, valorTotalPendente, quantidade, mensagem)</returns>
    /// <response code="200">Verificação concluída</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("contas-aberto/{codigoCliente}")]
    [ProducesResponseType(typeof(ContasAbertoDto), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ContasAbertoDto>> VerificarContasAberto(int codigoCliente)
    {
        try
        {
            _logger.LogInformation("🔍 Verificando contas em aberto do cliente {CodigoCliente}", codigoCliente);
            var resultado = await _receberService.VerificarContasAbertoAsync(codigoCliente);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao verificar contas em aberto do cliente {CodigoCliente}", codigoCliente);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca contas a receber por cliente
    /// </summary>
    /// <param name="codigoCliente">Código do cliente</param>
    /// <param name="apenasPendentes">Apenas contas pendentes (default: true)</param>
    /// <returns>Lista de contas a receber do cliente</returns>
    /// <response code="200">Contas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("cliente/{codigoCliente}")]
    [ProducesResponseType(typeof(IEnumerable<ReceberDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ReceberDto>>> GetReceberPorCliente(
        int codigoCliente,
        [FromQuery] bool apenasPendentes = true)
    {
        try
        {
            _logger.LogInformation("🔍 Buscando contas a receber do cliente {CodigoCliente} - Apenas pendentes: {ApenasPendentes}",
                codigoCliente, apenasPendentes);

            var receber = await _receberService.GetReceberPorClienteAsync(codigoCliente, apenasPendentes);

            _logger.LogInformation("✅ Encontradas {Count} contas a receber do cliente {CodigoCliente}", 
                receber.Count(), codigoCliente);

            return Ok(receber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar contas a receber do cliente {CodigoCliente}", codigoCliente);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Quita uma conta a receber (total ou parcial)
    /// </summary>
    /// <param name="request">Dados do quitamento</param>
    /// <returns>Resultado do quitamento</returns>
    /// <response code="200">Conta quitada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Conta não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("quitar")]
    [ProducesResponseType(typeof(QuitarReceberResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<QuitarReceberResponseDto>> QuitarReceber([FromBody] QuitarReceberRequest request)
    {
        try
        {
            _logger.LogInformation("💰 Quitando conta a receber - Numero: {Numero}, Ordem: {Ordem}, Valor: {Valor}",
                request.Numero, request.Ordem, request.ValorRecebido);

            var resultado = await _receberService.QuitarReceberAsync(request);

            _logger.LogInformation("✅ Conta {Numero}/{Ordem} quitada com sucesso - Total Recebido: {TotalRecebido}, Quitado: {Quitado}",
                request.Numero, request.Ordem, resultado.ValorTotalRecebido, resultado.TotalmenteQuitado);

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Dados inválidos para quitar conta");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "⚠️ Erro ao quitar conta");
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao quitar conta {Numero}/{Ordem}", request.Numero, request.Ordem);
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }
}

