using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IMesaRepository
{
    Task<IEnumerable<Mesa>> GetMesasAsync();
    Task<Mesa?> GetMesaPorNumeroAsync(int numero);
    Task<bool> AtualizarStatusMesaAsync(int numero, string status, int? comanda = null, string? nota = null, int? operador = null, int? cliente = null, int? numeroPessoas = null);
    Task<IEnumerable<Mesa>> GetMesasOcupadasAsync();
    Task<IEnumerable<Mesa>> GetMesasLivresAsync();
}

