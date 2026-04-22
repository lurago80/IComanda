using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class MetaFVService : IMetaFVService
{
    private readonly IMetaFVRepository   _metaRepo;
    private readonly IVendedorRepository _vendedorRepo;
    private readonly IPedidoFVRepository _pedidoRepo;
    private readonly IVisitaFVRepository _visitaRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<MetaFVService> _logger;

    public MetaFVService(
        IMetaFVRepository   metaRepo,
        IVendedorRepository vendedorRepo,
        IPedidoFVRepository pedidoRepo,
        IVisitaFVRepository visitaRepo,
        IMapper mapper,
        ILogger<MetaFVService> logger)
    {
        _metaRepo     = metaRepo;
        _vendedorRepo = vendedorRepo;
        _pedidoRepo   = pedidoRepo;
        _visitaRepo   = visitaRepo;
        _mapper       = mapper;
        _logger       = logger;
    }

    public async Task<IEnumerable<MetaFVDto>> GetByVendedorAsync(int idVendedor)
    {
        var metas = await _metaRepo.GetByVendedorAsync(idVendedor);
        return _mapper.Map<IEnumerable<MetaFVDto>>(metas);
    }

    public async Task<MetaFVDto?> GetAsync(int idVendedor, int mes, int ano)
    {
        var meta = await _metaRepo.GetAsync(idVendedor, mes, ano);
        return meta != null ? _mapper.Map<MetaFVDto>(meta) : null;
    }

    public async Task<IEnumerable<MetaFVDto>> GetRankingMesAsync(int mes, int ano)
    {
        var metas = await _metaRepo.GetMesAsync(mes, ano);
        var dtos  = _mapper.Map<IEnumerable<MetaFVDto>>(metas).ToList();

        // Enriquecer com valor realizado atualizado (vendas do mês)
        foreach (var dto in dtos)
        {
            var vendas = await _vendedorRepo.GetVendasMesAsync(dto.IdVendedor, mes, ano);
            dto.ValorRealizado   = vendas;
            dto.PercentualAtingido = dto.ValorMeta > 0 ? Math.Round((vendas / dto.ValorMeta) * 100, 2) : 0;
            dto.MetaAtingida      = vendas >= dto.ValorMeta;
        }

        return dtos.OrderByDescending(m => m.PercentualAtingido);
    }

    public async Task<MetaFVDto> DefinirMetaAsync(DefinirMetaFVRequest request)
    {
        _ = await _vendedorRepo.GetByIdAsync(request.IdVendedor)
            ?? throw new InvalidOperationException($"Vendedor {request.IdVendedor} não encontrado");

        var meta = new MetaFV
        {
            IdVendedor = request.IdVendedor,
            Mes        = request.Mes,
            Ano        = request.Ano,
            ValorMeta  = request.ValorMeta
        };

        var id = await _metaRepo.CriarOuAtualizarAsync(meta);
        meta.Id = id;

        _logger.LogInformation("Meta FV definida: Vendedor={V} {M}/{A} = {Meta:C}", request.IdVendedor, request.Mes, request.Ano, request.ValorMeta);
        return _mapper.Map<MetaFVDto>(meta);
    }

    public async Task<DashboardVendedorDto> GetDashboardAsync(int idVendedor)
    {
        var agora    = DateTime.Now;
        var vendedor = await _vendedorRepo.GetByIdAsync(idVendedor)
            ?? throw new KeyNotFoundException($"Vendedor {idVendedor} não encontrado");

        var vendasMes = await _vendedorRepo.GetVendasMesAsync(idVendedor, agora.Month, agora.Year);
        var metaMes   = await _metaRepo.GetAsync(idVendedor, agora.Month, agora.Year);

        var valorMeta         = metaMes?.ValorMeta ?? vendedor.MetaMensal;
        var percMeta          = valorMeta > 0 ? Math.Round((vendasMes / valorMeta) * 100, 2) : 0;
        var comissaoEstimada  = Math.Round(vendasMes * (vendedor.ComissaoPerc / 100), 2);

        // Pedidos FV do mês
        var todosPedidos = (await _pedidoRepo.GetByVendedorAsync(idVendedor)).ToList();
        var pedidosMes   = todosPedidos
            .Where(p => p.DataPedido.Month == agora.Month && p.DataPedido.Year == agora.Year)
            .ToList();

        var pendentes  = pedidosMes.Where(p => (int)p.Status == 0).ToList();
        var aprovados  = pedidosMes.Where(p => (int)p.Status == 1).ToList();
        var faturados  = pedidosMes.Where(p => (int)p.Status == 2).ToList();

        // Visitas de hoje
        var visitasHoje = (await _visitaRepo.GetHojeAsync(idVendedor)).ToList();

        // Próximas visitas (até 5 dias à frente)
        var visitasFuturas = (await _visitaRepo.GetByVendedorAsync(
            idVendedor,
            agora.Date,
            agora.Date.AddDays(7)))
            .Where(v => (int)v.Status < 2)
            .OrderBy(v => v.DataAgendada)
            .Take(5)
            .ToList();

        // Últimos 5 pedidos
        var ultimosPedidos = pedidosMes
            .OrderByDescending(p => p.DataPedido)
            .Take(5)
            .ToList();

        return new DashboardVendedorDto
        {
            IdVendedor              = idVendedor,
            NomeVendedor            = vendedor.Nome,
            MetaMes                 = valorMeta,
            VendasMes               = vendasMes,
            PercentualMeta          = percMeta,
            ComissaoEstimada        = comissaoEstimada,
            TotalPedidosPendentes   = pendentes.Count,
            TotalPedidosAprovados   = aprovados.Count,
            TotalPedidosFaturados   = faturados.Count,
            ValorPedidosPendentes   = pendentes.Sum(p => p.Total),
            ValorPedidosAprovados   = aprovados.Sum(p => p.Total),
            VisitasAgendadasHoje    = visitasHoje.Count,
            VisitasRealizadasHoje   = visitasHoje.Count(v => (int)v.Status == 2),
            ProximasVisitas         = _mapper.Map<List<VisitaFVDto>>(visitasFuturas),
            UltimosPedidos          = _mapper.Map<List<PedidoFVDto>>(ultimosPedidos)
        };
    }
}
