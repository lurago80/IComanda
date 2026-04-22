using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IImpressaoService
{
    Task<bool> ImprimirPedidoAsync(string nota, ImprimirPedidoRequest request);
}

