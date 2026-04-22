using IComanda.API.Models.Entities;
using System.Data;

namespace IComanda.API.Repositories.Interfaces;

public interface IVendaRepository
{
    Task<string> GerarProximaNotaAsync();
    Task<int> GerarProximoNumeroComandaAsync();
    Task<bool> CriarVendaAsync(Venda venda, IDbTransaction? transaction = null);
    Task<bool> CriarItensVendaAsync(List<ItemVenda> itens);
    Task<bool> CriarItensVendaAsync(List<ItemVenda> itens, IDbTransaction? transaction);
    Task<Venda?> GetVendaAsync(string nota);
    Task<Venda?> GetVendaSemFiltroStatusAsync(string nota);
    Task<IEnumerable<ItemVenda>> GetItensVendaAsync(string nota);
    Task<IEnumerable<Venda>> GetVendasHojeAsync();
    Task<IEnumerable<Venda>> GetVendasFechadasPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim);
    Task<IEnumerable<Venda>> GetVendasPorComandaAsync(int comanda);
    Task<IEnumerable<Venda>> GetVendasPorMesaAsync(int mesa);
    Task<Venda?> GetVendaAbertaPorMesaAsync(int mesa);
    Task<Venda?> GetVendaAbertaPorComandaAsync(int comanda);
    /// <summary>Obtém venda aberta pela nota (Caixa Rápido / PDV).</summary>
    Task<Venda?> GetVendaAbertaPorNotaAsync(string nota);
    /// <param name="origem">BA = comandas, DL = delivery. Se null/vazio, retorna BA.</param>
    Task<IEnumerable<Venda>> GetVendasAbertasAsync(string? origem = null);
    /// <summary>Comandas/vendas excluídas (lancado = CANCELADO).</summary>
    Task<IEnumerable<Venda>> GetVendasCanceladasAsync(string? origem = null, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<bool> AtualizarTotalVendaAsync(string nota, decimal totalProdutos, decimal total);
    Task<bool> AtualizarStatusVendaAsync(string nota, string status, IDbTransaction? transaction = null);
    /// <summary>Cancela a venda gravando também a justificativa no campo JUSTIFICATIVA da tabela VENDAS.</summary>
    Task<bool> CancelarComComandaAsync(string nota, string justificativa, IDbTransaction? transaction = null);
    /// <summary>Lê o campo SENHAD da tabela PARAMETROS (senha de cancelamento configurada no sistema).</summary>
    Task<string?> GetSenhaCancelamentoAsync();
    /// <summary>Obtém apenas o nome_cliente da venda (para exibição quando cliente não cadastrado).</summary>
    Task<string?> GetNomeClientePorNotaAsync(string nota);
}
