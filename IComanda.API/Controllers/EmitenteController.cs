using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de dados do emitente
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class EmitenteController : ControllerBase
{
    private readonly IEmitenteRepository _emitenteRepository;
    private readonly ILogger<EmitenteController> _logger;

    public EmitenteController(IEmitenteRepository emitenteRepository, ILogger<EmitenteController> logger)
    {
        _emitenteRepository = emitenteRepository;
        _logger = logger;
    }

    /// <summary>
    /// Obtém os dados do emitente
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Emitente), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Emitente>> GetEmitente()
    {
        try
        {
            _logger.LogInformation("🔍 Buscando dados do emitente");
            var emitente = await _emitenteRepository.GetEmitenteAsync();

            if (emitente == null)
            {
                _logger.LogWarning("⚠️ Emitente não encontrado");
                return NotFound(new { mensagem = "Emitente não encontrado" });
            }

            _logger.LogInformation("✅ Emitente encontrado: {Nome}", emitente.Nome);
            return Ok(emitente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar emitente");
            return StatusCode(500, new { mensagem = "Erro ao buscar emitente", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Insere ou atualiza os dados do emitente
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(Emitente), 200)]
    public async Task<IActionResult> SaveEmitente([FromBody] Emitente emitente)
    {
        try
        {
            _logger.LogInformation("💾 Salvando dados do emitente: {Nome}", emitente.Nome);
            await _emitenteRepository.SaveEmitenteAsync(emitente);
            var salvo = await _emitenteRepository.GetEmitenteAsync();
            return Ok(salvo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao salvar emitente");
            return StatusCode(500, new { mensagem = "Erro ao salvar emitente", detalhes = ex.Message });
        }
    }
}

