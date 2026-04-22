using System.Linq;

namespace IComanda.API.Models.DTOs;

/// <summary>
/// DTO para representar um cliente
/// </summary>
public class ClienteDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? RgIe { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? Email1 { get; set; }
    public string? Endereco1 { get; set; }
    public string? Numero1 { get; set; }
    public string? Complemento1 { get; set; }
    public string? Bairro1 { get; set; }
    public string? Cidade1 { get; set; }
    public string? Uf1 { get; set; }
    public string? Cep1 { get; set; }
    public string? Endereco2 { get; set; }
    public string? Numero2 { get; set; }
    public string? Complemento2 { get; set; }
    public string? Bairro2 { get; set; }
    public string? Cidade2 { get; set; }
    public string? Uf2 { get; set; }
    public string? Cep2 { get; set; }
    public string? Fantasia { get; set; }
    public bool Ativo { get; set; }
    public bool Bloqueado { get; set; }
    public DateTime? DataCadastro { get; set; }
    public DateTime? DataNascimento { get; set; }
    public decimal? Limite { get; set; }
    public string? Classificacao { get; set; }
    public string? Obs { get; set; }
    public int IdVendedor { get; set; }

    // Propriedades calculadas
    public string NomeCompleto => !string.IsNullOrEmpty(Fantasia) ? $"{Nome} ({Fantasia})" : Nome;
    public string Documento => !string.IsNullOrEmpty(CpfCnpj) ? CpfCnpj : "";
    public string Contato => !string.IsNullOrEmpty(Celular) ? Celular : Telefone ?? "";
    
    // Endereço completo formatado
    public string EnderecoCompleto
    {
        get
        {
            var partes = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(Endereco1))
            {
                var endereco = Endereco1;
                if (!string.IsNullOrWhiteSpace(Numero1))
                    endereco += $", {Numero1}";
                if (!string.IsNullOrWhiteSpace(Complemento1))
                    endereco += $" - {Complemento1}";
                partes.Add(endereco);
            }
            
            if (!string.IsNullOrWhiteSpace(Bairro1))
                partes.Add(Bairro1);
            
            if (!string.IsNullOrWhiteSpace(Cidade1))
            {
                var cidadeUf = Cidade1;
                if (!string.IsNullOrWhiteSpace(Uf1))
                    cidadeUf += $"/{Uf1}";
                partes.Add(cidadeUf);
            }
            else if (!string.IsNullOrWhiteSpace(Uf1))
            {
                partes.Add(Uf1);
            }
            
            return string.Join(" - ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
    }
}
