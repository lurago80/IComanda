namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade para tabela frente_tmpitvendas
/// Itens temporários da venda - serão finalizados pelo Delphi Desktop
/// </summary>
public class ItemVendaTemporario
{
    public string Cupom { get; set; } = string.Empty;
    public string NCaixa { get; set; } = "1";
    public DateTime Data { get; set; } = DateTime.Now;
    public TimeSpan Hora { get; set; } = DateTime.Now.TimeOfDay;
    public int Operador { get; set; }
    public int Item { get; set; }
    public int Codigo { get; set; }
    public string? Barras { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Qtd { get; set; }
    public decimal Preco { get; set; }
    public string? Tributacao { get; set; }
    public decimal Icms { get; set; }
    public decimal Iss { get; set; }
    public string? Und { get; set; }
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public decimal Total { get; set; }
    public string? Serial { get; set; }
    public string? Observacao { get; set; } // Usando campo Serial para observações temporariamente
    public int Tipo { get; set; } = 1; // Tipo 1 = item de venda normal
}

