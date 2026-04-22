using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface IMesaService
{
    Task<IEnumerable<MesaDto>> GetMesasAsync();
    Task<MesaDto?> GetMesaPorNumeroAsync(int numero);
    Task<MesaDto> OcuparMesaAsync(int numero, int comanda, string nota, int operador, int? cliente = null, int? numeroPessoas = null);
    Task<MesaDto> LiberarMesaAsync(int numero);
    Task<IEnumerable<MesaDto>> GetMesasOcupadasAsync();
    Task<IEnumerable<MesaDto>> GetMesasLivresAsync();
}

