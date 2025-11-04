using IComanda.API.Models.Entities;

namespace IComanda.API.Repositories.Interfaces;

public interface IVendaRepository
{
    Task<string> GerarProximaNotaAsync();
    Task<bool> CriarVendaAsync(Venda venda);
    Task<bool> CriarItensVendaAsync(List<ItemVenda> itens);
    Task<Venda?> GetVendaAsync(string nota);
    Task<IEnumerable<ItemVenda>> GetItensVendaAsync(string nota);
    Task<IEnumerable<Venda>> GetVendasHojeAsync();
    Task<IEnumerable<Venda>> GetVendasPorComandaAsync(int comanda);
    Task<IEnumerable<Venda>> GetVendasPorMesaAsync(int mesa);
    Task<Venda?> GetVendaAbertaPorMesaAsync(int mesa);
    Task<Venda?> GetVendaAbertaPorComandaAsync(int comanda);
}
