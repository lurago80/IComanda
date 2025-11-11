namespace IComanda.API.Models.DTOs;

/// <summary>
/// Response para verificação de cliente existente por CPF ou Telefone
/// </summary>
public class VerificarClienteResponse
{
    /// <summary>
    /// Indica se o cliente existe no banco de dados
    /// </summary>
    public bool Existe { get; set; }

    /// <summary>
    /// Dados do cliente encontrado (null se não existir)
    /// </summary>
    public ClienteDto? Cliente { get; set; }

    /// <summary>
    /// Mensagem informativa
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;
}

