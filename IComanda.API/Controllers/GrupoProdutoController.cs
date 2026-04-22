using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Gerente")]
public class GrupoProdutoController : ControllerBase
{
    private readonly IGrupoProdutoRepository _repo;

    public GrupoProdutoController(IGrupoProdutoRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<GrupoProdutoDto>>> GetAll()
    {
        var grupos = await _repo.GetAllAsync();
        var dtos = grupos.Select(g => new GrupoProdutoDto
        {
            Id = g.Id,
            Nome = g.Nome,
            Ativo = g.Ativo,
            Ordem = g.Ordem
        }).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GrupoProdutoDto>> GetById(int id)
    {
        var grupo = await _repo.GetByIdAsync(id);
        if (grupo == null) return NotFound();
        return Ok(new GrupoProdutoDto
        {
            Id = grupo.Id,
            Nome = grupo.Nome,
            Ativo = grupo.Ativo,
            Ordem = grupo.Ordem
        });
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] GrupoProdutoDto dto)
    {
        var grupo = new GrupoProduto
        {
            Nome = dto.Nome,
            Ativo = dto.Ativo,
            Ordem = dto.Ordem
        };
        await _repo.CreateAsync(grupo);
        return CreatedAtAction(nameof(GetById), new { id = grupo.Id }, dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] GrupoProdutoDto dto)
    {
        var grupo = await _repo.GetByIdAsync(id);
        if (grupo == null) return NotFound();
        grupo.Nome = dto.Nome;
        grupo.Ativo = dto.Ativo;
        grupo.Ordem = dto.Ordem;
        await _repo.UpdateAsync(grupo);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var grupo = await _repo.GetByIdAsync(id);
        if (grupo == null) return NotFound();
        await _repo.DeleteAsync(id);
        return NoContent();
    }
}
