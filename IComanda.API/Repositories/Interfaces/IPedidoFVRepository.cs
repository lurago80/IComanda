using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;

namespace IComanda.API.Repositories.Interfaces;

public interface IPedidoFVRepository
{
    Task<IEnumerable<PedidoFV>> BuscarAsync(BuscarPedidoFVRequest request);
    Task<PedidoFV?> GetByIdAsync(int id);
    Task<IEnumerable<PedidoFV>> GetByVendedorAsync(int idVendedor, int? status = null);
    Task<IEnumerable<PedidoFV>> GetPendenteAsync();
    Task<int> CriarAsync(PedidoFV pedido, IEnumerable<ItemPedidoFV> itens);
    Task<bool> AtualizarStatusAsync(int id, int status, int? idAprovador, string? motivo, string? notaFiscal);
    Task<int> ContarAsync(BuscarPedidoFVRequest request);
}
