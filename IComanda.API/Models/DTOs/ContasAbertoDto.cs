namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para informações de contas a receber em aberto do cliente
/// </summary>
public class ContasAbertoDto
{
    public bool TemContasAberto { get; set; }
    public decimal ValorTotalPendente { get; set; }
    public int QuantidadeContas { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}
