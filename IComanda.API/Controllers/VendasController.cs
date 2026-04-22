using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;
using IComanda.API.Models.Entities;
using IComanda.API.Services.Interfaces;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para gerenciamento de vendas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize] // Requer autenticação
public class VendasController : ControllerBase
{
    private readonly IVendaService _vendaService;
    private readonly IImpressaoService _impressaoService;
    private readonly IEmitenteRepository _emitenteRepository;
    private readonly IVendaRepository _vendaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IHistoricoService _historicoService;
    private readonly ILogger<VendasController> _logger;

    public VendasController(
        IVendaService vendaService,
        IImpressaoService impressaoService,
        IEmitenteRepository emitenteRepository,
        IVendaRepository vendaRepository,
        IUsuarioRepository usuarioRepository,
        IHistoricoService historicoService,
        ILogger<VendasController> logger)
    {
        _vendaService = vendaService;
        _impressaoService = impressaoService;
        _emitenteRepository = emitenteRepository;
        _vendaRepository = vendaRepository;
        _usuarioRepository = usuarioRepository;
        _historicoService = historicoService;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova venda
    /// </summary>
    /// <param name="request">Dados da venda</param>
    /// <returns>Venda criada</returns>
    /// <response code="201">Venda criada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(typeof(VendaDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<VendaDto>> CriarVenda([FromBody] CriarVendaRequest request)
    {
        try
        {
            _logger.LogInformation("Criando nova venda para cliente: {Cliente}", request.Cliente);

            var venda = await _vendaService.CriarVendaAsync(request);

            _logger.LogInformation("Venda criada com sucesso: {Nota}", venda.Nota);

            return CreatedAtAction(nameof(GetVenda), new { nota = venda.Nota }, venda);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tentativa de criar venda com comanda já aberta");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar venda");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma venda existente
    /// </summary>
    /// <param name="nota">Número da nota</param>
    /// <param name="request">Dados atualizados da venda</param>
    /// <returns>Venda atualizada</returns>
    /// <response code="200">Venda atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Venda não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{nota}")]
    [ProducesResponseType(typeof(VendaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<VendaDto>> AtualizarVenda(string nota, [FromBody] CriarVendaRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nota))
            {
                return BadRequest(new { mensagem = "Nota não pode ser nula ou vazia" });
            }

            _logger.LogInformation("🔄 [CONTROLLER] Recebida requisição PUT para atualizar venda: '{Nota}' (tamanho: {Tamanho})", 
                nota, nota.Length);
            _logger.LogInformation("📦 [CONTROLLER] Request body - Itens: {Count}, Cliente: {Cliente}, Operador: {Operador}", 
                request.Itens?.Count ?? 0, request.Cliente, request.Operador);

            var venda = await _vendaService.AtualizarVendaAsync(nota, request);

            _logger.LogInformation("✅ [CONTROLLER] Venda atualizada com sucesso: {Nota}", venda.Nota);

            return Ok(venda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [CONTROLLER] Erro ao atualizar venda: {Nota}", nota);
            _logger.LogError("📋 [CONTROLLER] Detalhes do erro: {Mensagem}", ex.Message);
            _logger.LogError("📋 [CONTROLLER] Stack trace: {StackTrace}", ex.StackTrace);
            
            if (ex.Message.Contains("não encontrada") || ex.Message.Contains("não encontrado") || ex.Message.Contains("não encontrada no banco"))
            {
                return NotFound(new { 
                    mensagem = ex.Message,
                    nota = nota,
                    detalhes = "Verifique se a venda existe e está com status ABERTO.",
                    sugestao = "Use o endpoint GET /api/vendas/{nota} para verificar se a venda existe"
                });
            }
            
            // Retornar mais detalhes do erro para debug
            var errorResponse = new
            {
                mensagem = "Erro interno do servidor",
                detalhes = ex.Message,
                nota = nota,
                tipoErro = ex.GetType().Name,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray() // Primeiras 5 linhas do stack trace
            };
            
            _logger.LogError("📋 [CONTROLLER] Response de erro: {Response}", System.Text.Json.JsonSerializer.Serialize(errorResponse));
            
            return StatusCode(500, errorResponse);
        }
    }

    /// <summary>
    /// Obtém uma venda por nota
    /// </summary>
    /// <param name="nota">Número da nota</param>
    /// <returns>Dados da venda</returns>
    /// <response code="200">Venda encontrada</response>
    /// <response code="404">Venda não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{nota}")]
    [ProducesResponseType(typeof(VendaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<VendaDto>> GetVenda(string nota)
    {
        try
        {
            _logger.LogInformation("Buscando venda: {Nota}", nota);

            var venda = await _vendaService.GetVendaAsync(nota);

            if (venda == null)
            {
                _logger.LogWarning("Venda {Nota} não encontrada", nota);
                return NotFound($"Venda {nota} não encontrada");
            }

            _logger.LogInformation("Venda encontrada: {Nota} - Total: {Total}", venda.Nota, venda.Total);
            return Ok(venda);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar venda: {Nota}", nota);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista vendas do dia atual
    /// </summary>
    /// <returns>Lista de vendas de hoje</returns>
    /// <response code="200">Vendas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("hoje")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasHoje()
    {
        try
        {
            _logger.LogInformation("Buscando vendas de hoje");

            var vendas = await _vendaService.GetVendasHojeAsync();

            _logger.LogInformation("Encontradas {Count} vendas hoje", vendas.Count());

            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas de hoje");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista vendas fechadas/recebidas no período (para reimpressão de recibos).
    /// </summary>
    /// <param name="dataInicio">Data início (opcional)</param>
    /// <param name="dataFim">Data fim (opcional)</param>
    /// <returns>Lista de vendas fechadas com recebimentos</returns>
    /// <response code="200">Vendas fechadas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("fechadas")]
    [ProducesResponseType(typeof(IEnumerable<VendaFechadaReciboDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaFechadaReciboDto>>> GetVendasFechadas([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim)
    {
        try
        {
            var vendas = await _vendaService.GetVendasFechadasPorPeriodoAsync(dataInicio, dataFim);
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas fechadas no período");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista vendas por comanda
    /// </summary>
    /// <param name="comanda">Número da comanda</param>
    /// <returns>Lista de vendas da comanda</returns>
    /// <response code="200">Vendas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("comanda/{comanda}")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorComanda(int comanda)
    {
        try
        {
            _logger.LogInformation("Buscando vendas da comanda: {Comanda}", comanda);

            var vendas = await _vendaService.GetVendasPorComandaAsync(comanda);

            _logger.LogInformation("Encontradas {Count} vendas na comanda {Comanda}", vendas.Count(), comanda);

            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas da comanda: {Comanda}", comanda);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista vendas por mesa
    /// </summary>
    /// <param name="mesa">Número da mesa</param>
    /// <returns>Lista de vendas da mesa</returns>
    /// <response code="200">Vendas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("mesa/{mesa}")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasPorMesa(int mesa)
    {
        try
        {
            _logger.LogInformation("Buscando vendas da mesa: {Mesa}", mesa);

            var vendas = await _vendaService.GetVendasPorMesaAsync(mesa);

            _logger.LogInformation("Encontradas {Count} vendas na mesa {Mesa}", vendas.Count(), mesa);

            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas da mesa: {Mesa}", mesa);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista todas as vendas em aberto
    /// </summary>
    /// <returns>Lista de vendas em aberto</returns>
    /// <response code="200">Vendas encontradas</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("abertas")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasAbertas([FromQuery] string? origem = null)
    {
        try
        {
            _logger.LogInformation("Buscando vendas em aberto (origem: {Origem})", origem ?? "BA");

            var vendas = await _vendaService.GetVendasAbertasAsync(origem);

            _logger.LogInformation("Encontradas {Count} vendas em aberto", vendas.Count());

            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas em aberto");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Lista comandas/vendas excluídas (canceladas) no período.
    /// </summary>
    [HttpGet("canceladas")]
    [ProducesResponseType(typeof(IEnumerable<VendaDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<VendaDto>>> GetVendasCanceladas(
        [FromQuery] string? origem = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        try
        {
            var vendas = await _vendaService.GetVendasCanceladasAsync(origem, dataInicio, dataFim);
            return Ok(vendas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vendas canceladas");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém itens de uma venda por nota
    /// </summary>
    /// <param name="nota">Número da nota</param>
    /// <returns>Lista de itens da venda</returns>
    /// <response code="200">Itens encontrados</response>
    /// <response code="404">Nenhum item encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{nota}/itens")]
    [ProducesResponseType(typeof(IEnumerable<ItemVendaDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ItemVendaDto>>> GetItensVenda(string nota)
    {
        try
        {
            _logger.LogInformation("Buscando itens da venda: {Nota}", nota);

            var itens = await _vendaRepository.GetItensVendaAsync(nota);
            var itensList = itens.ToList();

            if (!itensList.Any())
            {
                _logger.LogWarning("Nenhum item encontrado para a venda {Nota}", nota);
                return NotFound($"Nenhum item encontrado para a venda {nota}");
            }

            // Buscar descrições dos produtos via repositório
            var codigosProdutos = itensList.Select(i => i.Codigo).Distinct().ToList();
            var produtosDict = new Dictionary<int, string>();
            
            if (codigosProdutos.Any())
            {
                var produtoRepository = HttpContext.RequestServices.GetRequiredService<Repositories.Interfaces.IProdutoRepository>();
                foreach (var codigo in codigosProdutos)
                {
                    try
                    {
                        var produto = await produtoRepository.GetProdutoAsync(codigo);
                        if (produto != null && !string.IsNullOrEmpty(produto.Descricao))
                        {
                            produtosDict[codigo] = produto.Descricao;
                        }
                    }
                    catch
                    {
                        // Ignorar erros ao buscar produto
                    }
                }
            }

            var itensDto = itensList.Select(i => new ItemVendaDto
            {
                Nota = i.Nota,
                Item = i.Item,
                Codigo = i.Codigo,
                Barras = i.Barras,
                Descricao = produtosDict.GetValueOrDefault(i.Codigo) ?? $"Produto {i.Codigo}",
                Qtd = i.Qtd,
                Preco = i.Preco,
                Desconto = i.Desconto,
                Acrescimo = i.Acrescimo,
                Total = i.Total,
                Und = i.Und
            }).ToList();

            _logger.LogInformation("Encontrados {Count} itens para a venda {Nota}", itensDto.Count, nota);
            return Ok(itensDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar itens da venda: {Nota}", nota);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém itens temporários de uma venda por cupom (frente_tmpitvendas)
    /// </summary>
    /// <param name="cupom">Número do cupom/nota</param>
    /// <returns>Lista de itens temporários da venda</returns>
    /// <response code="200">Itens encontrados</response>
    /// <response code="404">Nenhum item encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("{cupom}/itens-temporarios")]
    [ProducesResponseType(typeof(IEnumerable<ItemVendaDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<ItemVendaDto>>> GetItensTemporariosVenda(string cupom)
    {
        try
        {
            _logger.LogInformation("Buscando itens temporários da venda (cupom): {Cupom}", cupom);

            var itemTemporarioRepository = HttpContext.RequestServices.GetRequiredService<Repositories.Interfaces.IItemVendaTemporarioRepository>();
            var itensTemporarios = await itemTemporarioRepository.GetItensPorCupomAsync(cupom);
            var itensList = itensTemporarios.ToList();

            if (!itensList.Any())
            {
                _logger.LogWarning("Nenhum item temporário encontrado para o cupom {Cupom}", cupom);
                return NotFound($"Nenhum item temporário encontrado para o cupom {cupom}");
            }

            // Buscar descrições dos produtos via repositório
            var codigosProdutos = itensList.Select(i => i.Codigo).Distinct().ToList();
            var produtosDict = new Dictionary<int, string>();
            
            if (codigosProdutos.Any())
            {
                var produtoRepository = HttpContext.RequestServices.GetRequiredService<Repositories.Interfaces.IProdutoRepository>();
                foreach (var codigo in codigosProdutos)
                {
                    try
                    {
                        var produto = await produtoRepository.GetProdutoAsync(codigo);
                        if (produto != null && !string.IsNullOrEmpty(produto.Descricao))
                        {
                            produtosDict[codigo] = produto.Descricao;
                        }
                    }
                    catch
                    {
                        // Ignorar erros ao buscar produto
                    }
                }
            }

            var itensDto = itensList.Select(i => new ItemVendaDto
            {
                Nota = i.Cupom,
                Item = i.Item,
                Codigo = i.Codigo,
                Barras = i.Barras ?? "",
                Descricao = produtosDict.GetValueOrDefault(i.Codigo) ?? i.Descricao ?? $"Produto {i.Codigo}",
                Qtd = i.Qtd,
                Preco = i.Preco,
                Desconto = i.Desconto,
                Acrescimo = i.Acrescimo,
                Total = i.Total,
                Und = i.Und ?? "UN",
                Emissao = i.Data.Add(i.Hora)
            }).ToList();

            _logger.LogInformation("Encontrados {Count} itens temporários para o cupom {Cupom}", itensDto.Count, cupom);
            return Ok(itensDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar itens temporários da venda: {Cupom}", cupom);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Obtém conferência de mesa (pré-conta) - Mostra itens e total para cliente
    /// </summary>
    /// <param name="mesa">Número da mesa</param>
    /// <returns>Conferência da mesa</returns>
    /// <response code="200">Conferência encontrada</response>
    /// <response code="404">Mesa não possui venda aberta</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("conferencia/mesa/{mesa}")]
    [ProducesResponseType(typeof(ConferenciaMesaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ConferenciaMesaDto>> GetConferenciaMesa(int mesa)
    {
        try
        {
            _logger.LogInformation("📋 Solicitando conferência da mesa: {Mesa}", mesa);

            var conferencia = await _vendaService.GetConferenciaMesaAsync(mesa);

            if (conferencia == null)
            {
                _logger.LogWarning("❌ Mesa {Mesa} não possui venda aberta", mesa);
                return NotFound(new { mensagem = $"Mesa {mesa} não possui venda aberta" });
            }

            _logger.LogInformation("✅ Conferência da mesa {Mesa}: {Total} itens, R$ {Valor}", 
                mesa, conferencia.TotalItens, conferencia.Total);

            return Ok(conferencia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conferência da mesa: {Mesa}", mesa);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Gera o próximo número de comanda disponível
    /// </summary>
    /// <returns>Próximo número de comanda</returns>
    /// <response code="200">Número gerado com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("comanda/proximo-numero")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<int>> GerarProximoNumeroComanda()
    {
        try
        {
            _logger.LogInformation("Gerando próximo número de comanda");

            var proximoNumero = await _vendaRepository.GerarProximoNumeroComandaAsync();

            _logger.LogInformation("Próximo número de comanda gerado: {Numero}", proximoNumero);

            return Ok(proximoNumero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar próximo número de comanda");
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Verifica se uma comanda está aberta
    /// </summary>
    /// <param name="comanda">Número da comanda</param>
    /// <returns>True se a comanda está aberta, false caso contrário</returns>
    /// <response code="200">Status da comanda</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("comanda/{comanda}/aberta")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> VerificarComandaAberta(int comanda)
    {
        try
        {
            _logger.LogInformation("Verificando se comanda {Comanda} está aberta", comanda);

            var estaAberta = await _vendaService.VerificarComandaAbertaAsync(comanda);

            return Ok(estaAberta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar se comanda {Comanda} está aberta", comanda);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém conferência de comanda (pré-conta) - Mostra itens e total para cliente
    /// </summary>
    /// <param name="comanda">Número da comanda</param>
    /// <returns>Conferência da comanda</returns>
    /// <response code="200">Conferência encontrada</response>
    /// <response code="404">Comanda não possui venda aberta</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("conferencia/comanda/{comanda}")]
    [ProducesResponseType(typeof(ConferenciaMesaDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ConferenciaMesaDto>> GetConferenciaComanda(int comanda)
    {
        try
        {
            _logger.LogInformation("📋 Solicitando conferência da comanda: {Comanda}", comanda);

            var conferencia = await _vendaService.GetConferenciaComandaAsync(comanda);

            if (conferencia == null)
            {
                _logger.LogWarning("❌ Comanda {Comanda} não possui venda aberta", comanda);
                return NotFound(new { mensagem = $"Comanda {comanda} não possui venda aberta" });
            }

            _logger.LogInformation("✅ Conferência da comanda {Comanda}: {Total} itens, R$ {Valor}", 
                comanda, conferencia.TotalItens, conferencia.Total);

            return Ok(conferencia);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar conferência da comanda: {Comanda}", comanda);
            return StatusCode(500, new { mensagem = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Transfere um item de uma comanda para outra
    /// </summary>
    /// <param name="request">Dados da transferência</param>
    /// <returns>Venda de destino atualizada</returns>
    /// <response code="200">Item transferido com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Venda não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("transferir-item")]
    [ProducesResponseType(typeof(VendaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<VendaDto>> TransferirItem([FromBody] TransferirItemRequest request)
    {
        try
        {
            _logger.LogInformation("🔄 Transferindo item {Item} da venda {NotaOrigem} para {NotaDestino}", 
                request.ItemOrigem, request.NotaOrigem, request.NotaDestino);

            var venda = await _vendaService.TransferirItemAsync(request);

            _logger.LogInformation("✅ Item transferido com sucesso");

            return Ok(venda);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para transferência");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex) when (ex.Message.Contains("não encontrada") || ex.Message.Contains("não encontrado"))
        {
            _logger.LogWarning(ex, "Venda não encontrada para transferência");
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao transferir item");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Imprime um pedido (cupom térmico)
    /// </summary>
    /// <param name="nota">Número da nota do pedido</param>
    /// <param name="request">Dados do pedido para impressão</param>
    /// <returns>True se impresso com sucesso</returns>
    /// <response code="200">Impressão realizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("{nota}/imprimir")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> ImprimirPedido(string nota, [FromBody] ImprimirPedidoRequest request)
    {
        try
        {
            _logger.LogInformation("🖨️ Solicitando impressão do pedido {Nota} - Apenas novos itens: {ApenasNovosItens}", 
                nota, request.ApenasNovosItens);

            var sucesso = await _impressaoService.ImprimirPedidoAsync(nota, request);

            if (sucesso)
            {
                _logger.LogInformation("✅ Pedido {Nota} impresso com sucesso", nota);
                return Ok(true);
            }
            else
            {
                _logger.LogWarning("⚠️ Falha ao imprimir pedido {Nota}", nota);
                return StatusCode(500, new { mensagem = "Falha ao imprimir pedido. Verifique a configuração da impressora." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao imprimir pedido {Nota}", nota);
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Obtém os dados do emitente (endpoint alternativo)
    /// </summary>
    /// <returns>Dados do emitente</returns>
    /// <response code="200">Dados do emitente encontrados</response>
    /// <response code="404">Emitente não encontrado</response>
    [HttpGet("emitente")]
    [ProducesResponseType(typeof(Emitente), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Emitente>> GetEmitente()
    {
        try
        {
            _logger.LogInformation("🔍 [VendasController] Endpoint /api/vendas/emitente chamado");
            _logger.LogInformation("🔍 [VendasController] Buscando dados do emitente via VendasController");
            
            var emitente = await _emitenteRepository.GetEmitenteAsync();

            if (emitente == null)
            {
                _logger.LogWarning("⚠️ [VendasController] Emitente não encontrado - retornando 404");
                return NotFound(new { mensagem = "Emitente não encontrado" });
            }

            _logger.LogInformation("✅ [VendasController] Emitente encontrado: {Nome}", emitente.Nome);
            _logger.LogInformation("✅ [VendasController] Retornando emitente com ID: {Id}, Nome: {Nome}", emitente.Id, emitente.Nome);
            return Ok(emitente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [VendasController] Erro ao buscar emitente: {Message}", ex.Message);
            if (ex.InnerException != null)
            {
                _logger.LogError("   Inner Exception: {InnerMessage}", ex.InnerException.Message);
            }
            return StatusCode(500, new { mensagem = "Erro ao buscar emitente", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Cancela um item de uma venda aberta
    /// </summary>
    /// <param name="request">Dados do cancelamento</param>
    /// <returns>Venda atualizada</returns>
    /// <response code="200">Item cancelado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Venda ou item não encontrado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("cancelar-item")]
    [ProducesResponseType(typeof(VendaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<VendaDto>> CancelarItem([FromBody] CancelarItemRequest request)
    {
        try
        {
            _logger.LogInformation("🗑️ Cancelando item {Item} da venda {Nota}", request.Item, request.Nota);

            var venda = await _vendaService.CancelarItemAsync(request);

            _logger.LogInformation("✅ Item cancelado com sucesso");

            return Ok(venda);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para cancelamento");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao cancelar item");
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar item");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Cancela uma venda aberta inteira
    /// </summary>
    /// <param name="nota">Número da nota</param>
    /// <param name="operador">ID do operador</param>
    /// <returns>True se cancelada com sucesso</returns>
    /// <response code="200">Venda cancelada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="404">Venda não encontrada</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("{nota}/cancelar")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> CancelarVenda(string nota, [FromQuery] int operador = 1)
    {
        try
        {
            _logger.LogInformation("🗑️ Cancelando venda {Nota}", nota);

            if (string.IsNullOrWhiteSpace(nota))
            {
                return BadRequest(new { mensagem = "Nota não pode ser nula ou vazia" });
            }

            var cancelado = await _vendaService.CancelarVendaAsync(nota, operador);

            _logger.LogInformation("✅ Venda cancelada com sucesso");

            return Ok(cancelado);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para cancelamento");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao cancelar venda");
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar venda");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Exclui (cancela) uma comanda em aberto.
    /// Valida a senha contra o campo SENHAD da tabela PARAMETROS.
    /// Requer justificativa obrigatória. Registra no histórico.
    /// </summary>
    [HttpPost("{nota}/excluir-comanda")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<bool>> ExcluirComanda(string nota, [FromBody] ExcluirComandaRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nota))
                return BadRequest(new { mensagem = "Nota não pode ser nula ou vazia" });

            if (request == null || string.IsNullOrWhiteSpace(request.Senha))
                return BadRequest(new { mensagem = "Informe a senha de cancelamento." });

            if (string.IsNullOrWhiteSpace(request.Justificativa))
                return BadRequest(new { mensagem = "Informe a justificativa para o cancelamento." });

            // Buscar senha de cancelamento cadastrada nos parâmetros do sistema (campo SENHAD)
            var senhaCancelamento = await _vendaRepository.GetSenhaCancelamentoAsync();
            if (string.IsNullOrWhiteSpace(senhaCancelamento))
                return BadRequest(new { mensagem = "Senha de cancelamento não configurada nos parâmetros do sistema (campo SENHAD)." });

            if (string.Compare(senhaCancelamento, request.Senha.Trim(), StringComparison.Ordinal) != 0)
                return BadRequest(new { mensagem = "Senha de cancelamento incorreta." });

            // Identificar o operador logado via JWT
            var nomeOperador = User.Identity?.Name ?? "Sistema";
            var operadorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var operadorId = int.TryParse(operadorIdClaim, out var oid) ? oid : 1;

            // Detectar se é delivery para registrar corretamente no histórico
            var vendaParaVerificar = await _vendaService.GetVendaAsync(nota);
            var tipoVenda = vendaParaVerificar?.Origem?.ToUpper() == "DL" ? "Pedido Delivery" : "Comanda";

            var cancelado = await _vendaService.CancelarVendaAsync(nota, operadorId, request.Justificativa.Trim());

            var descricao = $"{tipoVenda} {nota.Trim()} cancelado(a) por {nomeOperador}. Justificativa: {request.Justificativa.Trim()}";
            await _historicoService.RegistrarAlteracaoAsync("VENDA", nota.Trim(), "EXCLUIR", operadorId, descricao, null, null, nomeOperador);

            _logger.LogInformation("✅ {TipoVenda} {Nota} excluído por {Operador}. Justificativa: {Justificativa}", tipoVenda, nota, nomeOperador, request.Justificativa);
            return Ok(cancelado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir comanda {Nota}", nota);
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Envia WhatsApp ao cliente informando que o pedido delivery saiu para entrega
    /// </summary>
    /// <param name="nota">Número da nota do pedido delivery</param>
    /// <response code="200">Mensagem enviada ou tentativa realizada</response>
    /// <response code="400">Nota inválida</response>
    /// <response code="404">Pedido não encontrado ou não é delivery</response>
    [HttpPost("delivery/{nota}/saiu-para-entrega")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<object>> NotificarSaiuParaEntrega(string nota)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nota))
            {
                return BadRequest(new { mensagem = "Nota não pode ser nula ou vazia" });
            }

            var enviado = await _vendaService.NotificarSaiuParaEntregaAsync(nota);

            return Ok(new { enviado, mensagem = enviado ? "Mensagem enviada ao cliente." : "Cliente sem telefone ou pedido não é delivery." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao notificar saiu para entrega do pedido {Nota}", nota);
            return StatusCode(500, new { mensagem = "Erro ao enviar notificação", detalhes = ex.Message });
        }
    }

    // ============================================================
    // 🚚 DELIVERY - ENDPOINTS ESPECÍFICOS
    // ============================================================

    /// <summary>
    /// Cria um novo pedido de delivery
    /// </summary>
    /// <param name="request">Dados do pedido de delivery</param>
    /// <returns>Pedido criado</returns>
    /// <response code="201">Pedido criado com sucesso</response>
    /// <response code="400">Dados inválidos (cliente obrigatório, comanda/mesa não permitidas)</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost("delivery")]
    [ProducesResponseType(typeof(PedidoDeliveryDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PedidoDeliveryDto>> CriarPedidoDelivery([FromBody] CriarPedidoDeliveryRequest request)
    {
        try
        {
            _logger.LogInformation("🚚 Criando novo pedido de delivery para cliente: {Cliente}", request.Cliente);

            var pedido = await _vendaService.CriarPedidoDeliveryAsync(request);

            _logger.LogInformation("✅ Pedido delivery criado com sucesso: {Nota}", pedido.Nota);

            return CreatedAtAction(nameof(GetVenda), new { nota = pedido.Nota }, pedido);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Erro: request null");
            return BadRequest(new { mensagem = "Request não pode ser nulo" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tentativa de criar pedido delivery com dados inválidos");
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido de delivery");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Lista todos os pedidos de delivery em aberto
    /// </summary>
    /// <returns>Lista de pedidos de delivery abertos</returns>
    /// <response code="200">Pedidos encontrados</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("delivery/abertos")]
    [ProducesResponseType(typeof(IEnumerable<PedidoDeliveryDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<PedidoDeliveryDto>>> GetPedidosDeliveryAbertos()
    {
        try
        {
            _logger.LogInformation("🚚 Listando pedidos de delivery em aberto");

            var pedidos = await _vendaService.GetPedidosDeliveryAbertosAsync();

            _logger.LogInformation("✅ Encontrados {Count} pedidos de delivery em aberto", pedidos.Count());

            return Ok(pedidos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar pedidos de delivery");
            return StatusCode(500, new { mensagem = "Erro interno do servidor", detalhes = ex.Message });
        }
    }}