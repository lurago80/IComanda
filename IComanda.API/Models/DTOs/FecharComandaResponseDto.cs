namespace IComanda.API.Models.DTOs;

/// <summary>
/// Response do fechamento de comanda
/// </summary>
public class FecharComandaResponseDto
{
    /// <summary>
    /// Nota da venda fechada
    /// </summary>
    public string Nota { get; set; } = string.Empty;

    /// <summary>
    /// Total da venda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Total recebido
    /// </summary>
    public decimal TotalRecebido { get; set; }

    /// <summary>
    /// Troco total
    /// </summary>
    public decimal Troco { get; set; }

    /// <summary>
    /// Lista de recebimentos processados
    /// </summary>
    public List<RecebimentoVendasDto> Recebimentos { get; set; } = new();

    /// <summary>
    /// Indica se foi criado registro na tabela RECEBER (para pagamento a prazo)
    /// </summary>
    public bool CriadoReceber { get; set; }

    /// <summary>
    /// Número do registro criado em RECEBER (se aplicável)
    /// </summary>
    public string? NumeroReceber { get; set; }

    /// <summary>
    /// Informações sobre contas a receber em aberto do cliente (se houver)
    /// </summary>
    public ContasAbertoDto? ContasAberto { get; set; }
}

