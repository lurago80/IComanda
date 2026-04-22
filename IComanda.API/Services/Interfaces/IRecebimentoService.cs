using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

/// <summary>
/// Interface para serviço de recebimentos
/// </summary>
public interface IRecebimentoService
{
    /// <summary>
    /// Fecha uma comanda aberta processando os recebimentos
    /// </summary>
    Task<FecharComandaResponseDto> FecharComandaAsync(FecharComandaRequest request);

    /// <summary>
    /// Busca todas as formas de pagamento ativas
    /// </summary>
    Task<IEnumerable<FormaPagamentoDto>> GetFormasPagamentoAtivasAsync();

    /// <summary>
    /// Busca recebimentos por nota
    /// </summary>
    Task<IEnumerable<RecebimentoVendasDto>> GetRecebimentosPorNotaAsync(string nota);

    /// <summary>
    /// Busca recebimentos por período
    /// </summary>
    Task<IEnumerable<RecebimentoVendasDto>> GetRecebimentosPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim);
}

