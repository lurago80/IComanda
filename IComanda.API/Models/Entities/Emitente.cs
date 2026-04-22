namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade Emitente baseada na tabela EMITENTE do banco de dados
/// </summary>
public class Emitente
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? NomeFantasia { get; set; }
    public string? Cnpj { get; set; }
    public string? InscricaoEstadual { get; set; }
    public string? Endereco { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Site { get; set; }
}

