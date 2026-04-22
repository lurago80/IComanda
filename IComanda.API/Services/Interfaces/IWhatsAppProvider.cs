namespace IComanda.API.Services.Interfaces;

/// <summary>
/// Interface para diferentes provedores de envio de WhatsApp
/// </summary>
public interface IWhatsAppProvider
{
    /// <summary>
    /// Nome do provedor
    /// </summary>
    string Nome { get; }

    /// <summary>
    /// Envia uma mensagem via WhatsApp
    /// </summary>
    /// <param name="telefone">Número do telefone (apenas números, com DDD e código do país)</param>
    /// <param name="mensagem">Mensagem a ser enviada</param>
    /// <returns>True se enviado com sucesso, False caso contrário</returns>
    Task<bool> EnviarMensagemAsync(string telefone, string mensagem);

    /// <summary>
    /// Verifica se o provedor está disponível e configurado
    /// </summary>
    /// <returns>True se disponível, False caso contrário</returns>
    Task<bool> EstaDisponivelAsync();

    /// <summary>
    /// Obtém informações de status do provedor
    /// </summary>
    Task<object> ObterStatusAsync();
}
