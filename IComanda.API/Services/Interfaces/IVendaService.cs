using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IVendaService
{
    Task<VendaDto> CriarVendaAsync(CriarVendaRequest request);
    Task<VendaDto?> GetVendaAsync(string nota);
    Task<IEnumerable<VendaDto>> GetVendasHojeAsync();
    Task<IEnumerable<VendaDto>> GetVendasPorComandaAsync(int comanda);
    Task<IEnumerable<VendaDto>> GetVendasPorMesaAsync(int mesa);
}
