namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para histórico de alterações (auditoria)
/// </summary>
public class HistoricoAlteracaoDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // VENDA, ITEM, COMANDA, MESA, etc
    public string EntidadeId { get; set; } = string.Empty; // ID da entidade alterada
    public string Acao { get; set; } = string.Empty; // CRIAR, ATUALIZAR, CANCELAR, DELETAR
    public int Operador { get; set; }
    public string? NomeOperador { get; set; }
    public DateTime DataHora { get; set; }
    public string? Descricao { get; set; }
    public string? DadosAntigos { get; set; }
    public string? DadosNovos { get; set; }
    public string? Observacoes { get; set; }
}

