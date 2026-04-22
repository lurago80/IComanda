using IComanda.API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IComanda.API.Repositories.Interfaces
{
    public interface IOperadorComissaoRepository
    {
        Task<IEnumerable<OperadorComissao>> GetAllAsync();
        Task<OperadorComissao?> GetByIdAsync(int id);
        Task<IEnumerable<OperadorComissao>> GetByOperadorAsync(int operadorId);
        Task CreateAsync(OperadorComissao operadorComissao);
        Task UpdateAsync(OperadorComissao operadorComissao);
        Task DeleteAsync(int id);
        Task<decimal> CalcularComissaoAsync(int operadorId, DateTime dataInicio, DateTime dataFim, decimal percentual);
    }
}
