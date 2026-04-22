namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para excluir (cancelar) uma comanda.
/// A senha é validada contra o campo SENHAD da tabela PARAMETROS.
/// </summary>
public class ExcluirComandaRequest
{
    /// <summary>Justificativa obrigatória para o cancelamento.</summary>
    public string Justificativa { get; set; } = string.Empty;

    /// <summary>Senha de cancelamento (campo SENHAD da tabela PARAMETROS).</summary>
    public string Senha { get; set; } = string.Empty;
}
