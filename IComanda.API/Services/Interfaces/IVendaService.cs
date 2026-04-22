using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;

namespace IComanda.API.Services.Interfaces;

public interface IVendaService
{
    Task<VendaDto> CriarVendaAsync(CriarVendaRequest request);
    Task<PedidoDeliveryDto> CriarPedidoDeliveryAsync(CriarPedidoDeliveryRequest request);
    Task<VendaDto> AtualizarVendaAsync(string nota, CriarVendaRequest request);
    Task<VendaDto?> GetVendaAsync(string nota);
    Task<IEnumerable<VendaDto>> GetVendasHojeAsync();
    Task<IEnumerable<VendaFechadaReciboDto>> GetVendasFechadasPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim);
    Task<IEnumerable<VendaDto>> GetVendasPorComandaAsync(int comanda);
    Task<IEnumerable<VendaDto>> GetVendasPorMesaAsync(int mesa);
    /// <param name="origem">BA = comandas, DL = delivery. Se null/vazio, retorna BA.</param>
    Task<IEnumerable<VendaDto>> GetVendasAbertasAsync(string? origem = null);
    /// <summary>Lista apenas pedidos de delivery em aberto (atalho para GetVendasAbertasAsync("DL"))</summary>
    Task<IEnumerable<PedidoDeliveryDto>> GetPedidosDeliveryAbertosAsync();
    /// <summary>Comandas/vendas excluídas (lancado = CANCELADO).</summary>
    Task<IEnumerable<VendaDto>> GetVendasCanceladasAsync(string? origem = null, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<ConferenciaMesaDto?> GetConferenciaMesaAsync(int mesa);
    Task<ConferenciaMesaDto?> GetConferenciaComandaAsync(int comanda);
    Task<bool> VerificarComandaAbertaAsync(int comanda);
    Task<VendaDto> TransferirItemAsync(TransferirItemRequest request);
    Task<VendaDto> CancelarItemAsync(CancelarItemRequest request);
    Task<bool> CancelarVendaAsync(string nota, int operador, string justificativa = "");
    /// <summary>
    /// Envia WhatsApp ao cliente informando que o pedido delivery saiu para entrega.
    /// </summary>
    /// <param name="nota">Nota do pedido (deve ser venda com origem DL)</param>
    /// <returns>True se a mensagem foi enviada (ou tentada); false se pedido não for delivery ou cliente sem telefone</returns>
    Task<bool> NotificarSaiuParaEntregaAsync(string nota);
}
