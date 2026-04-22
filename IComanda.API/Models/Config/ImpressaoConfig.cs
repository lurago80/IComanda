namespace IComanda.API.Models.Config;

public class ImpressaoConfig
{
    public string Tipo { get; set; } = "Rede"; // "Rede" ou "Local"
    public ImpressoraRedeConfig ImpressoraRede { get; set; } = new();
    public ImpressoraLocalConfig ImpressoraLocal { get; set; } = new();
}

public class ImpressoraRedeConfig
{
    public string IP { get; set; } = "192.168.1.100";
    public int Porta { get; set; } = 9100; // Porta padrão para impressoras térmicas (9100 = RAW, 515 = LPR)
    public string Nome { get; set; } = "Impressora Termica";
    public string? CaminhoUNC { get; set; } // Caminho UNC para impressora compartilhada (ex: \\192.168.0.150\MP-2500 TH)
    public int LarguraPapel { get; set; } = 80; // Largura em mm (80mm = cupom padrão)
    public int Timeout { get; set; } = 5000; // Timeout em milissegundos
}

public class ImpressoraLocalConfig
{
    public string Nome { get; set; } = ""; // Nome da impressora no Windows
    public int LarguraPapel { get; set; } = 80; // Largura em mm
}

public class EmpresaConfig
{
    public string Nome { get; set; } = "";
    public string? NomeFantasia { get; set; }
    public string? Cnpj { get; set; }
    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Cep { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Site { get; set; }
}

