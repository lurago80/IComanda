namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para notificações/alertas
/// </summary>
public class NotificacaoDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // ALERTA, AVISO, INFO
    public string Categoria { get; set; } = string.Empty; // COMANDA, MESA, ESTOQUE, FINANCEIRO
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public bool Lida { get; set; }
    public string? EntidadeId { get; set; }
    public string? EntidadeTipo { get; set; }
    public int? Prioridade { get; set; } // 1 = Baixa, 2 = Média, 3 = Alta
}

