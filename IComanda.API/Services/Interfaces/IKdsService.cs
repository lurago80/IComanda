using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface IKdsService
{
    Task<IEnumerable<KdsPedidoDto>> GetPedidosAtivosAsync();
    Task<bool> AtualizarStatusAsync(string nota, string statusCozinha);
}
