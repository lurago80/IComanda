using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Controllers;

/// <summary>
/// Endpoints públicos para o cardápio digital acessado via QR Code.
/// Não requerem autenticação JWT — acessíveis por qualquer dispositivo na rede.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CardapioController : ControllerBase
{
    private readonly IGrupoRepository _grupoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IEmitenteRepository _emitenteRepository;
    private readonly IPizzaRepository _pizzaRepository;
    private readonly IVendaRepository _vendaRepository;
    private readonly ILogger<CardapioController> _logger;

    public CardapioController(
        IGrupoRepository grupoRepository,
        IProdutoRepository produtoRepository,
        IMesaRepository mesaRepository,
        IEmitenteRepository emitenteRepository,
        IPizzaRepository pizzaRepository,
        IVendaRepository vendaRepository,
        ILogger<CardapioController> logger)
    {
        _grupoRepository = grupoRepository;
        _produtoRepository = produtoRepository;
        _mesaRepository = mesaRepository;
        _emitenteRepository = emitenteRepository;
        _pizzaRepository = pizzaRepository;
        _vendaRepository = vendaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retorna informações do estabelecimento para exibição no topo do cardápio.
    /// </summary>
    [HttpGet("estabelecimento")]
    public async Task<IActionResult> GetEstabelecimento()
    {
        try
        {
            var emitente = await _emitenteRepository.GetEmitenteAsync();
            if (emitente == null) return Ok(new { nome = "Cardápio Digital", telefone = (string?)null });

            return Ok(new
            {
                nome = emitente.NomeFantasia ?? emitente.Nome ?? "Cardápio Digital",
                telefone = emitente.Telefone,
                cidade = emitente.Cidade,
                uf = emitente.Uf
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cardápio: erro ao buscar estabelecimento");
            return Ok(new { nome = "Cardápio Digital", telefone = (string?)null });
        }
    }

    /// <summary>
    /// Retorna todos os grupos/categorias ativos, incluindo o tipo (NORMAL ou PIZZA).
    /// </summary>
    [HttpGet("grupos")]
    public async Task<IActionResult> GetGrupos()
    {
        try
        {
            var grupos = await _grupoRepository.GetAllGruposAsync();
            return Ok(grupos.Select(g => new { g.Id, g.Descricao, tipo = g.Tipo ?? "NORMAL" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cardápio: erro ao buscar grupos");
            return StatusCode(500, new { mensagem = "Erro ao carregar categorias" });
        }
    }

    /// <summary>
    /// Retorna os tamanhos, sabores e bordas de um grupo de pizza — para o cardápio visual.
    /// </summary>
    [HttpGet("grupos/{grupoId}/pizza")]
    public async Task<IActionResult> GetPizzaDoGrupo(int grupoId)
    {
        try
        {
            var tamanhos = await _pizzaRepository.GetTamanhosPorGrupoAsync(grupoId, comSabores: true);
            var bordas = await _pizzaRepository.GetBordasAsync(apenasAtivas: true);
            return Ok(new { tamanhos, bordas });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cardápio: erro ao buscar pizza do grupo {GrupoId}", grupoId);
            return StatusCode(500, new { mensagem = "Erro ao carregar opções de pizza" });
        }
    }

    /// <summary>
    /// Retorna os produtos ativos de um grupo/categoria específico.
    /// </summary>
    [HttpGet("grupos/{grupoId}/produtos")]
    public async Task<IActionResult> GetProdutosPorGrupo(int grupoId)
    {
        try
        {
            var produtos = await _produtoRepository.BuscarProdutosAsync(null, ativo: true, grupo: grupoId, pagina: 1, itensPorPagina: 200);
            return Ok(produtos.Select(p => new
            {
                p.Id,
                p.Descricao,
                p.Caracteristica,
                preco = p.PrecoVenda ?? 0m
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cardápio: erro ao buscar produtos do grupo {GrupoId}", grupoId);
            return StatusCode(500, new { mensagem = "Erro ao carregar produtos" });
        }
    }

    /// <summary>
    /// Valida que uma mesa existe e retorna seu número para exibição no cardápio do cliente.
    /// </summary>
    [HttpGet("mesa/{numero}")]
    public async Task<IActionResult> GetMesa(int numero)
    {
        try
        {
            var mesa = await _mesaRepository.GetMesaPorNumeroAsync(numero);
            if (mesa == null)
                return NotFound(new { mensagem = $"Mesa {numero} não encontrada" });

            return Ok(new { numero = mesa.Numero });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cardápio: erro ao buscar mesa {Numero}", numero);
            return Ok(new { numero });
        }
    }

    /// <summary>
    /// Recebe o pedido do cliente via QR Code e grava como comanda aberta no sistema.
    /// </summary>
    [HttpPost("pedido")]
    public async Task<IActionResult> EnviarPedido([FromBody] PedidoCardapioRequest request)
    {
        try
        {
            if (request.Itens == null || request.Itens.Count == 0)
                return BadRequest(new { mensagem = "O pedido não contém itens." });

            if (request.Mesa <= 0)
                return BadRequest(new { mensagem = "Informe o número da mesa." });

            var agora = DateTime.Now;
            var nota = await _vendaRepository.GerarProximaNotaAsync();
            var numComanda = await _vendaRepository.GerarProximoNumeroComandaAsync();
            var totalPedido = request.Itens.Sum(i => i.Preco * i.Quantidade);

            var venda = new Venda
            {
                Nota = nota,
                Origem = "QR",
                Emissao = agora,
                Hora = agora.TimeOfDay,
                DataSaida = agora,
                HoraSaida = agora.TimeOfDay,
                Saida = 'X',
                Mesa = request.Mesa,
                Comanda = numComanda,
                TotProdutos = totalPedido,
                Total = totalPedido,
                Lancado = "ABERTO",
                NomeCliente = string.IsNullOrWhiteSpace(request.NomeCliente) ? $"Mesa {request.Mesa}" : request.NomeCliente,
                FormasPgto = "PENDENTE",
                Cfops = "5.102",
                Natureza = "Venda de Mercadoria",
                Modelo = "D2",
                Serie = "001",
                Subserie = "01",
            };

            var observacaoGeral = request.Observacao ?? "";
            var itens = request.Itens.Select((item, idx) => new ItemVenda
            {
                Nota = nota,
                Item = idx + 1,
                Codigo = item.ProdutoId > 0 ? item.ProdutoId : 0,
                // SERIAL é usado como fallback de descrição no KDS para itens sem produto cadastrado (ex: pizza montada)
                Serial = item.Descricao,
                Qtd = item.Quantidade,
                Preco = item.Preco,
                Total = item.Preco * item.Quantidade,
                Und = "UN",
                Modelo = "D2",
                Serie = "001",
                Subserie = "01",
                Origem = "QR",
                Emissao = agora,
                Cancelado = "0",
            }).ToList();

            await _vendaRepository.CriarVendaAsync(venda);
            await _vendaRepository.CriarItensVendaAsync(itens);

            _logger.LogInformation("Cardápio QR: pedido criado — nota {Nota}, mesa {Mesa}, {Itens} item(ns), total {Total}",
                nota, request.Mesa, itens.Count, totalPedido);

            return Ok(new PedidoCardapioResponse
            {
                Nota = nota,
                Mensagem = "Pedido enviado com sucesso! Em breve será preparado."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cardápio: erro ao gravar pedido da mesa {Mesa}", request.Mesa);
            return StatusCode(500, new { mensagem = "Erro ao enviar pedido. Tente novamente." });
        }
    }
}
