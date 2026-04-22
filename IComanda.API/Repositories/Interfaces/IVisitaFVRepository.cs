using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IVisitaFVRepository
{
    Task<IEnumerable<VisitaFV>> GetByVendedorAsync(int idVendedor, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<VisitaFV>> GetHojeAsync(int idVendedor);
    Task<VisitaFV?> GetByIdAsync(int id);
    Task<int> AgendarAsync(VisitaFV visita);
    Task<bool> CheckinAsync(int id, DateTime dataHora, decimal? lat, decimal? lng);
    Task<bool> ConcluirAsync(int id, DateTime dataHora, decimal? lat, decimal? lng, string? resultado, string? obs, int? idPedidoFV);
    Task<bool> MarcarNaoRealizadaAsync(int id, string? obs);
}
