using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Wrapper para o WhatsAppService original (Selenium) como um IWhatsAppProvider
/// </summary>
public class WhatsAppSeleniumProvider : IWhatsAppProvider
{
    private readonly WhatsAppService _whatsAppService;
    private readonly ILogger<WhatsAppSeleniumProvider> _logger;

    public string Nome => "Selenium";

    public WhatsAppSeleniumProvider(WhatsAppService whatsAppService, ILogger<WhatsAppSeleniumProvider> logger)
    {
        _whatsAppService = whatsAppService;
        _logger = logger;
    }

    public async Task<bool> EnviarMensagemAsync(string telefone, string mensagem)
    {
        return await _whatsAppService.EnviarMensagemAsync(telefone, mensagem);
    }

    public async Task<bool> EstaDisponivelAsync()
    {
        return await _whatsAppService.VerificarConexaoAsync();
    }

    public async Task<object> ObterStatusAsync()
    {
        var conectado = await _whatsAppService.VerificarConexaoAsync();
        var diagnostico = await _whatsAppService.ObterDiagnosticoAsync();
        
        return new
        {
            nome = Nome,
            disponivel = conectado,
            metodo = "selenium_webdriver",
            descricao = "Automação via Selenium WebDriver (requer Chrome com remote debugging)",
            diagnostico = diagnostico
        };
    }
}
