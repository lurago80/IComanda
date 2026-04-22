using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface IComandaService
{
    /// <summary>
    /// Lista todas as comandas abertas
    /// </summary>
    Task<IEnumerable<ComandaAbertaDto>> GetComandasAbertasAsync();

    /// <summary>
    /// Verifica se uma comanda está aberta
    /// </summary>
    Task<bool> VerificarComandaAbertaAsync(int comanda);

    /// <summary>
    /// Obtém informações de uma comanda específica
    /// </summary>
    Task<ComandaAbertaDto?> GetComandaAsync(int comanda);
}
