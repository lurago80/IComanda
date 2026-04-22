namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para comanda aberta
/// </summary>
public class ComandaAbertaDto
{
    public int Numero { get; set; }
    public string Nota { get; set; } = string.Empty;
    public int Cliente { get; set; }
    public decimal Total { get; set; }
    public DateTime DataAbertura { get; set; }
    public int Operador { get; set; }
    public int? Mesa { get; set; }
    public int QuantidadeItens { get; set; }
}

