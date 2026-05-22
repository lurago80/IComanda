namespace IComanda.API.Models.Requests;

public class CreateUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public bool Bloqueado { get; set; } = false;
    public bool PodeVisualizar { get; set; } = false;
    public bool PodeVerTotal { get; set; } = true;
    public bool PodeCancelar { get; set; } = false;
    public string Tipo { get; set; } = "0";
}

public class UpdateUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public bool Bloqueado { get; set; }
    public bool PodeVisualizar { get; set; }
    public bool PodeVerTotal { get; set; }
    public bool PodeCancelar { get; set; }
    public string Tipo { get; set; } = "0";
}

public class AlterarSenhaRequest
{
    public string NovaSenha { get; set; } = string.Empty;
}
