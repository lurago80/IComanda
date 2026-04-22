using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;

namespace IComanda.API.Repositories.Interfaces;

public interface IVendedorRepository
{
    Task<IEnumerable<Vendedor>> BuscarVendedoresAsync(BuscarVendedorRequest request);
    Task<Vendedor?> GetByIdAsync(int id);
    Task<IEnumerable<Vendedor>> GetAtivosAsync();
    Task<int> CriarAsync(Vendedor vendedor);
    Task<bool> AtualizarAsync(Vendedor vendedor);
    Task<bool> AlterarStatusAsync(int id, bool ativo);
    Task<bool> AlterarSenhaAsync(int id, string senhaHash);
    Task<decimal> GetVendasMesAsync(int idVendedor, int mes, int ano);
}
