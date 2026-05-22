using IComanda.API.Models.DTOs;

namespace IComanda.API.Repositories.Interfaces;

public interface IKdsRepository
{
    Task<IEnumerable<KdsPedidoDto>> GetPedidosAtivosAsync();
    Task<bool> AtualizarStatusCozinhaAsync(string nota, string statusCozinha);
    Task<bool> EnsureStatusCozinhaColumnAsync();
}
