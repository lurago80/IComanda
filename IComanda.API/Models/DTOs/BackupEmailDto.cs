namespace IComanda.API.Models.DTOs;

public class BackupEmailDto
{
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseSsl { get; set; } = true;
    public string Usuario { get; set; } = "";
    /// <summary>Senha SMTP. Se null ou vazia, mantém a senha salva anteriormente.</summary>
    public string? Senha { get; set; }
    public string Remetente { get; set; } = "";
    public string NomeRemetente { get; set; } = "IComanda Backup";
    public string Destinatario { get; set; } = "";
}
