using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IGrupoRepository
{
    Task<IEnumerable<Grupo>> GetAllGruposAsync();
    Task<Grupo?> GetGrupoAsync(int id);
    Task<IEnumerable<Grupo>> GetGruposComQuantidadeAsync();
    Task<IEnumerable<Grupo>> GetGruposComQuantidadeTodosAsync();
}
