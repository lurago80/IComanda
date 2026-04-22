using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces
{
    /// <summary>
    /// Interface para repositório de recebimentos de vendas
    /// </summary>
    public interface IRecebimentoRepository
    {
        Task<List<Recebimento>> GetAllAsync();
        Task<Recebimento?> GetByIdAsync(int id);
        Task<List<Recebimento>> GetByComandaAsync(int comandaId);
        Task<List<Recebimento>> GetByFormaPagamentoAsync(IComanda.API.Models.Enums.FormaPagamento forma);
        Task<List<Recebimento>> GetPendentesAsync();
        Task CreateAsync(Recebimento recebimento);
        Task UpdateAsync(Recebimento recebimento);
        Task DeleteAsync(int id);
        Task<int> CriarRecebimentoAsync(RecebimentoVendas recebimento, IDbTransaction? transaction = null);
        Task<IEnumerable<RecebimentoVendas>> GetRecebimentosPorNotaAsync(string nota);
        Task<IEnumerable<RecebimentoVendas>> GetRecebimentosPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim);
    }
}


