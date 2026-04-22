namespace IComanda.API.Models.Enums;

/// <summary>
/// Status do pedido de Força de Vendas
/// </summary>
public enum StatusPedidoFV
{
    /// <summary>Pedido criado pelo vendedor, aguardando aprovação</summary>
    Pendente = 0,

    /// <summary>Pedido aprovado pelo gestor, pode ser faturado</summary>
    Aprovado = 1,

    /// <summary>Pedido faturado/integrado ao sistema</summary>
    Faturado = 2,

    /// <summary>Pedido cancelado</summary>
    Cancelado = 3
}
