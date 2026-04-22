namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de receber (contas a receber) - tabela RECEBER
/// </summary>
public class Receber
{
    public string Numero { get; set; } = string.Empty;
    public string Ordem { get; set; } = string.Empty;
    public int Codigo { get; set; }
    public char Tipo { get; set; }
    public string Modelo { get; set; } = "PP";
    public string Serie { get; set; } = "PPP";
    public string Subserie { get; set; } = "PP";
    public string Origem { get; set; } = "PP";
    public string? Historico { get; set; }
    public DateTime Emissao { get; set; } = DateTime.Now;
    public DateTime Vencimento { get; set; } = DateTime.Now;
    public decimal Valor { get; set; } = 0;
    public DateTime? Recebimento { get; set; }
    public decimal ValorRecebido { get; set; } = 0;
    public decimal Acrescimo { get; set; } = 0;
    public decimal Desconto { get; set; } = 0;
    public short Fixo { get; set; } = 1;
    public decimal Juros { get; set; } = 0;
    public int Controle { get; set; } = 0;
    public int Operador { get; set; } = 0;
    public string? Especie { get; set; }
    public short Banco { get; set; } = 0;
    public int? Conta { get; set; }
    public int? Custo { get; set; }
    public string? AgConta { get; set; }
    public string? AgBanco { get; set; }
    public short QuitadoNoEcf { get; set; } = 0;
    public decimal? Dinheiro { get; set; }
    public decimal? Cheque { get; set; }
    public decimal? Cartao { get; set; }
    public decimal? Boleto { get; set; }
    public decimal? Troco { get; set; }
    public int? IdVendedor { get; set; }
    public string? NumChq { get; set; }
    public int? CodProf { get; set; }
    public decimal? CartaoD { get; set; }
    public decimal? ValorOriginal { get; set; }
    public decimal Pix { get; set; } = 0;
    public TimeSpan? HoraReceb { get; set; }
    public char Impresso { get; set; } = '0';
    public char Remessa { get; set; } = '0';
    public decimal Qrcode { get; set; } = 0;
    public string ControleNota { get; set; } = "0";
    public int Terminal { get; set; } = 0;
    public string? NotaFiscal { get; set; }
}

