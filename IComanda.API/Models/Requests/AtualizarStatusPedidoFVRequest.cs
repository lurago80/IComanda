using System.ComponentModel.DataAnnotations;

namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para atualizar o status de um Pedido FV
/// </summary>
public class AtualizarStatusPedidoFVRequest
{
    /// <summary>0=Pendente, 1=Aprovado, 2=Faturado, 3=Cancelado</summary>
    [Required]
    [Range(0, 3)]
    public int Status { get; set; }

    /// <summary>Motivo obrigatório ao cancelar</summary>
    public string? Motivo { get; set; }

    /// <summary>Número da nota fiscal ao faturar</summary>
    public string? NotaFiscal { get; set; }

    /// <summary>ID do usuário que está aprovando/recusando</summary>
    public int? IdAprovador { get; set; }
}
