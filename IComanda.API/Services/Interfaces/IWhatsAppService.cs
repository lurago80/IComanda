namespace IComanda.API.Services.Interfaces;

/// <summary>
/// Interface para serviço de envio de mensagens via WhatsApp Web
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Envia uma mensagem automaticamente via WhatsApp Web
    /// </summary>
    /// <param name="telefone">Número do telefone (apenas números, com DDD e código do país)</param>
    /// <param name="mensagem">Mensagem a ser enviada</param>
    /// <returns>True se enviado com sucesso, False caso contrário</returns>
    Task<bool> EnviarMensagemAsync(string telefone, string mensagem);

    /// <summary>
    /// Verifica se o WhatsApp Web está conectado e pronto para enviar mensagens
    /// </summary>
    /// <returns>True se conectado, False caso contrário</returns>
    Task<bool> VerificarConexaoAsync();

    /// <summary>
    /// Inicializa o navegador e mantém a sessão do WhatsApp Web ativa
    /// </summary>
    Task InicializarAsync();

    /// <summary>
    /// Obtém informações de diagnóstico sobre a conexão do WhatsApp Web
    /// </summary>
    Task<object> ObterDiagnosticoAsync();
}
