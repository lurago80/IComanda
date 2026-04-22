using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IMetaFVService
{
    Task<IEnumerable<MetaFVDto>> GetByVendedorAsync(int idVendedor);
    Task<MetaFVDto?> GetAsync(int idVendedor, int mes, int ano);
    Task<IEnumerable<MetaFVDto>> GetRankingMesAsync(int mes, int ano);
    Task<MetaFVDto> DefinirMetaAsync(DefinirMetaFVRequest request);
    Task<DashboardVendedorDto> GetDashboardAsync(int idVendedor);
}
