namespace IComanda.API.Models.Entities;

/// <summary>
/// Registro de recebimento/pagamento de comanda
/// </summary>
public class Recebimento
{
    public int Id { get; set; }
    public int ComandaId { get; set; }
    public decimal Valor { get; set; }
    public FormaPagamento? Forma { get; set; }
    public DateTime Data { get; set; }
    public string? Status { get; set; } = "PENDENTE";
}
