using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface IGrupoService
{
    Task<IEnumerable<GrupoDto>> GetAllGruposAsync();
    Task<GrupoDto?> GetGrupoAsync(int id);
    Task<IEnumerable<GrupoDto>> GetGruposComQuantidadeAsync();
    Task<IEnumerable<GrupoDto>> GetGruposComQuantidadeTodosAsync();
    Task<GrupoDto> CriarGrupoAsync(string descricao, bool imprimirDuasVias = false, decimal percentual = 0);
    Task<GrupoDto> AtualizarGrupoAsync(int id, string descricao, bool imprimirDuasVias = false, decimal percentual = 0);
    Task<bool> ExcluirGrupoAsync(int id);
}
