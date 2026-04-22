
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using IComanda.API.Models.Entities;
using IComanda.API.Data;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations
{
    public class RecebimentoRepository : IRecebimentoRepository
    {
        public RecebimentoRepository(IDbConnectionFactory connectionFactory) {}

        public Task<List<Recebimento>> GetAllAsync() => Task.FromResult(new List<Recebimento>());
        public Task<Recebimento?> GetByIdAsync(int id) => Task.FromResult<Recebimento?>(null);
        public Task<List<Recebimento>> GetByComandaAsync(int comandaId) => Task.FromResult(new List<Recebimento>());
        public Task<List<Recebimento>> GetByFormaPagamentoAsync(IComanda.API.Models.Enums.FormaPagamento forma) => Task.FromResult(new List<Recebimento>());
        public Task<List<Recebimento>> GetPendentesAsync() => Task.FromResult(new List<Recebimento>());
        public Task CreateAsync(Recebimento recebimento) => Task.CompletedTask;
        public Task UpdateAsync(Recebimento recebimento) => Task.CompletedTask;
        public Task DeleteAsync(int id) => Task.CompletedTask;
        public Task<int> CriarRecebimentoAsync(RecebimentoVendas recebimento, IDbTransaction? transaction = null) => Task.FromResult(0);
        public Task<IEnumerable<RecebimentoVendas>> GetRecebimentosPorNotaAsync(string nota) => Task.FromResult<IEnumerable<RecebimentoVendas>>(new List<RecebimentoVendas>());
        public Task<IEnumerable<RecebimentoVendas>> GetRecebimentosPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim) => Task.FromResult<IEnumerable<RecebimentoVendas>>(new List<RecebimentoVendas>());
    }
}


