using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IComanda.API.Repositories.Implementations;

public class NotificacaoRepository : INotificacaoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<NotificacaoRepository> _logger;
    private static readonly List<NotificacaoDto> _notificacoesEmMemoria = new();
    private static int _proximoId = 1;

    public NotificacaoRepository(IDbConnectionFactory connectionFactory, ILogger<NotificacaoRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public Task<int> CriarNotificacaoAsync(NotificacaoDto notificacao)
    {
        // Como não há tabela de notificações, vamos usar memória
        // Em produção, seria melhor criar uma tabela NOTIFICACOES
        
        notificacao.Id = _proximoId++;
        notificacao.DataHora = DateTime.Now;
        notificacao.Lida = false;

        _notificacoesEmMemoria.Add(notificacao);

        _logger.LogInformation("🔔 Notificação criada: {Titulo} - {Mensagem}", notificacao.Titulo, notificacao.Mensagem);

        return Task.FromResult(notificacao.Id);
    }

    public Task<IEnumerable<NotificacaoDto>> GetNotificacoesAsync(bool? apenasNaoLidas = null, string? categoria = null)
    {
        var query = _notificacoesEmMemoria.AsQueryable();

        if (apenasNaoLidas.HasValue && apenasNaoLidas.Value)
        {
            query = query.Where(n => !n.Lida);
        }

        if (!string.IsNullOrEmpty(categoria))
        {
            query = query.Where(n => n.Categoria == categoria);
        }

        IEnumerable<NotificacaoDto> result = query.OrderByDescending(n => n.DataHora).ToList();
        return Task.FromResult(result);
    }

    public Task<bool> MarcarComoLidaAsync(int id)
    {
        var notificacao = _notificacoesEmMemoria.FirstOrDefault(n => n.Id == id);
        if (notificacao != null)
        {
            notificacao.Lida = true;
            _logger.LogInformation("✅ Notificação {Id} marcada como lida", id);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<bool> MarcarTodasComoLidasAsync()
    {
        foreach (var notificacao in _notificacoesEmMemoria.Where(n => !n.Lida))
        {
            notificacao.Lida = true;
        }

        _logger.LogInformation("✅ Todas as notificações marcadas como lidas");
        return Task.FromResult(true);
    }

    public Task<int> GetQuantidadeNaoLidasAsync()
    {
        return Task.FromResult(_notificacoesEmMemoria.Count(n => !n.Lida));
    }
}

