using IComanda.API.Models.DTOs;

namespace IComanda.API.Repositories.Interfaces;

public interface INotificacaoRepository
{
    Task<int> CriarNotificacaoAsync(NotificacaoDto notificacao);
    Task<IEnumerable<NotificacaoDto>> GetNotificacoesAsync(bool? apenasNaoLidas = null, string? categoria = null);
    Task<bool> MarcarComoLidaAsync(int id);
    Task<bool> MarcarTodasComoLidasAsync();
    Task<int> GetQuantidadeNaoLidasAsync();
}

