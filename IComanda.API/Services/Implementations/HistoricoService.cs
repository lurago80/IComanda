using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class HistoricoService : IHistoricoService
{
    private readonly IHistoricoRepository _historicoRepository;
    private readonly ILogger<HistoricoService> _logger;

    public HistoricoService(
        IHistoricoRepository historicoRepository,
        ILogger<HistoricoService> logger)
    {
        _historicoRepository = historicoRepository;
        _logger = logger;
    }

    public async Task RegistrarAlteracaoAsync(string tipo, string entidadeId, string acao, int operador, string? descricao = null, string? dadosAntigos = null, string? dadosNovos = null, string? nomeOperador = null)
    {
        var historico = new HistoricoAlteracaoDto
        {
            Tipo = tipo,
            EntidadeId = entidadeId,
            Acao = acao,
            Operador = operador,
            NomeOperador = nomeOperador,
            DataHora = DateTime.Now,
            Descricao = descricao,
            DadosAntigos = dadosAntigos,
            DadosNovos = dadosNovos
        };

        await _historicoRepository.CriarHistoricoAsync(historico);
    }

    public async Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoAsync(string? tipo = null, string? entidadeId = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        _logger.LogInformation("🔍 Buscando histórico - Tipo: {Tipo}, Entidade: {EntidadeId}", tipo, entidadeId);
        return await _historicoRepository.GetHistoricoAsync(tipo, entidadeId, dataInicio, dataFim);
    }

    public async Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoPorOperadorAsync(int operador, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        _logger.LogInformation("🔍 Buscando histórico do operador {Operador}", operador);
        return await _historicoRepository.GetHistoricoPorOperadorAsync(operador, dataInicio, dataFim);
    }
}

