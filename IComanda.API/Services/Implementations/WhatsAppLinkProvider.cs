using System.Text;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

/// <summary>
/// Provedor que gera link direto do WhatsApp Web (método mais simples e confiável)
/// </summary>
public class WhatsAppLinkProvider : IWhatsAppProvider
{
    private readonly ILogger<WhatsAppLinkProvider> _logger;

    public string Nome => "Link Direto";

    public WhatsAppLinkProvider(ILogger<WhatsAppLinkProvider> logger)
    {
        _logger = logger;
    }

    public Task<bool> EnviarMensagemAsync(string telefone, string mensagem)
    {
        // Este provedor não envia diretamente, apenas gera o link
        // O frontend deve abrir o link
        _logger.LogInformation("Link direto gerado para {Telefone}", telefone);
        return Task.FromResult(true);
    }

    public Task<bool> EstaDisponivelAsync()
    {
        // Sempre disponível, não requer configuração
        return Task.FromResult(true);
    }

    public Task<object> ObterStatusAsync()
    {
        return Task.FromResult<object>(new
        {
            nome = Nome,
            disponivel = true,
            metodo = "link_direto",
            descricao = "Gera link do WhatsApp Web para abrir no navegador"
        });
    }

    /// <summary>
    /// Gera o link do WhatsApp Web com número e mensagem (UTF-8 para acentos corretos, ex.: HAMBÚRGUER).
    /// </summary>
    public string GerarLink(string telefone, string mensagem)
    {
        var telefoneFormatado = System.Text.RegularExpressions.Regex.Replace(telefone, @"\D", "");
        if (string.IsNullOrEmpty(telefoneFormatado) || telefoneFormatado.Length < 10)
        {
            throw new ArgumentException("Número de telefone inválido", nameof(telefone));
        }

        // Garantir UTF-8 na URL para acentos (Ú, Ã, Ç, etc.) aparecerem corretamente no WhatsApp
        var bytes = Encoding.UTF8.GetBytes(mensagem);
        var mensagemEncoded = string.Concat(Array.ConvertAll(bytes, b =>
            (b >= '0' && b <= '9') || (b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z') || b == '-' || b == '_' || b == '.' || b == '~'
                ? ((char)b).ToString()
                : b == (byte)' '
                    ? "%20"
                    : $"%{b:X2}"));
        return $"https://web.whatsapp.com/send?phone={telefoneFormatado}&text={mensagemEncoded}";
    }
}
