namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para cadastro rápido de cliente na abertura de comanda
/// </summary>
public class CadastroRapidoClienteRequest
{
    /// <summary>
    /// Nome completo do cliente (obrigatório)
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF ou CNPJ do cliente (obrigatório, usado para verificar duplicidade)
    /// </summary>
    public string CpfCnpj { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do cliente (obrigatório, usado para verificar duplicidade)
    /// </summary>
    public string Telefone { get; set; } = string.Empty;

    /// <summary>
    /// Celular do cliente (opcional)
    /// </summary>
    public string? Celular { get; set; }

    /// <summary>
    /// Nome fantasia/apelido (opcional)
    /// </summary>
    public string? Fantasia { get; set; }
}

