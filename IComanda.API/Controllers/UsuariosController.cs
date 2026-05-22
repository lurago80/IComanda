using IComanda.API.Extensions;
using IComanda.API.Models;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IComanda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles(UserRole.Admin, UserRole.Gerente)]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        ILogger<UsuariosController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>Lista todos os usuários</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        try
        {
            var usuarios = await _usuarioRepository.ListarTodosAsync();
            var dtos = usuarios.Select(MapToDto);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return StatusCode(500, new { error = "Erro ao listar usuários" });
        }
    }

    /// <summary>Busca um usuário por ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.BuscarPorIdAsync(id);
            if (usuario == null)
                return NotFound(new { error = "Usuário não encontrado" });

            return Ok(MapToDto(usuario));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário {Id}", id);
            return StatusCode(500, new { error = "Erro ao buscar usuário" });
        }
    }

    /// <summary>Cria um novo usuário</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CreateUsuarioRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { error = "Nome é obrigatório" });

            if (string.IsNullOrWhiteSpace(request.Senha))
                return BadRequest(new { error = "Senha é obrigatória" });

            var senhaHash = _passwordHasher.HashPassword(request.Senha);

            var usuario = new Usuario
            {
                Nome = request.Nome.Trim(),
                Senha = senhaHash,
                Ativo = request.Ativo ? "1" : "0",
                Bloqueio = request.Bloqueado ? "1" : "0",
                Visualizar = request.PodeVisualizar ? "1" : "0",
                Total = request.PodeVerTotal ? "1" : "0",
                Cancelar = request.PodeCancelar ? "1" : "0",
                Tipo = request.Tipo
            };

            var id = await _usuarioRepository.CriarAsync(usuario);
            usuario.Id = id;

            _logger.LogInformation("Usuário criado: {Nome} (ID={Id})", usuario.Nome, id);
            return CreatedAtAction(nameof(BuscarPorId), new { id }, MapToDto(usuario));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário");
            return StatusCode(500, new { error = "Erro ao criar usuário" });
        }
    }

    /// <summary>Atualiza dados de um usuário</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UpdateUsuarioRequest request)
    {
        try
        {
            var existente = await _usuarioRepository.BuscarPorIdAsync(id);
            if (existente == null)
                return NotFound(new { error = "Usuário não encontrado" });

            if (string.IsNullOrWhiteSpace(request.Nome))
                return BadRequest(new { error = "Nome é obrigatório" });

            existente.Nome = request.Nome.Trim();
            existente.Ativo = request.Ativo ? "1" : "0";
            existente.Bloqueio = request.Bloqueado ? "1" : "0";
            existente.Visualizar = request.PodeVisualizar ? "1" : "0";
            existente.Total = request.PodeVerTotal ? "1" : "0";
            existente.Cancelar = request.PodeCancelar ? "1" : "0";
            existente.Tipo = request.Tipo;

            await _usuarioRepository.AtualizarAsync(existente);
            _logger.LogInformation("Usuário atualizado: ID={Id}", id);
            return Ok(MapToDto(existente));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {Id}", id);
            return StatusCode(500, new { error = "Erro ao atualizar usuário" });
        }
    }

    /// <summary>Altera senha de um usuário</summary>
    [HttpPut("{id:int}/senha")]
    public async Task<IActionResult> AlterarSenha(int id, [FromBody] AlterarSenhaRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NovaSenha))
                return BadRequest(new { error = "Nova senha é obrigatória" });

            var existente = await _usuarioRepository.BuscarPorIdAsync(id);
            if (existente == null)
                return NotFound(new { error = "Usuário não encontrado" });

            var senhaHash = _passwordHasher.HashPassword(request.NovaSenha);
            await _usuarioRepository.AtualizarSenhaAsync(id, senhaHash);

            _logger.LogInformation("Senha alterada para usuário ID={Id}", id);
            return Ok(new { message = "Senha alterada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha do usuário {Id}", id);
            return StatusCode(500, new { error = "Erro ao alterar senha" });
        }
    }

    /// <summary>Desativa (exclui logicamente) um usuário</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            var existente = await _usuarioRepository.BuscarPorIdAsync(id);
            if (existente == null)
                return NotFound(new { error = "Usuário não encontrado" });

            await _usuarioRepository.ExcluirAsync(id);
            _logger.LogInformation("Usuário desativado: ID={Id}", id);
            return Ok(new { message = "Usuário desativado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir usuário {Id}", id);
            return StatusCode(500, new { error = "Erro ao excluir usuário" });
        }
    }

    private static UsuarioDto MapToDto(Usuario u) => new()
    {
        Id = u.Id,
        Nome = u.Nome ?? string.Empty,
        Ativo = u.Ativo == "1",
        Bloqueado = u.Bloqueio == "1",
        Tipo = u.Tipo ?? "0",
        PodeVisualizar = u.Visualizar == "1",
        PodeVerTotal = u.Total == "1",
        PodeCancelar = u.Cancelar == "1"
    };
}
