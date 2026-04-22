using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

/// <summary>
/// Interface para repositório de formas de pagamento
/// </summary>
public interface IFormaPagamentoRepository
{
    /// <summary>
    /// Busca todas as formas de pagamento ativas
    /// </summary>
    Task<IEnumerable<FormaPagamento>> GetFormasPagamentoAtivasAsync();

    /// <summary>
    /// Busca uma forma de pagamento por ID
    /// </summary>
    Task<FormaPagamento?> GetFormaPagamentoByIdAsync(int id);

    /// <summary>
    /// Busca forma de pagamento por descrição
    /// </summary>
    Task<FormaPagamento?> GetFormaPagamentoPorDescricaoAsync(string descricao);
}

