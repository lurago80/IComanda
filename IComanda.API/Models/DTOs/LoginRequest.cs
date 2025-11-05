using System.ComponentModel.DataAnnotations;

namespace IComanda.API.Models.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "O nome de usuário é obrigatório")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Senha { get; set; } = string.Empty;
}

