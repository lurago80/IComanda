using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class RotaFVService : IRotaFVService
{
    private readonly IVisitaFVRepository _visitaRepo;
    private readonly IVendedorRepository _vendedorRepo;
    private readonly IClienteRepository  _clienteRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<RotaFVService> _logger;

    public RotaFVService(
        IVisitaFVRepository visitaRepo,
        IVendedorRepository vendedorRepo,
        IClienteRepository  clienteRepo,
        IMapper mapper,
        ILogger<RotaFVService> logger)
    {
        _visitaRepo   = visitaRepo;
        _vendedorRepo = vendedorRepo;
        _clienteRepo  = clienteRepo;
        _mapper       = mapper;
        _logger       = logger;
    }

    public async Task<IEnumerable<VisitaFVDto>> GetVisitasVendedorAsync(int idVendedor, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var visitas = await _visitaRepo.GetByVendedorAsync(idVendedor, dataInicio, dataFim);
        return _mapper.Map<IEnumerable<VisitaFVDto>>(visitas);
    }

    public async Task<IEnumerable<VisitaFVDto>> GetAgendaHojeAsync(int idVendedor)
    {
        var visitas = await _visitaRepo.GetHojeAsync(idVendedor);
        return _mapper.Map<IEnumerable<VisitaFVDto>>(visitas);
    }

    public async Task<VisitaFVDto?> GetByIdAsync(int id)
    {
        var visita = await _visitaRepo.GetByIdAsync(id);
        return visita != null ? _mapper.Map<VisitaFVDto>(visita) : null;
    }

    public async Task<VisitaFVDto> AgendarVisitaAsync(AgendarVisitaFVRequest request)
    {
        _ = await _vendedorRepo.GetByIdAsync(request.IdVendedor)
            ?? throw new InvalidOperationException($"Vendedor {request.IdVendedor} não encontrado");

        _ = await _clienteRepo.GetByIdAsync(request.IdCliente)
            ?? throw new InvalidOperationException($"Cliente {request.IdCliente} não encontrado");

        var visita = new VisitaFV
        {
            IdVendedor   = request.IdVendedor,
            IdCliente    = request.IdCliente,
            DataAgendada = request.DataAgendada,
            Obs          = request.Obs
        };

        var id = await _visitaRepo.AgendarAsync(visita);
        _logger.LogInformation("Visita FV #{Id} agendada: Vendedor={V} Cliente={C} Data={D}",
            id, request.IdVendedor, request.IdCliente, request.DataAgendada);

        return (await GetByIdAsync(id))!;
    }

    public async Task<VisitaFVDto> CheckinAsync(int id, CheckinVisitaFVRequest request)
    {
        var visita = await _visitaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Visita FV {id} não encontrada");

        var ok = await _visitaRepo.CheckinAsync(id, DateTime.Now, request.Latitude, request.Longitude);
        if (!ok)
            throw new InvalidOperationException($"Não foi possível realizar check-in. Verifique se a visita está no status 'Agendada'");

        _logger.LogInformation("Check-in da Visita FV #{Id} realizado", id);
        return (await GetByIdAsync(id))!;
    }

    public async Task<VisitaFVDto> ConcluirAsync(int id, ConcluirVisitaFVRequest request)
    {
        _ = await _visitaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Visita FV {id} não encontrada");

        var ok = await _visitaRepo.ConcluirAsync(
            id, DateTime.Now,
            request.Latitude, request.Longitude,
            request.Resultado, request.Obs,
            request.IdPedidoFV);

        if (!ok)
            throw new InvalidOperationException("Não foi possível concluir a visita. Verifique se o check-in foi realizado");

        _logger.LogInformation("Visita FV #{Id} concluída com resultado '{Resultado}'", id, request.Resultado);
        return (await GetByIdAsync(id))!;
    }

    public async Task<VisitaFVDto> MarcarNaoRealizadaAsync(int id, string? obs)
    {
        _ = await _visitaRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Visita FV {id} não encontrada");

        var ok = await _visitaRepo.MarcarNaoRealizadaAsync(id, obs);
        if (!ok)
            throw new InvalidOperationException("Não foi possível marcar a visita como não realizada");

        return (await GetByIdAsync(id))!;
    }
}
