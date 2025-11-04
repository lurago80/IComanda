namespace IComanda.API.Models.Entities;

/// <summary>
/// Modelo de venda - tabela VENDAS
/// </summary>
public class Venda
{
    public string Nota { get; set; } = string.Empty;
    public string Modelo { get; set; } = "D2";
    public string Serie { get; set; } = "001";
    public string Subserie { get; set; } = "01";
    public string Origem { get; set; } = "BA";
    public DateTime Emissao { get; set; } = DateTime.Now;
    public TimeSpan Hora { get; set; } = DateTime.Now.TimeOfDay;
    public char? Entrada { get; set; }
    public char? Saida { get; set; } = 'X';
    public string Cfops { get; set; } = "5.102";
    public string Natureza { get; set; } = "Venda de Mercadoria";
    public int Cliente { get; set; }
    public DateTime DataSaida { get; set; } = DateTime.Now;
    public TimeSpan HoraSaida { get; set; } = DateTime.Now.TimeOfDay;
    public string FormasPgto { get; set; } = "À VISTA";
    public decimal TotProdutos { get; set; }
    public decimal Total { get; set; }
    public int Operador { get; set; } = 1;
    public string Sequencia { get; set; } = string.Empty;
    public string Avista { get; set; } = "1";
    public decimal Desconto { get; set; }
    public decimal Acrescimo { get; set; }
    public string Especie { get; set; } = "DINHEIRO";
    public string Loja { get; set; } = "";
    public decimal Vale { get; set; }
    public decimal Dinheiro { get; set; }
    public decimal Cheque { get; set; }
    public decimal Cartao { get; set; }
    public decimal Boleto { get; set; }
    public decimal Troco { get; set; }
    public string Quantidade { get; set; } = string.Empty;
    public string Lancado { get; set; } = "EFETIVADO";
    public int Vendedor { get; set; } = 1;
    public int Caixa { get; set; } = 0;
    public int? Comanda { get; set; }
    public int? Mesa { get; set; }
    public int? NumeroPessoas { get; set; }
    public List<ItemVenda> Itens { get; set; } = new();
}
