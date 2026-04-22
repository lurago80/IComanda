using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de Vendedores (Força de Vendas)
/// </summary>
[ApiController]
[Route("api/forcavendas/vendedores")]
[Produces("application/json")]
[Authorize]
public class VendedoresFVController : ControllerBase
{
    private readonly IVendedorService _vendedorService;
    private readonly ILogger<VendedoresFVController> _logger;

    public VendedoresFVController(IVendedorService vendedorService, ILogger<VendedoresFVController> logger)
    {
        _vendedorService = vendedorService;
        _logger          = logger;
    }

    /// <summary>Busca vendedores com filtros</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VendedorDto>), 200)]
    public async Task<ActionResult<IEnumerable<VendedorDto>>> BuscarVendedores([FromQuery] BuscarVendedorRequest request)
    {
        try
        {
            var vendedores = await _vendedorService.BuscarAsync(request);
            return Ok(vendedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendedores");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Lista todos os vendedores ativos</summary>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<VendedorDto>), 200)]
    public async Task<ActionResult<IEnumerable<VendedorDto>>> GetAtivos()
    {
        try
        {
            var vendedores = await _vendedorService.GetAtivosAsync();
            return Ok(vendedores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar vendedores ativos");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Busca vendedor por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VendedorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VendedorDto>> GetVendedor(int id)
    {
        try
        {
            var vendedor = await _vendedorService.GetByIdAsync(id);
            if (vendedor == null) return NotFound(new { mensagem = $"Vendedor {id} não encontrado" });
            return Ok(vendedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendedor {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Cria um novo vendedor</summary>
    [HttpPost]
    [ProducesResponseType(typeof(VendedorDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<VendedorDto>> CriarVendedor([FromBody] VendedorDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest(new { mensagem = "Nome do vendedor é obrigatório" });

            var vendedor = await _vendedorService.CriarAsync(dto);
            return CreatedAtAction(nameof(GetVendedor), new { id = vendedor.Id }, vendedor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar vendedor");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Atualiza um vendedor</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VendedorDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<VendedorDto>> AtualizarVendedor(int id, [FromBody] VendedorDto dto)
    {
        try
        {
            var vendedor = await _vendedorService.AtualizarAsync(id, dto);
            return Ok(vendedor);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar vendedor {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Ativa ou desativa um vendedor</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AlterarStatus(int id, [FromQuery] bool ativo)
    {
        try
        {
            await _vendedorService.AlterarStatusAsync(id, ativo);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar status do vendedor {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Define ou redefine a senha de acesso do vendedor</summary>
    [HttpPatch("{id:int}/senha")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AlterarSenha(int id, [FromBody] AlterarSenhaVendedorRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NovaSenha))
                return BadRequest(new { mensagem = "Nova senha é obrigatória" });

            if (request.NovaSenha.Length < 4)
                return BadRequest(new { mensagem = "Senha deve ter no mínimo 4 caracteres" });

            await _vendedorService.AlterarSenhaAsync(id, request.NovaSenha);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha do vendedor {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }
}
