using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class NotificacaoService : INotificacaoService
{
    private readonly INotificacaoRepository _notificacaoRepository;
    private readonly IMesaRepository _mesaRepository;
    private readonly IVendaRepository _vendaRepository;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(
        INotificacaoRepository notificacaoRepository,
        IMesaRepository mesaRepository,
        IVendaRepository vendaRepository,
        ILogger<NotificacaoService> logger)
    {
        _notificacaoRepository = notificacaoRepository;
        _mesaRepository = mesaRepository;
        _vendaRepository = vendaRepository;
        _logger = logger;
    }

    public async Task CriarNotificacaoAsync(string tipo, string categoria, string titulo, string mensagem, string? entidadeId = null, string? entidadeTipo = null, int? prioridade = null)
    {
        var notificacao = new NotificacaoDto
        {
            Tipo = tipo,
            Categoria = categoria,
            Titulo = titulo,
            Mensagem = mensagem,
            EntidadeId = entidadeId,
            EntidadeTipo = entidadeTipo,
            Prioridade = prioridade ?? 2
        };

        await _notificacaoRepository.CriarNotificacaoAsync(notificacao);
    }

    public async Task<IEnumerable<NotificacaoDto>> GetNotificacoesAsync(bool? apenasNaoLidas = null, string? categoria = null)
    {
        return await _notificacaoRepository.GetNotificacoesAsync(apenasNaoLidas, categoria);
    }

    public async Task<bool> MarcarComoLidaAsync(int id)
    {
        return await _notificacaoRepository.MarcarComoLidaAsync(id);
    }

    public async Task<bool> MarcarTodasComoLidasAsync()
    {
        return await _notificacaoRepository.MarcarTodasComoLidasAsync();
    }

    public async Task<int> GetQuantidadeNaoLidasAsync()
    {
        return await _notificacaoRepository.GetQuantidadeNaoLidasAsync();
    }

    public async Task VerificarAlertasAsync()
    {
        _logger.LogInformation("🔔 Verificando alertas automáticos");

        // Verificar comandas abertas há muito tempo
        var vendasAbertas = await _vendaRepository.GetVendasAbertasAsync();
        foreach (var venda in vendasAbertas)
        {
            var tempoAberto = DateTime.Now - venda.Emissao;
            if (tempoAberto.TotalHours > 2) // Mais de 2 horas
            {
                await CriarNotificacaoAsync(
                    "ALERTA",
                    "COMANDA",
                    $"Comanda aberta há muito tempo",
                    $"A comanda {venda.Comanda} (Nota: {venda.Nota}) está aberta há {tempoAberto.Hours}h {tempoAberto.Minutes}min",
                    venda.Nota,
                    "VENDA",
                    3);
            }
        }

        // Verificar mesas ocupadas há muito tempo
        var mesasOcupadas = await _mesaRepository.GetMesasOcupadasAsync();
        foreach (var mesa in mesasOcupadas)
        {
            if (mesa.TempoOcupacao.HasValue && mesa.TempoOcupacao.Value.TotalHours > 3) // Mais de 3 horas
            {
                await CriarNotificacaoAsync(
                    "AVISO",
                    "MESA",
                    $"Mesa ocupada há muito tempo",
                    $"A mesa {mesa.Numero} está ocupada há {mesa.TempoOcupacao.Value.Hours}h {mesa.TempoOcupacao.Value.Minutes}min",
                    mesa.Numero.ToString(),
                    "MESA",
                    2);
            }
        }
    }
}

