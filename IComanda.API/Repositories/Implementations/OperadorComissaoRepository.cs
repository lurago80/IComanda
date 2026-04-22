using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IComanda.API.Repositories.Implementations
{
    public class OperadorComissaoRepository : IOperadorComissaoRepository
    {
        public Task<IEnumerable<OperadorComissao>> GetAllAsync() => Task.FromResult<IEnumerable<OperadorComissao>>(new List<OperadorComissao>());
        public Task<OperadorComissao?> GetByIdAsync(int id) => Task.FromResult<OperadorComissao?>(null);
        public Task<IEnumerable<OperadorComissao>> GetByOperadorAsync(int operadorId) => Task.FromResult<IEnumerable<OperadorComissao>>(new List<OperadorComissao>());
        public Task CreateAsync(OperadorComissao operadorComissao) => Task.CompletedTask;
        public Task UpdateAsync(OperadorComissao operadorComissao) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
        public Task<decimal> CalcularComissaoAsync(int operadorId, DateTime dataInicio, DateTime dataFim, decimal percentual) => Task.FromResult(0m);
    }
}
