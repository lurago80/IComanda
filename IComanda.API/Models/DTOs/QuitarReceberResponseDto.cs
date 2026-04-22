namespace IComanda.API.Models.DTOs;

/// <summary>
/// Response do quitamento de conta a receber
/// </summary>
public class QuitarReceberResponseDto
{
    /// <summary>
    /// Número da conta
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Ordem da conta
    /// </summary>
    public string Ordem { get; set; } = string.Empty;

    /// <summary>
    /// Valor original da conta
    /// </summary>
    public decimal ValorOriginal { get; set; }

    /// <summary>
    /// Valor recebido agora
    /// </summary>
    public decimal ValorRecebido { get; set; }

    /// <summary>
    /// Valor total já recebido (incluindo este recebimento)
    /// </summary>
    public decimal ValorTotalRecebido { get; set; }

    /// <summary>
    /// Valor ainda pendente
    /// </summary>
    public decimal ValorPendente { get; set; }

    /// <summary>
    /// Indica se a conta foi totalmente quitada
    /// </summary>
    public bool TotalmenteQuitado { get; set; }

    /// <summary>
    /// Data do recebimento
    /// </summary>
    public DateTime DataRecebimento { get; set; }

    /// <summary>
    /// Informações sobre outras contas a receber em aberto do cliente (se houver)
    /// </summary>
    public ContasAbertoDto? ContasAberto { get; set; }
}

