using IComanda.API.Models.DTOs;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

/// <summary>
/// Interface para serviço de movimentos de caixa
/// </summary>
public interface ICaixaMovimentoService
{
    /// <summary>
    /// Registra abertura de caixa (entrada)
    /// </summary>
    Task<CaixaMovimentoDto> AbrirCaixaAsync(CaixaMovimentoRequest request);

    /// <summary>
    /// Registra suprimento (entrada)
    /// </summary>
    Task<CaixaMovimentoDto> RegistrarSuprimentoAsync(CaixaMovimentoRequest request);

    /// <summary>
    /// Registra sangria (saída)
    /// </summary>
    Task<CaixaMovimentoDto> RegistrarSangriaAsync(CaixaMovimentoRequest request);

    /// <summary>
    /// Registra pagamento de despesas (saída)
    /// </summary>
    Task<CaixaMovimentoDto> RegistrarPagamentoDespesaAsync(CaixaMovimentoRequest request);

    /// <summary>
    /// Busca resumo do caixa
    /// </summary>
    Task<CaixaResumoDto> GetResumoAsync(int terminal, DateTime? dataInicio = null, DateTime? dataFim = null);

    /// <summary>
    /// Busca movimentos do caixa
    /// </summary>
    Task<IEnumerable<CaixaMovimentoDto>> GetMovimentosAsync(int terminal, DateTime? dataInicio = null, DateTime? dataFim = null);
}
