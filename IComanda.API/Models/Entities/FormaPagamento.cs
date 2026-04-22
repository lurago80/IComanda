namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de forma de pagamento - tabela FORMA_PAGAMENTO
/// </summary>
public class FormaPagamento
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public short Ativo { get; set; }
    public int? Indice { get; set; }
    public short Moeda { get; set; } = 0;
    public short MeioPagto { get; set; } = 0;
    public short PermiteTroco { get; set; } = 0;
    public string? Tipo { get; set; }

    // Propriedades auxiliares
    public bool IsAtivo => Ativo == 1;
    public bool IsCarteira => !string.IsNullOrWhiteSpace(Descricao) && 
                              Descricao.Trim().ToUpper() == "CARTEIRA";
    public bool PermiteTrocoAtivo => PermiteTroco == 1;
}

