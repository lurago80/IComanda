using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface ICaixaService
{
    Task<CaixaDto> AbrirCaixaAsync(AbrirCaixaRequest request);
    Task<CaixaDto> FecharCaixaAsync(FecharCaixaRequest request);
    Task<CaixaDto?> GetCaixaAbertoAsync(int numero);
    Task<CaixaDto?> GetCaixaPorIdAsync(int id);
    Task<IEnumerable<CaixaDto>> GetCaixasAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<CaixaDto> GetRelatorioCaixaAsync(int caixaId, DateTime? dataInicio = null, DateTime? dataFim = null);
}

