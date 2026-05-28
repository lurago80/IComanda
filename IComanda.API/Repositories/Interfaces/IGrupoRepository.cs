using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IGrupoRepository
{
    Task<IEnumerable<Grupo>> GetAllGruposAsync();
    Task<Grupo?> GetGrupoAsync(int id);
    Task<IEnumerable<Grupo>> GetGruposComQuantidadeAsync();
    Task<IEnumerable<Grupo>> GetGruposComQuantidadeTodosAsync();
    Task<int> CriarGrupoAsync(string descricao, bool imprimirDuasVias = false, decimal percentual = 0);
    Task<bool> AtualizarGrupoAsync(int id, string descricao, bool imprimirDuasVias = false, decimal percentual = 0);
    Task<bool> ExcluirGrupoAsync(int id);
}
