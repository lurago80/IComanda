namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para representar um cliente
/// </summary>
public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Endereco1 { get; set; }
    public string? Cidade1 { get; set; }
    public string? Uf1 { get; set; }
    public string? Cep1 { get; set; }
    public string? Fantasia { get; set; }
    public bool Ativo { get; set; }
    public bool Bloqueado { get; set; }
    public DateTime? DataCadastro { get; set; }
    public decimal? Limite { get; set; }
    public string? Classificacao { get; set; }
    public int IdVendedor { get; set; }

    // Propriedades calculadas
    public string NomeCompleto => !string.IsNullOrEmpty(Fantasia) ? $"{Nome} ({Fantasia})" : Nome;
    public string Documento => !string.IsNullOrEmpty(CpfCnpj) ? CpfCnpj : "";
    public string Contato => !string.IsNullOrEmpty(Celular) ? Celular : Telefone ?? "";
    public string EnderecoCompleto => $"{Endereco1}, {Cidade1}/{Uf1}".TrimEnd(',', '/', ' ');
}
