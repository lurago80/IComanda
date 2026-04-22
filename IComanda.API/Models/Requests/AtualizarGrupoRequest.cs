namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para atualizar um grupo existente
/// </summary>
public class AtualizarGrupoRequest
{
    /// <summary>
    /// Descrição do grupo
    /// </summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o grupo deve imprimir 2 vias automaticamente (se habilitado nas configurações)
    /// </summary>
    public bool ImprimirDuasVias { get; set; } = false;
}
