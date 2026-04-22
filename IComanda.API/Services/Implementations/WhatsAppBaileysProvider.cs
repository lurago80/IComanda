using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Provedor usando o serviço Node.js com Baileys (sem Docker).
/// Rode: cd icomanda-whatsapp-baileys && npm run dev
/// </summary>
public class WhatsAppBaileysProvider : IWhatsAppProvider
{
    private readonly ILogger<WhatsAppBaileysProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string? _baseUrl;

    public string Nome => "Baileys (Node)";

    public WhatsAppBaileysProvider(
        ILogger<WhatsAppBaileysProvider> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _baseUrl = _configuration["WhatsApp:BaileysService:BaseUrl"]?.TrimEnd('/');
    }

    public async Task<bool> EnviarMensagemAsync(string telefone, string mensagem)
    {
        if (!await EstaDisponivelAsync())
        {
            _logger.LogWarning("Serviço Baileys não está disponível ou configurado");
            return false;
        }

        try
        {
            var telefoneFormatado = System.Text.RegularExpressions.Regex.Replace(telefone, @"\D", "");
            if (string.IsNullOrEmpty(telefoneFormatado) || telefoneFormatado.Length < 10)
            {
                _logger.LogError("Número de telefone inválido: {Telefone}", telefone);
                return false;
            }
            if (!telefoneFormatado.StartsWith("55") && telefoneFormatado.Length == 11)
                telefoneFormatado = "55" + telefoneFormatado;

            var url = $"{_baseUrl}/send";
            var body = new { telefone = telefoneFormatado, mensagem };
            _logger.LogInformation("Enviando mensagem via Baileys para {Telefone}...", telefoneFormatado);
            var response = await _httpClient.PostAsJsonAsync(url, body);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var sucesso = doc.RootElement.TryGetProperty("sucesso", out var s) && s.GetBoolean();
                if (sucesso)
                    _logger.LogInformation("✅ Mensagem enviada com sucesso via Baileys");
                return sucesso;
            }
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Baileys retornou {StatusCode}: {Error}", response.StatusCode, err);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem via Baileys");
            return false;
        }
    }

    public async Task<bool> EstaDisponivelAsync()
    {
        if (string.IsNullOrEmpty(_baseUrl))
            return false;
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/status");
            if (!response.IsSuccessStatusCode)
                return false;
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("conectado", out var c) && c.GetBoolean();
        }
        catch
        {
            return false;
        }
    }

    public async Task<object> ObterStatusAsync()
    {
        var disponivel = await EstaDisponivelAsync();
        return new
        {
            nome = Nome,
            disponivel = disponivel,
            metodo = "baileys",
            baseUrl = _baseUrl ?? "não configurado",
            configurado = !string.IsNullOrEmpty(_baseUrl)
        };
    }
}
