using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PizzaController : ControllerBase
{
    private readonly IPizzaRepository _repo;
    private readonly ILogger<PizzaController> _logger;

    public PizzaController(IPizzaRepository repo, ILogger<PizzaController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // ── TAMANHOS ─────────────────────────────────────────────────────────────

    [HttpGet("grupos/{grupoId}/tamanhos")]
    public async Task<IActionResult> GetTamanhos(int grupoId, [FromQuery] bool comSabores = false)
    {
        var tamanhos = await _repo.GetTamanhosPorGrupoAsync(grupoId, comSabores);
        return Ok(tamanhos);
    }

    [HttpPost("grupos/{grupoId}/tamanhos")]
    public async Task<IActionResult> CriarTamanho(int grupoId, [FromBody] TamanhoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Descricao))
            return BadRequest(new { mensagem = "Descrição obrigatória" });
        var id = await _repo.CriarTamanhoAsync(grupoId, req.Descricao.Trim(), req.Ordem);
        return Ok(new { id });
    }

    [HttpPut("tamanhos/{id}")]
    public async Task<IActionResult> AtualizarTamanho(int id, [FromBody] TamanhoRequest req)
    {
        var ok = await _repo.AtualizarTamanhoAsync(id, req.Descricao.Trim(), req.Ordem);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("tamanhos/{id}")]
    public async Task<IActionResult> ExcluirTamanho(int id)
    {
        var ok = await _repo.ExcluirTamanhoAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // ── SABORES ──────────────────────────────────────────────────────────────

    [HttpGet("tamanhos/{tamanhoId}/sabores")]
    public async Task<IActionResult> GetSabores(int tamanhoId, [FromQuery] bool apenasAtivos = false)
    {
        var sabores = await _repo.GetSaboresPorTamanhoAsync(tamanhoId, apenasAtivos);
        return Ok(sabores);
    }

    [HttpPost("tamanhos/{tamanhoId}/sabores")]
    public async Task<IActionResult> CriarSabor(int tamanhoId, [FromBody] SaborRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Descricao))
            return BadRequest(new { mensagem = "Descrição obrigatória" });
        var id = await _repo.CriarSaborAsync(tamanhoId, req.Descricao.Trim(), req.Ingredientes?.Trim(), req.Preco);
        return Ok(new { id });
    }

    [HttpPut("sabores/{id}")]
    public async Task<IActionResult> AtualizarSabor(int id, [FromBody] SaborRequest req)
    {
        var ok = await _repo.AtualizarSaborAsync(id, req.Descricao.Trim(), req.Ingredientes?.Trim(), req.Preco, req.Ativo);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("sabores/{id}")]
    public async Task<IActionResult> ExcluirSabor(int id)
    {
        var ok = await _repo.ExcluirSaborAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // ── BORDAS ───────────────────────────────────────────────────────────────

    [HttpGet("bordas")]
    public async Task<IActionResult> GetBordas([FromQuery] bool apenasAtivas = false)
    {
        var bordas = await _repo.GetBordasAsync(apenasAtivas);
        return Ok(bordas);
    }

    [HttpPost("bordas")]
    public async Task<IActionResult> CriarBorda([FromBody] BordaRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Descricao))
            return BadRequest(new { mensagem = "Descrição obrigatória" });
        var id = await _repo.CriarBordaAsync(req.Descricao.Trim(), req.Preco);
        return Ok(new { id });
    }

    [HttpPut("bordas/{id}")]
    public async Task<IActionResult> AtualizarBorda(int id, [FromBody] BordaRequest req)
    {
        var ok = await _repo.AtualizarBordaAsync(id, req.Descricao.Trim(), req.Preco, req.Ativo);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("bordas/{id}")]
    public async Task<IActionResult> ExcluirBorda(int id)
    {
        var ok = await _repo.ExcluirBordaAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // ── TIPO DO GRUPO ────────────────────────────────────────────────────────

    [HttpPatch("grupos/{grupoId}/tipo")]
    public async Task<IActionResult> AtualizarTipoGrupo(int grupoId, [FromBody] TipoGrupoRequest req)
    {
        var tipo = req.Tipo?.ToUpperInvariant();
        if (tipo != "NORMAL" && tipo != "PIZZA")
            return BadRequest(new { mensagem = "Tipo deve ser NORMAL ou PIZZA" });
        var ok = await _repo.AtualizarTipoGrupoAsync(grupoId, tipo);
        return ok ? NoContent() : NotFound();
    }
}

// ── Request records ──────────────────────────────────────────────────────────

public record TamanhoRequest(string Descricao, int Ordem = 0);
public record SaborRequest(string Descricao, string? Ingredientes, decimal Preco, bool Ativo = true);
public record BordaRequest(string Descricao, decimal Preco, bool Ativo = true);
public record TipoGrupoRequest(string Tipo);
