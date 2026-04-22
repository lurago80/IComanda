namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para buscar/filtrar vendedores
/// </summary>
public class BuscarVendedorRequest
{
    public string? Q { get; set; }
    public bool? Ativo { get; set; }
    public string? Regiao { get; set; }
    public int? Pagina { get; set; }
    public int? ItensPorPagina { get; set; }
}
