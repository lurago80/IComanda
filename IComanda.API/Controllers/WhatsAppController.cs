using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IComanda.API.Services.Interfaces;
using System.Threading;
using System.Text.Json;

namespace IComanda.API.Controllers;

/// <summary>
/// Controller para envio de mensagens via WhatsApp Web (Evolution API / Baileys)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class WhatsAppController : ControllerBase
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly ILogger<WhatsAppController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public WhatsAppController(
        IWhatsAppService whatsAppService,
        ILogger<WhatsAppController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _whatsAppService = whatsAppService;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Envia uma mensagem automaticamente via WhatsApp Web
    /// </summary>
    /// <param name="request">Dados da mensagem (telefone e mensagem)</param>
    /// <returns>Resultado do envio</returns>
    [HttpPost("enviar")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult> EnviarMensagem([FromBody] EnviarMensagemRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Telefone))
            {
                return BadRequest(new { mensagem = "Telefone é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(request.Mensagem))
            {
                return BadRequest(new { mensagem = "Mensagem é obrigatória" });
            }

            _logger.LogInformation("Enviando mensagem WhatsApp para {Telefone}", request.Telefone);

            // Tentar enviar mensagem
            _logger.LogInformation("Iniciando envio de mensagem para {Telefone}...", request.Telefone);
            
            string? link = null;
            bool sucesso = false;
            
            // Se o serviço suporta retorno de link (WhatsAppMultiProviderService)
            if (_whatsAppService is Services.Implementations.WhatsAppMultiProviderService multiProvider)
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                try
                {
                    var (sucessoEnvio, linkGerado) = await Task.Run(
                        async () => await multiProvider.EnviarMensagemComLinkAsync(request.Telefone, request.Mensagem),
                        cancellationTokenSource.Token);
                    sucesso = sucessoEnvio;
                    link = linkGerado;
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⚠️ Timeout ao enviar mensagem para {Telefone}", request.Telefone);
                    return StatusCode(504, new { 
                        sucesso = false, 
                        mensagem = "Tempo de espera excedido. O envio está demorando mais que o esperado." 
                    });
                }
            }
            else
            {
                // Método antigo (compatibilidade)
                var conectado = await _whatsAppService.VerificarConexaoAsync();
                if (!conectado)
                {
                    _logger.LogWarning("WhatsApp Web não está conectado ou serviço não está disponível");
                    return StatusCode(503, new { 
                        sucesso = false,
                        mensagem = "WhatsApp Web não está conectado. Use o método manual (botão copiar) ou inicialize o serviço primeiro.",
                        precisaLogin = true
                    });
                }

                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                try
                {
                    var taskEnvio = _whatsAppService.EnviarMensagemAsync(request.Telefone, request.Mensagem);
                    sucesso = await Task.Run(async () => await taskEnvio, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⚠️ Timeout ao enviar mensagem para {Telefone}", request.Telefone);
                    return StatusCode(504, new { 
                        sucesso = false, 
                        mensagem = "Tempo de espera excedido. O envio está demorando mais que o esperado." 
                    });
                }
            }

            if (sucesso)
            {
                if (!string.IsNullOrEmpty(link))
                {
                    _logger.LogInformation("✅ Link gerado para {Telefone}: {Link}", request.Telefone, link);
                    return Ok(new { 
                        sucesso = true, 
                        mensagem = "Link gerado com sucesso!",
                        link = link,
                        metodo = "link_direto"
                    });
                }
                else
                {
                    _logger.LogInformation("✅ Mensagem enviada com sucesso para {Telefone}", request.Telefone);
                    return Ok(new { 
                        sucesso = true, 
                        mensagem = "Mensagem enviada com sucesso!",
                        metodo = "automatico"
                    });
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Falha ao enviar mensagem para {Telefone}", request.Telefone);
                return StatusCode(500, new { 
                    sucesso = false, 
                    mensagem = "Não foi possível enviar a mensagem. Verifique se o WhatsApp Web está conectado e se o número está correto." 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem WhatsApp");
            return StatusCode(500, new { 
                sucesso = false, 
                mensagem = "Erro interno do servidor", 
                detalhes = ex.Message 
            });
        }
    }

    /// <summary>
    /// Verifica se o WhatsApp Web está conectado
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> VerificarStatus()
    {
        try
        {
            _logger.LogInformation("Verificando status do WhatsApp Web...");
            
            // Tentar inicializar se não estiver inicializado
            try
            {
                await _whatsAppService.InicializarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao tentar inicializar durante verificação de status");
            }
            
            var conectado = await _whatsAppService.VerificarConexaoAsync();
            
            _logger.LogInformation("Status do WhatsApp Web: {Conectado}", conectado ? "Conectado" : "Nao conectado");
            
            return Ok(new { conectado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status do WhatsApp");
            return StatusCode(500, new { 
                conectado = false,
                mensagem = "Erro ao verificar status", 
                detalhes = ex.Message 
            });
        }
    }

    /// <summary>
    /// Inicializa o WhatsApp Web (abre navegador)
    /// </summary>
    [HttpPost("inicializar")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> Inicializar()
    {
        try
        {
            _logger.LogInformation("Inicializando WhatsApp Web...");
            await _whatsAppService.InicializarAsync();
            return Ok(new { mensagem = "WhatsApp Web inicializado. Faça o login manualmente se necessário." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar WhatsApp Web");
            return StatusCode(500, new { mensagem = "Erro ao inicializar", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Diagnóstico completo do WhatsApp Web
    /// </summary>
    [HttpGet("diagnostico")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> Diagnostico()
    {
        try
        {
            var diagnostico = await _whatsAppService.ObterDiagnosticoAsync();
            return Ok(diagnostico);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter diagnóstico");
            return StatusCode(500, new { mensagem = "Erro ao obter diagnóstico", detalhes = ex.Message });
        }
    }

    /// <summary>
    /// Obtém o QR Code para conectar o WhatsApp (Baileys = Node sem Docker, ou Evolution API).
    /// </summary>
    [HttpGet("qrcode")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> ObterQrCode()
    {
        var metodo = _configuration["WhatsApp:Metodo"]?.ToLowerInvariant();
        if (metodo == "baileys")
        {
            var baseUrl = _configuration["WhatsApp:BaileysService:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrEmpty(baseUrl))
                return Ok(new { base64 = (string?)null, conectado = false, erro = "Baileys não configurado. Configure WhatsApp:BaileysService:BaseUrl no appsettings.json." });
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                var response = await client.GetAsync($"{baseUrl}/qrcode");
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var conectado = root.TryGetProperty("conectado", out var c) && c.GetBoolean();
                string? base64 = null;
                if (root.TryGetProperty("base64", out var b64))
                    base64 = b64.GetString();
                string? erro = null;
                if (root.TryGetProperty("erro", out var e))
                    erro = e.GetString();
                return Ok(new { base64 = base64, conectado, erro });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao obter QR do serviço Baileys");
                return Ok(new { base64 = (string?)null, conectado = false, erro = "Serviço Baileys não está rodando. No terminal: cd icomanda-whatsapp-baileys && npm run dev" });
            }
        }

        var evolutionBaseUrl = _configuration["WhatsApp:EvolutionApi:BaseUrl"]?.TrimEnd('/');
        var apiKey = _configuration["WhatsApp:EvolutionApi:ApiKey"];
        var instanceName = _configuration["WhatsApp:EvolutionApi:InstanceName"] ?? "default";
        if (string.IsNullOrEmpty(evolutionBaseUrl))
        {
            return Ok(new { base64 = (string?)null, conectado = false, erro = "Evolution API não configurada. Configure WhatsApp:EvolutionApi:BaseUrl no appsettings.json." });
        }
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            if (!string.IsNullOrEmpty(apiKey))
                client.DefaultRequestHeaders.Add("apikey", apiKey);
            var url = $"{evolutionBaseUrl}/instance/connect/{instanceName}";
            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return Ok(new { base64 = (string?)null, conectado = false, erro = json });
            }
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            string? qrBase64 = null;
            if (root.TryGetProperty("base64", out var b64))
                qrBase64 = b64.GetString();
            else if (root.TryGetProperty("code", out var codeEl))
                qrBase64 = codeEl.GetString();
            return Ok(new { base64 = qrBase64, conectado = false });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao obter QR Code da Evolution API");
            return Ok(new { base64 = (string?)null, conectado = false, erro = ex.Message });
        }
    }

    /// <summary>
    /// Verifica se a instância Evolution está conectada (WhatsApp pronto para envio direto).
    /// </summary>
    [HttpGet("conectado")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> VerificarConectado()
    {
        var conectado = await _whatsAppService.VerificarConexaoAsync();
        return Ok(new { conectado });
    }

    /// <summary>
    /// Reseta a sessão do Baileys: apaga auth_info_baileys e reconecta (gera novo QR).
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> ResetarSessao()
    {
        var metodo = _configuration["WhatsApp:Metodo"]?.ToLowerInvariant();
        if (metodo != "baileys")
            return BadRequest(new { sucesso = false, mensagem = "Reset disponível apenas para o método Baileys." });

        var baseUrl = _configuration["WhatsApp:BaileysService:BaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            return BadRequest(new { sucesso = false, mensagem = "Baileys não configurado. Verifique WhatsApp:BaileysService:BaseUrl." });

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            var response = await client.PostAsync($"{baseUrl}/reset", null);
            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Reset sessão WhatsApp Baileys: {Json}", json);
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resetar sessão Baileys");
            return StatusCode(500, new { sucesso = false, mensagem = "Não foi possível comunicar com o serviço Baileys.", detalhes = ex.Message });
        }
    }
}

/// <summary>
/// Request para envio de mensagem
/// </summary>
public class EnviarMensagemRequest
{
    public string Telefone { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}
