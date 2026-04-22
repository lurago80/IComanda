using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Implementação legada via Selenium — desabilitada. O envio real é feito pelo Baileys (WhatsAppBaileysProvider).
/// Mantida apenas para compatibilidade com o WhatsAppSeleniumProvider registrado no DI.
/// </summary>
public class WhatsAppService : IWhatsAppService, IDisposable
{
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(ILogger<WhatsAppService> logger)
    {
        _logger = logger;
    }

    public Task InicializarAsync()
    {
        _logger.LogInformation("WhatsAppService (Selenium) está desabilitado. Use o Baileys.");
        return Task.CompletedTask;
    }

    public Task<bool> VerificarConexaoAsync()
    {
        return Task.FromResult(false);
    }

    public Task<bool> EnviarMensagemAsync(string telefone, string mensagem)
    {
        _logger.LogWarning("WhatsAppService (Selenium) está desabilitado. Mensagem não enviada.");
        return Task.FromResult(false);
    }

    public Task<object> ObterDiagnosticoAsync()
    {
        return Task.FromResult<object>(new
        {
            disponivel = false,
            mensagem = "Provedor Selenium desabilitado. Use o Baileys.",
            metodo = "selenium_desabilitado"
        });
    }

    public void Dispose() { }
}
