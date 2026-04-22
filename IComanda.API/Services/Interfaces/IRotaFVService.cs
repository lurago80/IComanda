using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IRotaFVService
{
    Task<IEnumerable<VisitaFVDto>> GetVisitasVendedorAsync(int idVendedor, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<VisitaFVDto>> GetAgendaHojeAsync(int idVendedor);
    Task<VisitaFVDto?> GetByIdAsync(int id);
    Task<VisitaFVDto> AgendarVisitaAsync(AgendarVisitaFVRequest request);
    Task<VisitaFVDto> CheckinAsync(int id, CheckinVisitaFVRequest request);
    Task<VisitaFVDto> ConcluirAsync(int id, ConcluirVisitaFVRequest request);
    Task<VisitaFVDto> MarcarNaoRealizadaAsync(int id, string? obs);
}
