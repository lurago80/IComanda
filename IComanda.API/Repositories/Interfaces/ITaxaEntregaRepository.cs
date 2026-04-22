using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface ITaxaEntregaRepository
{
    Task<IEnumerable<TaxaEntrega>> GetAllAsync();
    Task<TaxaEntrega?> GetByIdAsync(int id);
    Task<int> CriarAsync(string descricao, decimal valor);
    Task<bool> AtualizarAsync(int id, string descricao, decimal valor);
    Task<bool> ExcluirAsync(int id);
}
