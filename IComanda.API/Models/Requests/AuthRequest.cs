namespace IComanda.API.Models.Requests;

/// <summary>
/// Requisição de login para autenticação
/// </summary>
public class LoginRequest
{
    /// <summary>Nome de usuário ou ID</summary>
    public string Username { get; set; } = "";
    
    /// <summary>Senha do usuário</summary>
    public string Password { get; set; } = "";
}
