using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface INotificacaoService
{
    Task CriarNotificacaoAsync(string tipo, string categoria, string titulo, string mensagem, string? entidadeId = null, string? entidadeTipo = null, int? prioridade = null);
    Task<IEnumerable<NotificacaoDto>> GetNotificacoesAsync(bool? apenasNaoLidas = null, string? categoria = null);
    Task<bool> MarcarComoLidaAsync(int id);
    Task<bool> MarcarTodasComoLidasAsync();
    Task<int> GetQuantidadeNaoLidasAsync();
    Task VerificarAlertasAsync(); // Verifica e cria alertas automáticos
}

