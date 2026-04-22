using IComanda.API.Models.Entities;
using System.Data;

namespace IComanda.API.Repositories.Interfaces;

public interface IItemVendaTemporarioRepository
{
    Task<bool> CriarItensTemporariosAsync(List<ItemVendaTemporario> itens, IDbTransaction? transaction = null);
    Task<IEnumerable<ItemVendaTemporario>> GetItensPorCupomAsync(string cupom, int operador);
    Task<IEnumerable<ItemVendaTemporario>> GetItensPorCupomAsync(string cupom);
    Task<bool> LimparItensCupomAsync(string cupom, int operador);
    Task<bool> LimparItensCupomAsync(string cupom, int operador, IDbTransaction? transaction);
    Task<bool> AtualizarItensTemporariosAsync(string cupom, int operador, List<ItemVendaTemporario> novosItens, IDbTransaction? transaction = null);
    Task<bool> TransferirItemAsync(string notaOrigem, int itemOrigem, string notaDestino, int operador);
    Task<ItemVendaTemporario?> GetItemPorCupomEItemAsync(string cupom, int item, int operador);
    Task<bool> CancelarItemAsync(string cupom, int item, int operador, IDbTransaction? transaction = null);
}

