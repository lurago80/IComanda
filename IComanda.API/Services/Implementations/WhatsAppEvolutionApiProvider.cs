using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Provedor usando Evolution API (open source, mais confiável que Selenium)
/// Requer Evolution API rodando (Docker ou servidor separado)
/// </summary>
public class WhatsAppEvolutionApiProvider : IWhatsAppProvider
{
    private readonly ILogger<WhatsAppEvolutionApiProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string? _baseUrl;
    private readonly string? _apiKey;
    private readonly string? _instanceName;

    public string Nome => "Evolution API";

    public WhatsAppEvolutionApiProvider(
        ILogger<WhatsAppEvolutionApiProvider> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);

        _baseUrl = _configuration["WhatsApp:EvolutionApi:BaseUrl"];
        _apiKey = _configuration["WhatsApp:EvolutionApi:ApiKey"];
        _instanceName = _configuration["WhatsApp:EvolutionApi:InstanceName"] ?? "default";
    }

    public async Task<bool> EnviarMensagemAsync(string telefone, string mensagem)
    {
        if (!await EstaDisponivelAsync())
        {
            _logger.LogWarning("Evolution API não está disponível ou configurada");
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

            // Adicionar código do país se não tiver (assumir Brasil +55)
            if (!telefoneFormatado.StartsWith("55") && telefoneFormatado.Length == 11)
            {
                telefoneFormatado = "55" + telefoneFormatado;
            }

            var url = $"{_baseUrl}/message/sendText/{_instanceName}";
            
            var requestBody = new
            {
                number = telefoneFormatado,
                text = mensagem
            };

            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("apikey", _apiKey);
            }

            _logger.LogInformation("Enviando mensagem via Evolution API para {Telefone}...", telefoneFormatado);
            
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ Mensagem enviada com sucesso via Evolution API");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Erro ao enviar mensagem via Evolution API: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao enviar mensagem via Evolution API");
            return false;
        }
    }

    public async Task<bool> EstaDisponivelAsync()
    {
        if (string.IsNullOrEmpty(_baseUrl))
        {
            return false;
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("apikey", _apiKey);
            }

            // Verificar se a instância está conectada (state = open)
            var urlState = $"{_baseUrl.TrimEnd('/')}/instance/connectionState/{_instanceName}";
            var response = await _httpClient.GetAsync(urlState);
            if (!response.IsSuccessStatusCode)
                return false;
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return false;
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("state", out var stateEl))
            {
                var state = stateEl.GetString()?.ToLowerInvariant();
                return state == "open";
            }
            if (root.TryGetProperty("instance", out var inst) && inst.TryGetProperty("state", out var stateEl2))
            {
                var state = stateEl2.GetString()?.ToLowerInvariant();
                return state == "open";
            }
            return false;
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
            metodo = "evolution_api",
            baseUrl = _baseUrl ?? "não configurado",
            instanceName = _instanceName,
            configurado = !string.IsNullOrEmpty(_baseUrl)
        };
    }
}
