using Microsoft.AspNetCore.Authorization;
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
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly IGrupoService _grupoService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(
        IProdutoService produtoService, 
        IGrupoService grupoService,
        ILogger<ProdutosController> logger)
    {
        _produtoService = produtoService;
        _grupoService = grupoService;
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
            _logger.LogInformation("🔍 Buscando produtos - Termo: '{Termo}', Grupo: {Grupo}, Ativo: {Ativo}", 
                request.Q ?? "(vazio)", request.Grupo, request.Ativo);

            var produtos = await _produtoService.BuscarProdutosAsync(request);
            var produtosList = produtos.ToList();

            _logger.LogInformation("✅ Encontrados {Count} produtos", produtosList.Count);
            
            if (produtosList.Count > 0)
            {
                _logger.LogInformation("📦 Primeiros produtos: {Produtos}", 
                    string.Join(", ", produtosList.Take(3).Select(p => $"{p.Id}:{p.Descricao}")));
            }
            else
            {
                _logger.LogWarning("⚠️ Nenhum produto encontrado com os filtros aplicados");
            }

            return Ok(produtosList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar produtos");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Endpoint de teste para verificar se produtos estão sendo retornados
    /// </summary>
    [HttpGet("teste")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> TesteProdutos()
    {
        try
        {
            _logger.LogInformation("🧪 Executando teste de produtos...");
            
            // Teste 1: Buscar todos os produtos ativos sem filtros
            var produtosAtivos = await _produtoService.BuscarProdutosAsync(new BuscarProdutoRequest 
            { 
                Ativo = true,
                Pagina = 1,
                ItensPorPagina = 10
            });
            
            // Teste 2: Buscar todos os produtos (ativos e inativos)
            var todosProdutos = await _produtoService.BuscarProdutosAsync(new BuscarProdutoRequest 
            { 
                Ativo = null,
                Pagina = 1,
                ItensPorPagina = 10
            });
            
            // Teste 3: Buscar produtos completos (endpoint que estava falhando)
            var produtosCompletos = await _produtoService.BuscarProdutosCompletosAsync(null, true, 1, 10);
            
            return Ok(new
            {
                sucesso = true,
                produtosAtivos = produtosAtivos.Count(),
                todosProdutos = todosProdutos.Count(),
                produtosCompletos = produtosCompletos.Count(),
                amostraAtivos = produtosAtivos.Take(3).Select(p => new { p.Id, p.Descricao, p.Grupo, p.Ativo }),
                amostraTodos = todosProdutos.Take(3).Select(p => new { p.Id, p.Descricao, p.Grupo, p.Ativo }),
                amostraCompletos = produtosCompletos.Take(3).Select(p => new { p.Id, p.Descricao, p.Grupo, p.Ativo })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro no teste de produtos");
            return StatusCode(500, new { 
                sucesso = false,
                erro = ex.Message, 
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace 
            });
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
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosPorGrupo(int grupoId, [FromQuery] bool ativo = true, [FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 50)
    {
        try
        {
            _logger.LogInformation("🔍 [ProdutosController] Iniciando busca de produtos do grupo: {GrupoId} - Ativo: {Ativo} - Página: {Pagina}", grupoId, ativo, pagina);

            // Primeiro, verificar se o grupo existe
            _logger.LogInformation("🔍 [ProdutosController] Verificando se grupo {GrupoId} existe...", grupoId);
            var grupo = await _grupoService.GetGrupoAsync(grupoId);
            
            if (grupo == null)
            {
                _logger.LogWarning("⚠️ [ProdutosController] Grupo {GrupoId} não encontrado no banco de dados", grupoId);
                return NotFound(new { error = $"Grupo com ID {grupoId} não encontrado", grupoId });
            }

            _logger.LogInformation("✅ [ProdutosController] Grupo encontrado: ID={Id}, Descricao={Descricao}", grupo.Id, grupo.Descricao);

            // Agora buscar os produtos do grupo
            _logger.LogInformation("🔍 [ProdutosController] Buscando produtos do grupo {GrupoId}...", grupoId);
            var request = new BuscarProdutoRequest
            {
                Grupo = grupoId,
                Ativo = ativo,
                Pagina = pagina,
                ItensPorPagina = itensPorPagina
            };

            var produtos = await _produtoService.BuscarProdutosAsync(request);
            var produtosList = produtos.ToList();

            _logger.LogInformation("✅ [ProdutosController] Encontrados {Count} produtos no grupo {GrupoId} ({Descricao})", 
                produtosList.Count, grupoId, grupo.Descricao);
            
            if (produtosList.Count > 0)
            {
                _logger.LogInformation("📦 [ProdutosController] Primeiros produtos: {Produtos}", 
                    string.Join(", ", produtosList.Take(3).Select(p => $"{p.Id}:{p.Descricao}")));
            }
            else
            {
                _logger.LogWarning("⚠️ [ProdutosController] Nenhum produto encontrado para grupo {GrupoId} ({Descricao}) com ativo={Ativo}. Verifique se há produtos cadastrados neste grupo.", 
                    grupoId, grupo.Descricao, ativo);
            }

            return Ok(produtosList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ProdutosController] Erro ao buscar produtos do grupo: {GrupoId}. Mensagem: {Message}", 
                grupoId, ex.Message);
            _logger.LogError("❌ [ProdutosController] StackTrace completo: {StackTrace}", ex.StackTrace);
            
            if (ex.InnerException != null)
            {
                _logger.LogError("❌ [ProdutosController] InnerException: {Message}. StackTrace: {StackTrace}", 
                    ex.InnerException.Message, ex.InnerException.StackTrace);
            }
            
            // Retornar erro mais detalhado
            var errorMessage = $"Erro ao buscar produtos: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $" | Inner: {ex.InnerException.Message}";
            }
            
            return StatusCode(500, new { error = errorMessage, grupoId, ativo, stackTrace = ex.StackTrace });
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

    /// <summary>
    /// Obtém um produto completo por ID (com todas as informações das 4 tabelas)
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Dados completos do produto</returns>
    /// <response code="200">Produto encontrado</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{id}/completo")]
    [ProducesResponseType(typeof(ProdutoCompletoDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ProdutoCompletoDto>> GetProdutoCompleto(int id)
    {
        try
        {
            _logger.LogInformation("Buscando produto completo com ID: {Id}", id);

            var produto = await _produtoService.GetProdutoCompletoAsync(id);

            if (produto == null)
            {
                _logger.LogWarning("Produto completo com ID {Id} não encontrado", id);
                return NotFound($"Produto com ID {id} não encontrado");
            }

            return Ok(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto completo com ID: {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor", message = ex.Message });
        }
    }

    /// <summary>
    /// Busca produtos completos com filtros
    /// </summary>
    /// <param name="termo">Termo de busca (código, código de barras, código interno, descrição)</param>
    /// <param name="ativo">Filtrar apenas produtos ativos</param>
    /// <param name="pagina">Página para paginação</param>
    /// <param name="itensPorPagina">Itens por página</param>
    /// <returns>Lista de produtos completos</returns>
    [HttpGet("completos")]
    [ProducesResponseType(typeof(IEnumerable<ProdutoCompletoDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ProdutoCompletoDto>>> BuscarProdutosCompletos(
        [FromQuery] string? termo = null,
        [FromQuery] bool? ativo = null,
        [FromQuery] int pagina = 1,
        [FromQuery] int itensPorPagina = 50)
    {
        try
        {
            _logger.LogInformation("Buscando produtos completos - Termo: '{Termo}', Ativo: {Ativo}, Página: {Pagina}",
                termo ?? "(vazio)", ativo, pagina);

            var produtos = await _produtoService.BuscarProdutosCompletosAsync(termo, ativo, pagina, itensPorPagina);

            return Ok(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao buscar produtos completos - Mensagem: {Message}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                ex.Message, ex.InnerException?.Message, ex.StackTrace);
            return StatusCode(500, new { 
                error = "Erro interno do servidor", 
                message = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    /// <summary>
    /// Cria um novo produto
    /// </summary>
    /// <param name="request">Dados do produto a ser criado</param>
    /// <returns>ID do produto criado</returns>
    /// <response code="201">Produto criado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(int), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<int>> CriarProduto([FromBody] CriarProdutoRequest request)
    {
        try
        {
            _logger.LogInformation("Criando novo produto - Descrição: {Descricao}", request.Descricao);

            var id = await _produtoService.CriarProdutoAsync(request);

            _logger.LogInformation("Produto criado com sucesso - ID: {Id}", id);
            return CreatedAtAction(nameof(GetProdutoCompleto), new { id }, id);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Erro de validação ao criar produto: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao criar produto - Mensagem: {Message}, InnerException: {InnerException}, StackTrace: {StackTrace}", 
                ex.Message, ex.InnerException?.Message, ex.StackTrace);
            return StatusCode(500, new { error = "Erro interno do servidor", message = ex.Message, details = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Atualiza um produto existente
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <param name="request">Dados do produto a ser atualizado</param>
    /// <returns>Resultado da operação</returns>
    /// <response code="200">Produto atualizado com sucesso</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> AtualizarProduto(int id, [FromBody] AtualizarProdutoRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando produto - ID: {Id}", id);

            var sucesso = await _produtoService.AtualizarProdutoAsync(id, request);

            if (!sucesso)
            {
                return NotFound($"Produto com ID {id} não encontrado");
            }

            _logger.LogInformation("Produto atualizado com sucesso - ID: {Id}", id);
            return Ok(new { message = "Produto atualizado com sucesso", id });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Produto não encontrado: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produto - ID: {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor", message = ex.Message });
        }
    }

    /// <summary>
    /// Exclui um produto
    /// </summary>
    /// <param name="id">ID do produto</param>
    /// <returns>Resultado da operação</returns>
    /// <response code="200">Produto excluído com sucesso</response>
    /// <response code="404">Produto não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> ExcluirProduto(int id)
    {
        try
        {
            _logger.LogInformation("Excluindo produto - ID: {Id}", id);

            var sucesso = await _produtoService.ExcluirProdutoAsync(id);

            if (!sucesso)
            {
                return NotFound($"Produto com ID {id} não encontrado");
            }

            _logger.LogInformation("Produto excluído com sucesso - ID: {Id}", id);
            return Ok(new { message = "Produto excluído com sucesso", id });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Produto não encontrado: {Message}", ex.Message);
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Não é possível excluir produto {Id}: {Message}", id, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir produto - ID: {Id}", id);
            return StatusCode(500, new { error = "Erro interno do servidor", message = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint de diagnóstico - retorna informações sobre produtos no banco
    /// </summary>
    [HttpGet("diagnostico")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> Diagnostico()
    {
        try
        {
            _logger.LogInformation("🔍 Executando diagnóstico de produtos...");

            // Buscar TODOS os produtos (sem filtro de ativo)
            var requestTodos = new BuscarProdutoRequest
            {
                Ativo = null, // Sem filtro
                Pagina = 1,
                ItensPorPagina = 1000
            };

            var todosProdutos = await _produtoService.BuscarProdutosAsync(requestTodos);
            var todosList = todosProdutos.ToList();

            // Buscar produtos ATIVOS
            var requestAtivos = new BuscarProdutoRequest
            {
                Ativo = true,
                Pagina = 1,
                ItensPorPagina = 1000
            };

            var produtosAtivos = await _produtoService.BuscarProdutosAsync(requestAtivos);
            var ativosList = produtosAtivos.ToList();

            // Buscar produtos INATIVOS
            var requestInativos = new BuscarProdutoRequest
            {
                Ativo = false,
                Pagina = 1,
                ItensPorPagina = 1000
            };

            var produtosInativos = await _produtoService.BuscarProdutosAsync(requestInativos);
            var inativosList = produtosInativos.ToList();

            var resultado = new
            {
                totalProdutos = todosList.Count,
                produtosAtivos = ativosList.Count,
                produtosInativos = inativosList.Count,
                produtosSemGrupo = todosList.Count(p => p.Grupo == 0 || p.Grupo < 1),
                produtosComGrupo = todosList.Count(p => p.Grupo > 0),
                gruposComProdutos = todosList.Where(p => p.Grupo > 0).Select(p => p.Grupo).Distinct().Count(),
                primeirosProdutos = todosList.Take(10).Select(p => new { 
                    id = p.Id, 
                    descricao = p.Descricao, 
                    grupo = p.Grupo, 
                    ativo = p.Ativo 
                })
            };

            _logger.LogInformation("✅ Diagnóstico completo: {Resultado}", resultado);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao executar diagnóstico");
            return StatusCode(500, new { error = "Erro ao executar diagnóstico", message = ex.Message });
        }
    }
}
