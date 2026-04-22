using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Enums;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class PedidoFVService : IPedidoFVService
{
    private readonly IPedidoFVRepository  _pedidoRepo;
    private readonly IProdutoRepository   _produtoRepo;
    private readonly IVendedorRepository  _vendedorRepo;
    private readonly IClienteRepository   _clienteRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<PedidoFVService> _logger;

    public PedidoFVService(
        IPedidoFVRepository  pedidoRepo,
        IProdutoRepository   produtoRepo,
        IVendedorRepository  vendedorRepo,
        IClienteRepository   clienteRepo,
        IMapper mapper,
        ILogger<PedidoFVService> logger)
    {
        _pedidoRepo   = pedidoRepo;
        _produtoRepo  = produtoRepo;
        _vendedorRepo = vendedorRepo;
        _clienteRepo  = clienteRepo;
        _mapper       = mapper;
        _logger       = logger;
    }

    public async Task<IEnumerable<PedidoFVDto>> BuscarAsync(BuscarPedidoFVRequest request)
    {
        var pedidos = await _pedidoRepo.BuscarAsync(request);
        return _mapper.Map<IEnumerable<PedidoFVDto>>(pedidos);
    }

    public async Task<PedidoFVDto?> GetByIdAsync(int id)
    {
        var pedido = await _pedidoRepo.GetByIdAsync(id);
        return pedido != null ? _mapper.Map<PedidoFVDto>(pedido) : null;
    }

    public async Task<IEnumerable<PedidoFVDto>> GetByVendedorAsync(int idVendedor, int? status = null)
    {
        var pedidos = await _pedidoRepo.GetByVendedorAsync(idVendedor, status);
        return _mapper.Map<IEnumerable<PedidoFVDto>>(pedidos);
    }

    public async Task<IEnumerable<PedidoFVDto>> GetPendentesAsync()
    {
        var pedidos = await _pedidoRepo.GetPendenteAsync();
        return _mapper.Map<IEnumerable<PedidoFVDto>>(pedidos);
    }

    public async Task<PedidoFVDto> CriarAsync(CriarPedidoFVRequest request)
    {
        // Validações
        var vendedor = await _vendedorRepo.GetByIdAsync(request.IdVendedor)
            ?? throw new InvalidOperationException($"Vendedor {request.IdVendedor} não encontrado");

        var cliente = await _clienteRepo.GetByIdAsync(request.IdCliente)
            ?? throw new InvalidOperationException($"Cliente {request.IdCliente} não encontrado");

        // Montar itens
        var itens = new List<ItemPedidoFV>();
        decimal subtotal = 0;

        foreach (var itemReq in request.Itens)
        {
            var produto = await _produtoRepo.GetProdutoAsync(itemReq.IdProduto)
                ?? throw new InvalidOperationException($"Produto {itemReq.IdProduto} não encontrado");

            var total = itemReq.Quantidade * itemReq.PrecoUnitario * (1 - (itemReq.Desconto / 100));
            subtotal += total;

            itens.Add(new ItemPedidoFV
            {
                IdProduto        = produto.Id,
                CodigoProduto    = produto.CodigoInterno ?? produto.CodigoBarra ?? produto.Id.ToString(),
                DescricaoProduto = produto.Descricao ?? "",
                Quantidade       = itemReq.Quantidade,
                Unidade          = produto.UnMedida ?? "UN",
                PrecoUnitario    = itemReq.PrecoUnitario,
                Desconto         = itemReq.Desconto,
                Total            = Math.Round(total, 2),
                Obs              = itemReq.Obs
            });
        }

        var total_ = subtotal - request.Desconto + request.Acrescimo;

        var pedido = new PedidoFV
        {
            IdVendedor   = request.IdVendedor,
            IdCliente    = request.IdCliente,
            DataPedido   = DateTime.Now,
            Status       = StatusPedidoFV.Pendente,
            Subtotal     = Math.Round(subtotal, 2),
            Desconto     = request.Desconto,
            Acrescimo    = request.Acrescimo,
            Total        = Math.Round(total_, 2),
            Obs          = request.Obs,
            CondicaoPgto = request.CondicaoPgto,
            TabelaPreco  = request.TabelaPreco
        };

        var id = await _pedidoRepo.CriarAsync(pedido, itens);
        _logger.LogInformation("Pedido FV #{Id} criado para cliente '{Cliente}' por vendedor '{Vendedor}'",
            id, cliente.Nome, vendedor.Nome);

        return (await GetByIdAsync(id))!;
    }

    public async Task<PedidoFVDto> AtualizarStatusAsync(int id, AtualizarStatusPedidoFVRequest request)
    {
        var pedido = await _pedidoRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Pedido FV {id} não encontrado");

        // Regras de negócio de transição de status
        var statusAtual  = (int)pedido.Status;
        var statusNovo   = request.Status;

        if (statusNovo == (int)StatusPedidoFV.Cancelado && string.IsNullOrWhiteSpace(request.Motivo))
            throw new InvalidOperationException("Motivo é obrigatório ao cancelar um pedido");

        if (statusNovo == (int)StatusPedidoFV.Faturado && string.IsNullOrWhiteSpace(request.NotaFiscal))
            throw new InvalidOperationException("Número da nota fiscal é obrigatório ao faturar");

        // Já está no status final, não pode regredir
        if (statusAtual == (int)StatusPedidoFV.Faturado || statusAtual == (int)StatusPedidoFV.Cancelado)
            throw new InvalidOperationException($"Pedido já está no status '{pedido.Status}' e não pode ser alterado");

        var ok = await _pedidoRepo.AtualizarStatusAsync(id, statusNovo, request.IdAprovador, request.Motivo, request.NotaFiscal);
        if (!ok) throw new InvalidOperationException("Falha ao atualizar o status do pedido");

        _logger.LogInformation("Pedido FV #{Id} alterado de {Antes} para {Depois}", id, pedido.Status, (StatusPedidoFV)statusNovo);
        return (await GetByIdAsync(id))!;
    }
}
