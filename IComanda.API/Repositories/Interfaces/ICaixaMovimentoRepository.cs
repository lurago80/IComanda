using IComanda.API.Models.Entities;
using System.Data;

namespace IComanda.API.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de movimentos de caixa
/// </summary>
public interface ICaixaMovimentoRepository
{
    /// <summary>
    /// Gera o próximo código para movimento de caixa
    /// </summary>
    Task<int> GerarProximoCodigoAsync(IDbTransaction? transaction = null);

    /// <summary>
    /// Cria um movimento de caixa
    /// </summary>
    Task<int> CriarMovimentoAsync(CaixaMovimento movimento, IDbTransaction? transaction = null);

    /// <summary>
    /// Busca o saldo atual do caixa (terminal)
    /// </summary>
    Task<decimal> GetSaldoAtualAsync(int terminal);

    /// <summary>
    /// Busca movimentos do caixa por período
    /// </summary>
    Task<IEnumerable<CaixaMovimento>> GetMovimentosAsync(int terminal, DateTime? dataInicio = null, DateTime? dataFim = null);

    /// <summary>
    /// Busca movimentos do caixa por origem
    /// </summary>
    Task<IEnumerable<CaixaMovimento>> GetMovimentosPorOrigemAsync(int terminal, string origem, DateTime? dataInicio = null, DateTime? dataFim = null);

    /// <summary>
    /// Busca o valor de abertura do caixa em uma data (primeiro movimento ABERTURA do dia).
    /// </summary>
    Task<decimal?> GetValorAberturaAsync(int terminal, DateTime data);

    /// <summary>
    /// Busca todos os movimentos de abertura em um período (para incluir na listagem de caixas mesmo sem vendas).
    /// </summary>
    Task<IEnumerable<CaixaMovimento>> GetAberturasPorPeriodoAsync(DateTime? dataInicio = null, DateTime? dataFim = null);

    /// <summary>
    /// Busca a primeira abertura do caixa em uma data (para exibir data/hora corretas).
    /// </summary>
    Task<CaixaMovimento?> GetPrimeiraAberturaAsync(int terminal, DateTime data);

    /// <summary>
    /// Busca todos os movimentos de fechamento manual em um período.
    /// Usado para determinar se um caixa foi fechado manualmente.
    /// </summary>
    Task<IEnumerable<CaixaMovimento>> GetFechamentosPorPeriodoAsync(DateTime? dataInicio = null, DateTime? dataFim = null);

    /// <summary>
    /// Verifica se existe movimento de FECHAMENTO para um terminal em uma data.
    /// </summary>
    Task<bool> ExisteFechamentoAsync(int terminal, DateTime data);
}
