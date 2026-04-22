namespace IComanda.API.Models.Requests;

/// <summary>Request para alterar a senha de um vendedor</summary>
public class AlterarSenhaVendedorRequest
{
    public string NovaSenha { get; set; } = string.Empty;
}
