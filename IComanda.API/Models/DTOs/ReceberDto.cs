namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para conta a receber
/// </summary>
public class ReceberDto
{
    public string Numero { get; set; } = string.Empty;
    public string Ordem { get; set; } = string.Empty;
    public int Codigo { get; set; }
    public string? NomeCliente { get; set; }
    public string? TelefoneCliente { get; set; }
    public char Tipo { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string? Historico { get; set; }
    public DateTime Emissao { get; set; }
    public DateTime Vencimento { get; set; }
    public decimal Valor { get; set; }
    public DateTime? Recebimento { get; set; }
    public decimal ValorRecebido { get; set; }
    public decimal ValorPendente => Valor - ValorRecebido;
    public bool EstaQuitado => ValorRecebido >= Valor;
    public decimal Acrescimo { get; set; }
    public decimal Desconto { get; set; }
    public decimal Juros { get; set; }
    public int? Operador { get; set; }
    public string? Especie { get; set; }
    public string? ControleNota { get; set; }
    public string? NotaFiscal { get; set; }
    public int DiasVencidos => EstaQuitado ? 0 : (DateTime.Now - Vencimento).Days;
}

