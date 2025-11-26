using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de grupos de produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
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
            _logger.LogInformation("Buscando todos os grupos");

            var grupos = await _grupoService.GetAllGruposAsync();

            _logger.LogInformation("Encontrados {Count} grupos", grupos.Count());

            return Ok(grupos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar grupos");
            return StatusCode(500, "Erro interno do servidor");
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
            _logger.LogInformation("Buscando todos os grupos com quantidade de produtos");

            // Usar o endpoint básico que funciona
            var grupos = await _grupoService.GetAllGruposAsync();

            // Para cada grupo, buscar a quantidade de produtos
            var gruposComQuantidade = new List<GrupoDto>();

            foreach (var grupo in grupos)
            {
                try
                {
                    // Buscar produtos do grupo
                    var produtos = await _produtoService.BuscarProdutosAsync(new Models.Requests.BuscarProdutoRequest
                    {
                        Grupo = grupo.Id,
                        Ativo = true
                    });

                    gruposComQuantidade.Add(new GrupoDto
                    {
                        Id = grupo.Id,
                        Descricao = grupo.Descricao,
                        QuantidadeProdutos = produtos.Count()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao buscar produtos do grupo {GrupoId}, usando quantidade 0", grupo.Id);
                    gruposComQuantidade.Add(new GrupoDto
                    {
                        Id = grupo.Id,
                        Descricao = grupo.Descricao,
                        QuantidadeProdutos = 0
                    });
                }
            }

            _logger.LogInformation("Encontrados {Count} grupos", gruposComQuantidade.Count());

            return Ok(gruposComQuantidade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os grupos com quantidade");
            return StatusCode(500, "Erro interno do servidor");
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
}
