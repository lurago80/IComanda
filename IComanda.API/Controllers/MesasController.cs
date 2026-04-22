using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de mesas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class MesasController : ControllerBase
{
    private readonly IMesaService _mesaService;
    private readonly ILogger<MesasController> _logger;

    public MesasController(
        IMesaService mesaService,
        ILogger<MesasController> logger)
    {
        _mesaService = mesaService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as mesas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MesaDto>), 200)]
    public async Task<ActionResult<IEnumerable<MesaDto>>> GetMesas()
    {
        try
        {
            var mesas = await _mesaService.GetMesasAsync();
            return Ok(mesas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar mesas");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Busca mesa por número
    /// </summary>
    [HttpGet("{numero}")]
    [ProducesResponseType(typeof(MesaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<MesaDto>> GetMesa(int numero)
    {
        try
        {
            var mesa = await _mesaService.GetMesaPorNumeroAsync(numero);
            if (mesa == null)
            {
                return NotFound(new { mensagem = $"Mesa {numero} não encontrada" });
            }
            return Ok(mesa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar mesa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista mesas ocupadas
    /// </summary>
    [HttpGet("ocupadas")]
    [ProducesResponseType(typeof(IEnumerable<MesaDto>), 200)]
    public async Task<ActionResult<IEnumerable<MesaDto>>> GetMesasOcupadas()
    {
        try
        {
            var mesas = await _mesaService.GetMesasOcupadasAsync();
            return Ok(mesas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar mesas ocupadas");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista mesas livres
    /// </summary>
    [HttpGet("livres")]
    [ProducesResponseType(typeof(IEnumerable<MesaDto>), 200)]
    public async Task<ActionResult<IEnumerable<MesaDto>>> GetMesasLivres()
    {
        try
        {
            var mesas = await _mesaService.GetMesasLivresAsync();
            return Ok(mesas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar mesas livres");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Ocupa uma mesa
    /// </summary>
    [HttpPost("{numero}/ocupar")]
    [ProducesResponseType(typeof(MesaDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MesaDto>> OcuparMesa(
        int numero,
        [FromQuery] int comanda,
        [FromQuery] string nota,
        [FromQuery] int operador,
        [FromQuery] int? cliente = null,
        [FromQuery] int? numeroPessoas = null)
    {
        try
        {
            var mesa = await _mesaService.OcuparMesaAsync(numero, comanda, nota, operador, cliente, numeroPessoas);
            return Ok(mesa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ocupar mesa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Libera uma mesa
    /// </summary>
    [HttpPost("{numero}/liberar")]
    [ProducesResponseType(typeof(MesaDto), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MesaDto>> LiberarMesa(int numero)
    {
        try
        {
            var mesa = await _mesaService.LiberarMesaAsync(numero);
            return Ok(mesa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao liberar mesa");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }
}

