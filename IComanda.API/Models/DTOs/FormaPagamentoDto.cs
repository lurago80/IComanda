namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para forma de pagamento
/// </summary>
public class FormaPagamentoDto
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public int? Indice { get; set; }
    public short Moeda { get; set; }
    public short MeioPagto { get; set; }
    public bool PermiteTroco { get; set; }
    public string? Tipo { get; set; }
}

