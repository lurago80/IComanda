using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface IGrupoService
{
    Task<IEnumerable<GrupoDto>> GetAllGruposAsync();
    Task<GrupoDto?> GetGrupoAsync(int id);
    Task<IEnumerable<GrupoDto>> GetGruposComQuantidadeAsync();
    Task<IEnumerable<GrupoDto>> GetGruposComQuantidadeTodosAsync();
}
