using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface ITaxaEntregaService
{
    Task<IEnumerable<TaxaEntregaDto>> GetAllAsync();
    Task<TaxaEntregaDto?> GetByIdAsync(int id);
    Task<TaxaEntregaDto> CriarAsync(string descricao, decimal valor);
    Task<TaxaEntregaDto?> AtualizarAsync(int id, string descricao, decimal valor);
    Task<bool> ExcluirAsync(int id);
}
