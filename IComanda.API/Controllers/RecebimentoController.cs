using IComanda.API.Models.Entities;
using IComanda.API.Models.Enums;
using IComanda.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Gerente,Caixa")]
public class RecebimentoController : ControllerBase
{
    private readonly IRecebimentoRepository _repo;

    public RecebimentoController(IRecebimentoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<Recebimento>>> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Recebimento>> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("por-comanda/{comandaId}")]
    public async Task<ActionResult<List<Recebimento>>> GetByComanda(int comandaId)
    {
        var list = await _repo.GetByComandaAsync(comandaId);
        return Ok(list);
    }

    [HttpGet("por-forma/{forma}")]
    public async Task<ActionResult<List<Recebimento>>> GetByFormaPagamento(IComanda.API.Models.Enums.FormaPagamento forma)
    {
        var list = await _repo.GetByFormaPagamentoAsync(forma);
        return Ok(list);
    }

    [HttpGet("pendentes")]
    public async Task<ActionResult<List<Recebimento>>> GetPendentes()
    {
        var list = await _repo.GetPendentesAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Recebimento recebimento)
    {
        await _repo.CreateAsync(recebimento);
        return CreatedAtAction(nameof(GetById), new { id = recebimento.Id }, recebimento);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] Recebimento recebimento)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound();
        recebimento.Id = id;
        await _repo.UpdateAsync(recebimento);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound();
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
