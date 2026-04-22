using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface IHistoricoService
{
    Task RegistrarAlteracaoAsync(string tipo, string entidadeId, string acao, int operador, string? descricao = null, string? dadosAntigos = null, string? dadosNovos = null, string? nomeOperador = null);
    Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoAsync(string? tipo = null, string? entidadeId = null, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoPorOperadorAsync(int operador, DateTime? dataInicio = null, DateTime? dataFim = null);
}

