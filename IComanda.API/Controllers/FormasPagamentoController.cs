using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.Entities;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Gerenciamento de formas de pagamento (Admin/Gerente)
/// </summary>
[ApiController]
[Route("api/formas-pagamento")]
[Produces("application/json")]
[Authorize]
public class FormasPagamentoController : ControllerBase
{
    private readonly IFormaPagamentoRepository _repo;
    private readonly ILogger<FormasPagamentoController> _logger;

    public FormasPagamentoController(
        IFormaPagamentoRepository repo,
        ILogger<FormasPagamentoController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>Lista todas as formas de pagamento (ativas e inativas)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FormaPagamentoDto>), 200)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var formas = await _repo.GetAllAsync();
            var dto = formas.Select(ToDto);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar formas de pagamento");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Atualiza uma forma de pagamento</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFormaPagamentoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Descricao))
            return BadRequest(new { mensagem = "Descrição é obrigatória." });

        try
        {
            var existente = await _repo.GetFormaPagamentoByIdAsync(id);
            if (existente is null)
                return NotFound(new { mensagem = "Forma de pagamento não encontrada." });

            existente.Descricao     = request.Descricao.Trim().ToUpperInvariant();
            existente.Indice        = request.Indice;
            existente.MeioPagto     = request.MeioPagto;
            existente.PermiteTroco  = request.PermiteTroco ? (short)1 : (short)0;
            existente.Tipo          = request.Tipo;

            var ok = await _repo.UpdateAsync(existente);
            if (!ok)
                return StatusCode(500, new { mensagem = "Nenhuma linha foi atualizada." });

            _logger.LogInformation("✅ Forma de pagamento {Id} atualizada", id);
            return Ok(ToDto(existente));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar forma de pagamento {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Alterna ativo/inativo de uma forma de pagamento</summary>
    [HttpPatch("{id:int}/ativo")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleAtivo(int id)
    {
        try
        {
            var existente = await _repo.GetFormaPagamentoByIdAsync(id);
            if (existente is null)
                return NotFound(new { mensagem = "Forma de pagamento não encontrada." });

            var ok = await _repo.ToggleAtivoAsync(id);
            if (!ok)
                return StatusCode(500, new { mensagem = "Não foi possível alterar o status." });

            _logger.LogInformation("🔄 Forma de pagamento {Id} teve status alternado", id);
            return Ok(new { id, ativo = existente.Ativo == 0 }); // retorna novo estado
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alternar status de forma de pagamento {Id}", id);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>Cria uma nova forma de pagamento</summary>
    [HttpPost]
    [ProducesResponseType(typeof(FormaPagamentoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateFormaPagamentoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Descricao))
            return BadRequest(new { mensagem = "Descrição é obrigatória." });

        try
        {
            var forma = new FormaPagamento
            {
                Descricao    = request.Descricao.Trim().ToUpperInvariant(),
                Ativo        = 1,
                Indice       = request.Indice,
                MeioPagto    = request.MeioPagto,
                PermiteTroco = request.PermiteTroco ? (short)1 : (short)0,
                Tipo         = request.Tipo
            };

            var novoId = await _repo.CreateAsync(forma);
            forma.Id = novoId;

            _logger.LogInformation("➕ Nova forma de pagamento criada: {Descricao} (ID {Id})", forma.Descricao, novoId);
            return CreatedAtAction(nameof(GetAll), ToDto(forma));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar forma de pagamento");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    private static FormaPagamentoDto ToDto(FormaPagamento f) => new()
    {
        Id           = f.Id,
        Descricao    = f.Descricao,
        Ativo        = f.IsAtivo,
        Indice       = f.Indice,
        Moeda        = f.Moeda,
        MeioPagto    = f.MeioPagto,
        PermiteTroco = f.PermiteTrocoAtivo,
        Tipo         = f.Tipo
    };
}

public record UpdateFormaPagamentoRequest(
    string Descricao,
    int? Indice,
    short MeioPagto,
    bool PermiteTroco,
    string? Tipo);

public record CreateFormaPagamentoRequest(
    string Descricao,
    int? Indice,
    short MeioPagto,
    bool PermiteTroco,
    string? Tipo);
