using IComanda.API.Models.Entities;
using System.Data;

namespace IComanda.API.Repositories.Interfaces;

public interface ICaixaRepository
{
    Task<Caixa?> GetCaixaAbertoAsync(int numero);
    Task<Caixa?> GetCaixaPorIdAsync(int id);
    Task<IEnumerable<Caixa>> GetCaixasAsync(DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<int> CriarCaixaAsync(Caixa caixa, IDbTransaction? transaction = null);
    Task<bool> AtualizarCaixaAsync(Caixa caixa, IDbTransaction? transaction = null);
    Task<decimal> GetTotalVendasPorCaixaAsync(int caixaId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<decimal> GetTotalRecebimentosPorCaixaAsync(int caixaId, DateTime? dataInicio = null, DateTime? dataFim = null);
}

