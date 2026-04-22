using IComanda.API.Models.DTOs;

namespace IComanda.API.Repositories.Interfaces;

public interface IHistoricoRepository
{
    /// <summary>Verifica se a tabela HISTORICO_ALTERACOES existe e a cria se necessário (para uso na inicialização).</summary>
    Task EnsureTableExistsAsync();
    Task<int> CriarHistoricoAsync(HistoricoAlteracaoDto historico);
    Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoAsync(string? tipo = null, string? entidadeId = null, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<HistoricoAlteracaoDto>> GetHistoricoPorOperadorAsync(int operador, DateTime? dataInicio = null, DateTime? dataFim = null);
}

