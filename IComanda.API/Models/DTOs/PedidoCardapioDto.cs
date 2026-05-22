namespace IComanda.API.Models.DTOs;

public class PedidoCardapioRequest
{
    public int Mesa { get; set; }
    public string? NomeCliente { get; set; }
    public string? Observacao { get; set; }
    public List<ItemPedidoCardapioDto> Itens { get; set; } = [];
}

public class ItemPedidoCardapioDto
{
    /// <summary>ID do produto (0 para pizza montada)</summary>
    public int ProdutoId { get; set; }
    public string Descricao { get; set; } = "";
    public decimal Preco { get; set; }
    public int Quantidade { get; set; } = 1;
    public string? Observacao { get; set; }
}

public class PedidoCardapioResponse
{
    public string Nota { get; set; } = "";
    public string Mensagem { get; set; } = "";
}
