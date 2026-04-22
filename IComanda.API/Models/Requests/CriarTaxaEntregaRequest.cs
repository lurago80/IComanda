namespace IComanda.API.Models.Requests;

public class CriarTaxaEntregaRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
