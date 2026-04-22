using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Gerente")]
public class OperadorComissaoController : ControllerBase
{
    private readonly IOperadorComissaoRepository _repo;

    public OperadorComissaoController(IOperadorComissaoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<OperadorComissao>>> GetAll()
    {
        var list = await _repo.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OperadorComissao>> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("por-operador/{operadorId}")]
    public async Task<ActionResult<List<OperadorComissao>>> GetByOperador(int operadorId)
    {
        var list = await _repo.GetByOperadorAsync(operadorId);
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] OperadorComissao comissao)
    {
        await _repo.CreateAsync(comissao);
        return CreatedAtAction(nameof(GetById), new { id = comissao.Id }, comissao);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] OperadorComissao comissao)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound();
        comissao.Id = id;
        await _repo.UpdateAsync(comissao);
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

    [HttpGet("calcular")] // /api/operadorcomissao/calcular?operadorId=1&dataInicio=2024-01-01&dataFim=2024-01-31&percentual=5
    public async Task<ActionResult<decimal>> CalcularComissao(int operadorId, DateTime dataInicio, DateTime dataFim, decimal percentual)
    {
        var valor = await _repo.CalcularComissaoAsync(operadorId, dataInicio, dataFim, percentual);
        return Ok(valor);
    }
}
