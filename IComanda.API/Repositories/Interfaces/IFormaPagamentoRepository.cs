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
    /// Busca todas as formas de pagamento (ativas e inativas)
    /// </summary>
    Task<IEnumerable<FormaPagamento>> GetAllAsync();

    /// <summary>
    /// Busca uma forma de pagamento por ID
    /// </summary>
    Task<FormaPagamento?> GetFormaPagamentoByIdAsync(int id);

    /// <summary>
    /// Busca forma de pagamento por descrição
    /// </summary>
    Task<FormaPagamento?> GetFormaPagamentoPorDescricaoAsync(string descricao);

    /// <summary>
    /// Atualiza os dados de uma forma de pagamento
    /// </summary>
    Task<bool> UpdateAsync(FormaPagamento forma);

    /// <summary>
    /// Alterna o status ativo/inativo de uma forma de pagamento
    /// </summary>
    Task<bool> ToggleAtivoAsync(int id);

    /// <summary>
    /// Cria uma nova forma de pagamento
    /// </summary>
    Task<int> CreateAsync(FormaPagamento forma);
}

