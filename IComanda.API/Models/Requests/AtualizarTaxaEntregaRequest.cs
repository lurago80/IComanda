namespace IComanda.API.Models.Requests;

public class AtualizarTaxaEntregaRequest
{
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}
