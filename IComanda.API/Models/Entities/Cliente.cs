namespace IComanda.API.Models.Entities;

/// <summary>
/// Entidade Cliente baseada na tabela CLIENTES do banco de dados
/// </summary>
public class Cliente
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? CpfCnpj { get; set; }
    public string? RgIe { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Obs { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Endereco1 { get; set; }
    public string? Numero1 { get; set; }
    public string? Complemento1 { get; set; }
    public string? Bairro1 { get; set; }
    public string? Cidade1 { get; set; }
    public string? Uf1 { get; set; }
    public string? Email { get; set; }
    public string? Endereco2 { get; set; }
    public string? Numero2 { get; set; }
    public string? Complemento2 { get; set; }
    public string? Bairro2 { get; set; }
    public string? Cidade2 { get; set; }
    public string? Uf2 { get; set; }
    public decimal? TabelaPreco { get; set; }
    public int? Ativo { get; set; }
    public string? Email1 { get; set; }
    public string? Cep1 { get; set; }
    public string? Cep2 { get; set; }
    public string? Classificacao { get; set; }
    public string? Acesso { get; set; }
    public decimal? Limite { get; set; }
    public string? RgOrgao { get; set; }
    public string? RgEstado { get; set; }
    public DateTime? RgEmissao { get; set; }
    public string? TelFax { get; set; }
    public DateTime? DataCadastro { get; set; }
    public string? TipoProdutor { get; set; }
    public string? Ip { get; set; }
    public string? Incra { get; set; }
    public string? Nirf { get; set; }
    public string? DeclaItr { get; set; }
    public string? Carro { get; set; }
    public string? PlacaCarro1 { get; set; }
    public string? PlacaCarro2 { get; set; }
    public string? PlacaCarro3 { get; set; }
    public string? Moto { get; set; }
    public string? PlacaMoto1 { get; set; }
    public string? PlacaMoto2 { get; set; }
    public string? PlacaMoto3 { get; set; }
    public byte[]? Foto { get; set; }
    public string? Cei { get; set; }
    public string? NumProprio { get; set; }
    public string? Fantasia { get; set; }
    public string? Cnae { get; set; }
    public int IdVendedor { get; set; }
    public int IbgeUf { get; set; }
    public int IbgeMunicipio { get; set; }
    public short? Contribuinte { get; set; }
    public decimal? Mensal { get; set; }
    public string? Site { get; set; }
    public short Bloqueado { get; set; }
    public int IdRota { get; set; }
    public decimal? ValorAluguel { get; set; }
    public DateTime? VencAluguel { get; set; }
    public decimal? Comissao { get; set; }

    // Propriedades calculadas para facilitar o uso
    public bool IsAtivo => Ativo == 1;
    public bool IsBloqueado => Bloqueado == 1;
    public string NomeCompleto => !string.IsNullOrEmpty(Fantasia) ? $"{Nome} ({Fantasia})" : Nome ?? "";
    public string Documento => !string.IsNullOrEmpty(CpfCnpj) ? CpfCnpj : "";
    public string Contato => !string.IsNullOrEmpty(Celular) ? Celular : Telefone ?? "";
}
