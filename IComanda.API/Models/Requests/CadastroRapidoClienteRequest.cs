namespace IComanda.API.Models.Requests;

/// <summary>
/// Request para cadastro rápido de cliente na abertura de comanda
/// </summary>
public class CadastroRapidoClienteRequest
{
    /// <summary>
    /// Nome completo do cliente (obrigatório)
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF ou CNPJ do cliente (opcional, usado para verificar duplicidade quando informado)
    /// </summary>
    public string CpfCnpj { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do cliente (opcional, usado para verificar duplicidade quando informado)
    /// </summary>
    public string Telefone { get; set; } = string.Empty;

    /// <summary>
    /// Celular do cliente (opcional)
    /// </summary>
    public string? Celular { get; set; }

    /// <summary>
    /// Nome fantasia/apelido (opcional)
    /// </summary>
    public string? Fantasia { get; set; }

    /// <summary>
    /// Endereço (logradouro) para delivery
    /// </summary>
    public string? Endereco1 { get; set; }

    /// <summary>
    /// Número do endereço
    /// </summary>
    public string? Numero1 { get; set; }

    /// <summary>
    /// Complemento do endereço
    /// </summary>
    public string? Complemento1 { get; set; }

    /// <summary>
    /// Bairro
    /// </summary>
    public string? Bairro1 { get; set; }

    /// <summary>
    /// Cidade
    /// </summary>
    public string? Cidade1 { get; set; }

    /// <summary>
    /// UF
    /// </summary>
    public string? Uf1 { get; set; }

    /// <summary>
    /// CEP
    /// </summary>
    public string? Cep1 { get; set; }

    /// <summary>
    /// Se true, cliente fica ativo no cadastro (visível na lista de clientes).
    /// Se false, cliente é criado com Ativo=0 (só para esta venda/delivery).
    /// </summary>
    public bool GravarNoCadastro { get; set; } = true;
}

