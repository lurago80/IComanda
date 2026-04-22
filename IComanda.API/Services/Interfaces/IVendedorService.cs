using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IVendedorService
{
    Task<IEnumerable<VendedorDto>> BuscarAsync(BuscarVendedorRequest request);
    Task<VendedorDto?> GetByIdAsync(int id);
    Task<IEnumerable<VendedorDto>> GetAtivosAsync();
    Task<VendedorDto> CriarAsync(VendedorDto dto);
    Task<VendedorDto> AtualizarAsync(int id, VendedorDto dto);
    Task AlterarStatusAsync(int id, bool ativo);
    Task AlterarSenhaAsync(int id, string novaSenha);
}
