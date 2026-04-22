using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Serviço que gerencia múltiplos provedores de WhatsApp e escolhe o melhor disponível
/// </summary>
public class WhatsAppMultiProviderService : IWhatsAppService
{
    private readonly ILogger<WhatsAppMultiProviderService> _logger;
    private readonly IConfiguration _configuration;
    private readonly List<IWhatsAppProvider> _providers;
    private readonly IWhatsAppProvider? _providerSelecionado;
    private readonly WhatsAppLinkProvider _linkProvider;

    public WhatsAppMultiProviderService(
        ILogger<WhatsAppMultiProviderService> logger,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        IEnumerable<IWhatsAppProvider> providers)
    {
        _logger = logger;
        _configuration = configuration;
        _providers = providers.ToList();
        _linkProvider = _providers.OfType<WhatsAppLinkProvider>().FirstOrDefault() 
            ?? new WhatsAppLinkProvider(loggerFactory.CreateLogger<WhatsAppLinkProvider>());

        // Obter método preferido da configuração
        var metodoPreferido = _configuration["WhatsApp:Metodo"] ?? "link";
        
        _logger.LogInformation("Método WhatsApp preferido: {Metodo}", metodoPreferido);
        _logger.LogInformation("Provedores disponíveis: {Providers}", 
            string.Join(", ", _providers.Select(p => p.Nome)));

        // Selecionar provedor baseado na configuração (Baileys = Node sem Docker)
        _providerSelecionado = metodoPreferido.ToLower() switch
        {
            "baileys" => _providers.OfType<WhatsAppBaileysProvider>().FirstOrDefault(),
            "evolution" or "evolutionapi" => _providers.OfType<WhatsAppEvolutionApiProvider>().FirstOrDefault(),
            "selenium" or "webdriver" => _providers.OfType<WhatsAppSeleniumProvider>().FirstOrDefault(),
            "link" or "linkdireto" or "directlink" => _linkProvider,
            _ => _linkProvider // Padrão: link direto
        };

        if (_providerSelecionado != null)
        {
            _logger.LogInformation("Provedor selecionado: {Nome}", _providerSelecionado.Nome);
        }
    }

    public async Task<(bool Sucesso, string? Link)> EnviarMensagemComLinkAsync(string telefone, string mensagem)
    {
        // Se o método é "link", retornar o link
        if (_providerSelecionado is WhatsAppLinkProvider)
        {
            var link = _linkProvider.GerarLink(telefone, mensagem);
            _logger.LogInformation("Link gerado: {Link}", link);
            return (true, link);
        }

        // Tentar usar o provedor selecionado
        if (_providerSelecionado != null)
        {
            var disponivel = await _providerSelecionado.EstaDisponivelAsync();
            if (disponivel)
            {
                _logger.LogInformation("Tentando enviar via {Provedor}...", _providerSelecionado.Nome);
                var sucesso = await _providerSelecionado.EnviarMensagemAsync(telefone, mensagem);
                return (sucesso, null);
            }
            else
            {
                _logger.LogWarning("Provedor {Provedor} não está disponível, tentando alternativas...", 
                    _providerSelecionado.Nome);
            }
        }

        // Se o provedor selecionado não está disponível, tentar outros em ordem de preferência
        var ordemPreferencia = new[] { "Baileys (Node)", "Evolution API", "Selenium", "Link Direto" };
        
        foreach (var nomeProvedor in ordemPreferencia)
        {
            var provider = _providers.FirstOrDefault(p => p.Nome == nomeProvedor);
            if (provider != null && provider != _providerSelecionado)
            {
                var disponivel = await provider.EstaDisponivelAsync();
                if (disponivel)
                {
                    _logger.LogInformation("Usando provedor alternativo: {Provedor}", provider.Nome);
                    var sucesso = await provider.EnviarMensagemAsync(telefone, mensagem);
                    return (sucesso, null);
                }
            }
        }

        // Se nenhum provedor automático funcionou, usar link direto como fallback
        _logger.LogWarning("Nenhum provedor automático disponível, usando link direto como fallback");
        var linkFallback = _linkProvider.GerarLink(telefone, mensagem);
        _logger.LogInformation("Link de fallback gerado: {Link}", linkFallback);
        return (true, linkFallback);
    }

    public async Task<bool> EnviarMensagemAsync(string telefone, string mensagem)
    {
        var (sucesso, _) = await EnviarMensagemComLinkAsync(telefone, mensagem);
        return sucesso;
    }

    public async Task<bool> VerificarConexaoAsync()
    {
        if (_providerSelecionado is WhatsAppLinkProvider)
        {
            // Link direto sempre está "disponível"
            return true;
        }

        if (_providerSelecionado != null)
        {
            return await _providerSelecionado.EstaDisponivelAsync();
        }

        // Verificar se algum provedor está disponível
        foreach (var provider in _providers)
        {
            if (provider is WhatsAppLinkProvider)
                continue; // Pular link provider

            if (await provider.EstaDisponivelAsync())
            {
                return true;
            }
        }

        return false;
    }

    public Task InicializarAsync()
    {
        // Para o serviço multi-provider, a inicialização é feita automaticamente
        // Se usar Selenium, ele será inicializado quando necessário
        var seleniumProvider = _providers.OfType<WhatsAppSeleniumProvider>().FirstOrDefault();
        if (seleniumProvider != null)
        {
            // O WhatsAppSeleniumProvider usa o WhatsAppService internamente
            // A inicialização será feita automaticamente quando necessário
        }

        return Task.CompletedTask;
    }

    public async Task<object> ObterDiagnosticoAsync()
    {
        var diagnosticos = new List<object>();

        foreach (var provider in _providers)
        {
            try
            {
                var status = await provider.ObterStatusAsync();
                diagnosticos.Add(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter diagnóstico do provedor {Nome}", provider.Nome);
                diagnosticos.Add(new
                {
                    nome = provider.Nome,
                    erro = ex.Message
                });
            }
        }

        return new
        {
            timestamp = DateTime.Now,
            provedorSelecionado = _providerSelecionado?.Nome ?? "nenhum",
            metodoConfigurado = _configuration["WhatsApp:Metodo"] ?? "link",
            provedores = diagnosticos
        };
    }
}
