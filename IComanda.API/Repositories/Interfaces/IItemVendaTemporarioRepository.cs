using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IItemVendaTemporarioRepository
{
    Task<bool> CriarItensTemporariosAsync(List<ItemVendaTemporario> itens);
    Task<IEnumerable<ItemVendaTemporario>> GetItensPorCupomAsync(string cupom, int operador);
    Task<bool> LimparItensCupomAsync(string cupom, int operador);
}

