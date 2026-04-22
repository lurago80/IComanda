using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IPedidoFVService
{
    Task<IEnumerable<PedidoFVDto>> BuscarAsync(BuscarPedidoFVRequest request);
    Task<PedidoFVDto?> GetByIdAsync(int id);
    Task<IEnumerable<PedidoFVDto>> GetByVendedorAsync(int idVendedor, int? status = null);
    Task<IEnumerable<PedidoFVDto>> GetPendentesAsync();
    Task<PedidoFVDto> CriarAsync(CriarPedidoFVRequest request);
    Task<PedidoFVDto> AtualizarStatusAsync(int id, AtualizarStatusPedidoFVRequest request);
}
