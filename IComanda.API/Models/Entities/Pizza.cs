namespace IComanda.API.Models.Entities;

public class PizzaTamanho
{
    public int Id { get; set; }
    public int GrupoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public List<PizzaSabor> Sabores { get; set; } = new();
}

public class PizzaSabor
{
    public int Id { get; set; }
    public int TamanhoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string? Ingredientes { get; set; }
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
}

public class PizzaBorda
{
    public int Id { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
}
