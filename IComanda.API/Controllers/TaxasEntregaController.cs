using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TaxasEntregaController : ControllerBase
{
    private readonly ITaxaEntregaService _service;
    private readonly ILogger<TaxasEntregaController> _logger;

    public TaxasEntregaController(ITaxaEntregaService service, ILogger<TaxasEntregaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaxaEntregaDto>), 200)]
    public async Task<ActionResult<IEnumerable<TaxaEntregaDto>>> GetAll()
    {
        try
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar taxas de entrega");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaxaEntregaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TaxaEntregaDto>> GetById(int id)
    {
        try
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar taxa de entrega {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaxaEntregaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TaxaEntregaDto>> Criar([FromBody] CriarTaxaEntregaRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Descricao))
                return BadRequest(new { error = "A descrição é obrigatória." });
            var dto = await _service.CriarAsync(request.Descricao, request.Valor);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar taxa de entrega");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaxaEntregaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TaxaEntregaDto>> Atualizar(int id, [FromBody] AtualizarTaxaEntregaRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Descricao))
                return BadRequest(new { error = "A descrição é obrigatória." });
            var dto = await _service.AtualizarAsync(id, request.Descricao, request.Valor);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar taxa de entrega {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Excluir(int id)
    {
        try
        {
            var ok = await _service.ExcluirAsync(id);
            if (!ok) return NotFound();
            return Ok(new { message = "Taxa de entrega excluída com sucesso." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir taxa de entrega {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
