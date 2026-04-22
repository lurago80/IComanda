using IComanda.API.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IComanda.API.Repositories.Interfaces
{
    public interface IGrupoProdutoRepository
    {
        Task<IEnumerable<GrupoProduto>> GetAllAsync();
        Task<GrupoProduto?> GetByIdAsync(int id);
        Task CreateAsync(GrupoProduto grupoProduto);
        Task UpdateAsync(GrupoProduto grupoProduto);
        Task DeleteAsync(int id);
    }
}
