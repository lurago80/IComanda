using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IMetaFVRepository
{
    Task<IEnumerable<MetaFV>> GetByVendedorAsync(int idVendedor);
    Task<MetaFV?> GetAsync(int idVendedor, int mes, int ano);
    Task<IEnumerable<MetaFV>> GetMesAsync(int mes, int ano);
    Task<int> CriarOuAtualizarAsync(MetaFV meta);
    Task<bool> AtualizarRealizadoAsync(int idVendedor, int mes, int ano, decimal valorRealizado);
}
