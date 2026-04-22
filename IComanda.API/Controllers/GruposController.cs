using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de grupos de produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class GruposController : ControllerBase
{
    private readonly IGrupoService _grupoService;
    private readonly IProdutoService _produtoService;
    private readonly ILogger<GruposController> _logger;

    public GruposController(IGrupoService grupoService, IProdutoService produtoService, ILogger<GruposController> logger)
    {
        _grupoService = grupoService;
        _produtoService = produtoService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os grupos de produtos
    /// </summary>
    /// <returns>Lista de grupos</returns>
    /// <response code="200">Grupos encontrados com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GrupoDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<GrupoDto>>> GetAllGrupos()
    {
        try
        {
            _logger.LogInformation("🔍 [GruposController] Buscando todos os grupos");

            var grupos = await _grupoService.GetAllGruposAsync();
            var gruposList = grupos.ToList();

            _logger.LogInformation("✅ [GruposController] Encontrados {Count} grupos", gruposList.Count);
            
            if (gruposList.Count > 0)
            {
                _logger.LogInformation("📦 [GruposController] Primeiros grupos: {Grupos}", 
                    string.Join(", ", gruposList.Take(5).Select(g => $"{g.Id}:{g.Descricao}")));
            }
            else
            {
                _logger.LogWarning("⚠️ [GruposController] Nenhum grupo encontrado na tabela GRUPO");
            }

            return Ok(gruposList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GruposController] Erro ao buscar grupos. Erro: {Message}", ex.Message);
            return StatusCode(500, new { error = $"Erro ao buscar grupos: {ex.Message}" });
        }
    }

    /// <summary>
    /// Lista grupos com quantidade de produtos ativos (apenas grupos com produtos)
    /// </summary>
    /// <returns>Lista de grupos com contagem de produtos</returns>
    /// <response code="200">Grupos encontrados com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("com-quantidade")]
    [ProducesResponseType(typeof(IEnumerable<GrupoDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<GrupoDto>>> GetGruposComQuantidade()
    {
        try
        {
            _logger.LogInformation("Buscando grupos com quantidade de produtos");

            var grupos = await _grupoService.GetGruposComQuantidadeAsync();

            _logger.LogInformation("Encontrados {Count} grupos com produtos", grupos.Count());

            return Ok(grupos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar grupos com quantidade");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista todos os grupos com quantidade de produtos (incluindo grupos vazios)
    /// </summary>
    /// <returns>Lista de todos os grupos com contagem de produtos</returns>
    /// <response code="200">Grupos encontrados com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("todos-com-quantidade")]
    [ProducesResponseType(typeof(IEnumerable<GrupoDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<GrupoDto>>> GetGruposTodosComQuantidade()
    {
        try
        {
            _logger.LogInformation("🔍 [GruposController] Buscando todos os grupos com quantidade de produtos");

            // Usar o método do repositório que já faz o JOIN
            var grupos = await _grupoService.GetGruposComQuantidadeTodosAsync();
            var gruposList = grupos.ToList();

            _logger.LogInformation("✅ [GruposController] Encontrados {Count} grupos com quantidade", gruposList.Count);
            
            if (gruposList.Count > 0)
            {
                _logger.LogInformation("📦 [GruposController] Primeiros grupos: {Grupos}", 
                    string.Join(", ", gruposList.Take(5).Select(g => $"{g.Id}:{g.Descricao} ({g.QuantidadeProdutos} produtos)")));
            }
            else
            {
                _logger.LogWarning("⚠️ [GruposController] Nenhum grupo encontrado");
            }

            return Ok(gruposList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GruposController] Erro ao buscar todos os grupos com quantidade. Erro: {Message}", ex.Message);
            return StatusCode(500, new { error = $"Erro ao buscar grupos: {ex.Message}" });
        }
    }

    /// <summary>
    /// Obtém um grupo por ID
    /// </summary>
    /// <param name="id">ID do grupo</param>
    /// <returns>Dados do grupo</returns>
    /// <response code="200">Grupo encontrado</response>
    /// <response code="404">Grupo não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GrupoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GrupoDto>> GetGrupo(int id)
    {
        try
        {
            _logger.LogInformation("Buscando grupo com ID: {Id}", id);

            var grupo = await _grupoService.GetGrupoAsync(id);

            if (grupo == null)
            {
                _logger.LogWarning("Grupo com ID {Id} não encontrado", id);
                return NotFound($"Grupo com ID {id} não encontrado");
            }

            _logger.LogInformation("Grupo encontrado: {Descricao}", grupo.Descricao);
            return Ok(grupo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar grupo com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Cria um novo grupo
    /// </summary>
    /// <param name="request">Dados do grupo a ser criado</param>
    /// <returns>Grupo criado</returns>
    /// <response code="201">Grupo criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(GrupoDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GrupoDto>> CriarGrupo([FromBody] CriarGrupoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Descricao))
            {
                return BadRequest(new { error = "A descrição do grupo é obrigatória" });
            }

            _logger.LogInformation("🔍 [GruposController] Criando grupo - Descricao: {Descricao}", request.Descricao);

            var grupo = await _grupoService.CriarGrupoAsync(request.Descricao, request.ImprimirDuasVias);

            _logger.LogInformation("✅ [GruposController] Grupo criado com sucesso - ID: {Id}", grupo.Id);

            return CreatedAtAction(nameof(GetGrupo), new { id = grupo.Id }, grupo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GruposController] Erro ao criar grupo. Mensagem: {Message}", ex.Message);
            return StatusCode(500, new { error = $"Erro ao criar grupo: {ex.Message}" });
        }
    }

    /// <summary>
    /// Atualiza um grupo existente
    /// </summary>
    /// <param name="id">ID do grupo</param>
    /// <param name="request">Dados do grupo a ser atualizado</param>
    /// <returns>Grupo atualizado</returns>
    /// <response code="200">Grupo atualizado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Grupo não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(GrupoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GrupoDto>> AtualizarGrupo(int id, [FromBody] AtualizarGrupoRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Descricao))
            {
                return BadRequest(new { error = "A descrição do grupo é obrigatória" });
            }

            _logger.LogInformation("🔍 [GruposController] Atualizando grupo - ID: {Id}, Descricao: {Descricao}", id, request.Descricao);

            var grupo = await _grupoService.AtualizarGrupoAsync(id, request.Descricao, request.ImprimirDuasVias);

            _logger.LogInformation("✅ [GruposController] Grupo atualizado com sucesso - ID: {Id}", id);

            return Ok(grupo);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("⚠️ [GruposController] Grupo com ID {Id} não encontrado", id);
            return NotFound(new { error = $"Grupo com ID {id} não encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GruposController] Erro ao atualizar grupo. Mensagem: {Message}", ex.Message);
            return StatusCode(500, new { error = $"Erro ao atualizar grupo: {ex.Message}" });
        }
    }

    /// <summary>
    /// Exclui um grupo
    /// </summary>
    /// <param name="id">ID do grupo</param>
    /// <returns>Resultado da exclusão</returns>
    /// <response code="200">Grupo excluído com sucesso</response>
    /// <response code="400">Não é possível excluir o grupo (há produtos associados)</response>
    /// <response code="404">Grupo não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> ExcluirGrupo(int id)
    {
        try
        {
            _logger.LogInformation("🔍 [GruposController] Excluindo grupo - ID: {Id}", id);

            var excluido = await _grupoService.ExcluirGrupoAsync(id);

            if (!excluido)
            {
                _logger.LogWarning("⚠️ [GruposController] Grupo com ID {Id} não encontrado", id);
                return NotFound(new { error = $"Grupo com ID {id} não encontrado" });
            }

            _logger.LogInformation("✅ [GruposController] Grupo excluído com sucesso - ID: {Id}", id);

            return Ok(new { message = "Grupo excluído com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ [GruposController] Não é possível excluir grupo {Id}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [GruposController] Erro ao excluir grupo. Mensagem: {Message}", ex.Message);
            return StatusCode(500, new { error = $"Erro ao excluir grupo: {ex.Message}" });
        }
    }
}
