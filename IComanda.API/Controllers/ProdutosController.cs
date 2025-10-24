using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de produtos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(IProdutoService produtoService, ILogger<ProdutosController> logger)
    {
        _produtoService = produtoService;
        _logger = logger;
    }

    /// <summary>
    /// Busca produtos por termo, grupo ou filtros
    /// </summary>
    /// <param name="request">Parâmetros de busca</param>
    /// <returns>Lista de produtos encontrados</returns>
    /// <response code="200">Produtos encontrados com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> BuscarProdutos([FromQuery] BuscarProdutoRequest request)
    {
        try
        {
            _logger.LogInformation("Buscando produtos com termo: {Termo}", request.Q);

            var produtos = await _produtoService.BuscarProdutosAsync(request);

            _logger.LogInformation("Encontrados {Count} produtos", produtos.Count());

            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Busca produtos com paginação
    /// </summary>
    /// <param name="request">Parâmetros de busca e paginação</param>
    /// <returns>Lista paginada de produtos</returns>
    /// <response code="200">Produtos encontrados com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("buscar-paginado")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> BuscarProdutosPaginado([FromQuery] BuscarProdutoRequest request)
    {
        try
        {
            _logger.LogInformation("Buscando produtos paginados - Página: {Pagina}, Itens: {Itens}",
                request.Pagina, request.ItensPorPagina);

            var (produtos, total) = await _produtoService.BuscarProdutosComPaginacaoAsync(request);

            var resultado = new
            {
                Produtos = produtos,
                Total = total,
                Pagina = request.Pagina,
                ItensPorPagina = request.ItensPorPagina,
                TotalPaginas = (int)Math.Ceiling((double)total / request.ItensPorPagina)
            };

            _logger.LogInformation("Encontrados {Count} produtos de {Total} total", produtos.Count(), total);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos paginados");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um produto por ID
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Dados do produto</returns>
    /// <response code="200">Produto encontrado</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProdutoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ProdutoDto>> GetProduto(int id)
    {
        try
        {
            _logger.LogInformation("Buscando produto com ID: {Id}", id);

            var produto = await _produtoService.GetProdutoAsync(id);

            if (produto == null)
            {
                _logger.LogWarning("Produto com ID {Id} não encontrado", id);
                return NotFound($"Produto com ID {id} não encontrado");
            }

            _logger.LogInformation("Produto encontrado: {Descricao}", produto.Descricao);
            return Ok(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto com ID: {Id}", id);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um produto por código de barras
    /// </summary>
    /// <param name="codigoBarra">Código de barras do produto</param>
    /// <returns>Dados do produto</returns>
    /// <response code="200">Produto encontrado</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("codigo-barras/{codigoBarra}")]
    [ProducesResponseType(typeof(ProdutoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ProdutoDto>> GetProdutoPorCodigoBarra(string codigoBarra)
    {
        try
        {
            _logger.LogInformation("Buscando produto com código de barras: {CodigoBarra}", codigoBarra);

            var produto = await _produtoService.GetProdutoPorCodigoBarraAsync(codigoBarra);

            if (produto == null)
            {
                _logger.LogWarning("Produto com código de barras {CodigoBarra} não encontrado", codigoBarra);
                return NotFound($"Produto com código de barras {codigoBarra} não encontrado");
            }

            _logger.LogInformation("Produto encontrado: {Descricao}", produto.Descricao);
            return Ok(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto com código de barras: {CodigoBarra}", codigoBarra);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém um produto por código interno
    /// </summary>
    /// <param name="codigoInterno">Código interno do produto</param>
    /// <returns>Dados do produto</returns>
    /// <response code="200">Produto encontrado</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("codigo-interno/{codigoInterno}")]
    [ProducesResponseType(typeof(ProdutoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ProdutoDto>> GetProdutoPorCodigoInterno(string codigoInterno)
    {
        try
        {
            _logger.LogInformation("Buscando produto com código interno: {CodigoInterno}", codigoInterno);

            var produto = await _produtoService.GetProdutoPorCodigoInternoAsync(codigoInterno);

            if (produto == null)
            {
                _logger.LogWarning("Produto com código interno {CodigoInterno} não encontrado", codigoInterno);
                return NotFound($"Produto com código interno {codigoInterno} não encontrado");
            }

            _logger.LogInformation("Produto encontrado: {Descricao}", produto.Descricao);
            return Ok(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto com código interno: {CodigoInterno}", codigoInterno);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Busca produtos por grupo (ideal para comanda eletrônica)
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <param name="ativo">Filtrar apenas produtos ativos (padrão: true)</param>
    /// <param name="pagina">Página para paginação (padrão: 1)</param>
    /// <param name="itensPorPagina">Itens por página (padrão: 50)</param>
    /// <returns>Lista de produtos do grupo</returns>
    /// <response code="200">Produtos encontrados</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("grupo/{grupoId}")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosPorGrupo(int grupoId, [FromQuery] bool ativo = true, [FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 50)
    {
        try
        {
            _logger.LogInformation("Buscando produtos do grupo: {GrupoId} - Página: {Pagina}", grupoId, pagina);

            var request = new BuscarProdutoRequest
            {
                Grupo = grupoId,
                Ativo = ativo,
                Pagina = pagina,
                ItensPorPagina = itensPorPagina
            };

            var produtos = await _produtoService.BuscarProdutosAsync(request);

            _logger.LogInformation("Encontrados {Count} produtos no grupo {GrupoId}", produtos.Count(), grupoId);

            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos do grupo: {GrupoId}", grupoId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Busca produtos por grupo com paginação completa (ideal para comanda eletrônica)
    /// </summary>
    /// <param name="grupoId">ID do grupo</param>
    /// <param name="ativo">Filtrar apenas produtos ativos (padrão: true)</param>
    /// <param name="pagina">Página para paginação (padrão: 1)</param>
    /// <param name="itensPorPagina">Itens por página (padrão: 50)</param>
    /// <returns>Lista paginada de produtos do grupo</returns>
    /// <response code="200">Produtos encontrados</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("grupo/{grupoId}/paginado")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> GetProdutosPorGrupoPaginado(int grupoId, [FromQuery] bool ativo = true, [FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 50)
    {
        try
        {
            _logger.LogInformation("Buscando produtos paginados do grupo: {GrupoId} - Página: {Pagina}", grupoId, pagina);

            var request = new BuscarProdutoRequest
            {
                Grupo = grupoId,
                Ativo = ativo,
                Pagina = pagina,
                ItensPorPagina = itensPorPagina
            };

            var (produtos, total) = await _produtoService.BuscarProdutosComPaginacaoAsync(request);

            var resultado = new
            {
                Produtos = produtos,
                Total = total,
                Pagina = pagina,
                ItensPorPagina = itensPorPagina,
                TotalPaginas = (int)Math.Ceiling((double)total / itensPorPagina),
                GrupoId = grupoId
            };

            _logger.LogInformation("Encontrados {Count} produtos de {Total} total no grupo {GrupoId}", produtos.Count(), total, grupoId);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos paginados do grupo: {GrupoId}", grupoId);
            return StatusCode(500, "Erro interno do servidor");
        }
    }
}
