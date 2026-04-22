using IComanda.API.Models.Config;
using IComanda.API.Models.Requests;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.Versioning;

namespace IComanda.API.Services.Implementations;

public class ImpressaoService : IImpressaoService
{
    /// <summary>Largura máxima em caracteres para cupom 80mm (35 evita corte).</summary>
    private const int LARGURA_80MM = 35;

    private readonly IConfiguration _configuration;
    private readonly ILogger<ImpressaoService> _logger;
    private readonly IEmitenteRepository _emitenteRepository;
    private ImpressaoConfig? _impressaoConfig;
    private EmpresaConfig? _empresaConfig;
    private Emitente? _emitenteCache;

    /// <summary>Garante que a linha não ultrapasse a largura do cupom 80mm.</summary>
    private static string LimitarLinha(string? texto, int max = LARGURA_80MM)
    {
        if (string.IsNullOrEmpty(texto)) return string.Empty;
        var s = texto.Trim();
        return s.Length <= max ? s : s.Substring(0, max);
    }

    /// <summary>
    /// Retorna as linhas de um campo com prefixo (ex: "CLIENTE: ", "PT. REF.: ") quebrando
    /// o valor em múltiplas linhas quando não cabe em uma só — sem cortar nenhum caractere.
    /// Linhas de continuação recebem 2 espaços de recuo.
    /// </summary>
    private List<string> QuebrarCampo(string prefixo, string? valor, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(valor)) return new List<string>();
        var full = prefixo + valor.Trim();
        var linhas = QuebrarDescricao(full, maxLen);
        for (int i = 1; i < linhas.Count; i++)
            linhas[i] = "  " + linhas[i]; // recuo para linhas de continuação
        return linhas;
    }

    public ImpressaoService(
        IConfiguration configuration, 
        ILogger<ImpressaoService> logger,
        IEmitenteRepository emitenteRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _emitenteRepository = emitenteRepository;
        CarregarConfiguracao();
    }

    private void CarregarConfiguracao()
    {
        _impressaoConfig = _configuration.GetSection("Impressao").Get<ImpressaoConfig>();
        if (_impressaoConfig == null)
        {
            _logger.LogWarning("⚠️ Configuração de impressão não encontrada. Usando valores padrão.");
            _impressaoConfig = new ImpressaoConfig();
        }

        _empresaConfig = _configuration.GetSection("Empresa").Get<EmpresaConfig>();
        if (_empresaConfig == null)
        {
            _logger.LogWarning("⚠️ Configuração de empresa não encontrada.");
            _empresaConfig = new EmpresaConfig();
        }

        // Compatibilidade com variáveis de ambiente legadas (sem seção aninhada)
        var tipoEnv = Environment.GetEnvironmentVariable("IMPRESSORA_TIPO");
        if (!string.IsNullOrWhiteSpace(tipoEnv))
            _impressaoConfig!.Tipo = tipoEnv.Trim();

        var ipEnv = Environment.GetEnvironmentVariable("IMPRESSORA_IP");
        if (!string.IsNullOrWhiteSpace(ipEnv))
            _impressaoConfig!.ImpressoraRede.IP = ipEnv.Trim();

        var portaEnv = Environment.GetEnvironmentVariable("IMPRESSORA_PORTA");
        if (int.TryParse(portaEnv, out var porta) && porta > 0)
            _impressaoConfig!.ImpressoraRede.Porta = porta;

        var nomeEnv = Environment.GetEnvironmentVariable("IMPRESSORA_NOME");
        if (!string.IsNullOrWhiteSpace(nomeEnv))
        {
            var nome = nomeEnv.Trim();
            _impressaoConfig!.ImpressoraRede.Nome = nome;
            _impressaoConfig.ImpressoraLocal.Nome = nome;
        }

        var uncEnv = Environment.GetEnvironmentVariable("IMPRESSORA_CAMINHO_UNC");
        if (!string.IsNullOrWhiteSpace(uncEnv))
            _impressaoConfig!.ImpressoraRede.CaminhoUNC = uncEnv.Trim();

        var timeoutEnv = Environment.GetEnvironmentVariable("IMPRESSORA_TIMEOUT");
        if (int.TryParse(timeoutEnv, out var timeout) && timeout > 0)
            _impressaoConfig!.ImpressoraRede.Timeout = timeout;

        var larguraEnv = Environment.GetEnvironmentVariable("IMPRESSORA_LARGURA");
        if (int.TryParse(larguraEnv, out var largura) && largura > 0)
        {
            _impressaoConfig!.ImpressoraRede.LarguraPapel = largura;
            _impressaoConfig.ImpressoraLocal.LarguraPapel = largura;
        }

        _logger.LogInformation(
            "🧾 Configuração de impressão carregada: Tipo={Tipo}; Rede={IP}:{Porta}; UNC={UNC}; Local={Local}",
            _impressaoConfig!.Tipo,
            _impressaoConfig.ImpressoraRede.IP,
            _impressaoConfig.ImpressoraRede.Porta,
            _impressaoConfig.ImpressoraRede.CaminhoUNC ?? "(vazio)",
            _impressaoConfig.ImpressoraLocal.Nome ?? "(vazio)");
    }

    private async Task<EmpresaConfig> ObterDadosEmpresaAsync()
    {
        // Sempre buscar do banco de dados (não usar cache para garantir dados atualizados)
        _logger.LogInformation("🔍 Buscando dados do emitente no banco de dados...");
        _emitenteCache = await _emitenteRepository.GetEmitenteAsync();

        if (_emitenteCache != null)
        {
            _logger.LogInformation("✅ Usando dados do emitente do banco de dados");
            _logger.LogInformation("   Nome: {Nome}", _emitenteCache.Nome ?? "(vazio)");
            _logger.LogInformation("   NomeFantasia: {NomeFantasia}", _emitenteCache.NomeFantasia ?? "(vazio)");
            _logger.LogInformation("   CNPJ: {Cnpj}", _emitenteCache.Cnpj ?? "(vazio)");
            _logger.LogInformation("   Cidade: {Cidade}", _emitenteCache.Cidade ?? "(vazio)");
            
            // Montar endereço completo: ENDER + NUMEND + BAIRRO
            var enderecoCompleto = _emitenteCache.Endereco ?? "";
            if (!string.IsNullOrWhiteSpace(_emitenteCache.Numero))
            {
                enderecoCompleto = string.IsNullOrWhiteSpace(enderecoCompleto) 
                    ? _emitenteCache.Numero 
                    : $"{enderecoCompleto}, {_emitenteCache.Numero}";
            }
            if (!string.IsNullOrWhiteSpace(_emitenteCache.Bairro))
            {
                enderecoCompleto = string.IsNullOrWhiteSpace(enderecoCompleto)
                    ? _emitenteCache.Bairro
                    : $"{enderecoCompleto} - {_emitenteCache.Bairro}";
            }
            
            // Montar cidade completa: CIDADE + UF (CEP separado para evitar duplicar na impressão)
            var cidadeCompleta = _emitenteCache.Cidade ?? "";
            if (!string.IsNullOrWhiteSpace(_emitenteCache.Uf))
            {
                cidadeCompleta = string.IsNullOrWhiteSpace(cidadeCompleta)
                    ? _emitenteCache.Uf
                    : $"{cidadeCompleta} - {_emitenteCache.Uf}";
            }
            
            return new EmpresaConfig
            {
                Nome = _emitenteCache.Nome ?? "",
                NomeFantasia = _emitenteCache.NomeFantasia,
                Cnpj = _emitenteCache.Cnpj,
                Endereco = enderecoCompleto,
                Bairro = _emitenteCache.Bairro,
                Cidade = cidadeCompleta,
                Cep = _emitenteCache.Cep,
                Telefone = _emitenteCache.Telefone,
                Email = _emitenteCache.Email,
                Site = _emitenteCache.Site
            };
        }

        // Fallback para configuração do appsettings.json
        _logger.LogWarning("⚠️ Nenhum emitente encontrado no banco. Usando dados da configuração do appsettings.json (fallback)");
        return _empresaConfig ?? new EmpresaConfig();
    }

    public async Task<bool> ImprimirPedidoAsync(string nota, ImprimirPedidoRequest request)
    {
        try
        {
            _logger.LogInformation("🖨️ Iniciando impressão do pedido {Nota} - Apenas novos itens: {ApenasNovosItens}", 
                nota, request.ApenasNovosItens);

            // Buscar dados da empresa (do banco ou configuração)
            var empresaData = await ObterDadosEmpresaAsync();

            // Gerar cupom em dois formatos:
            // - ESC/POS: para TCP/IP direto (porta 9100)
            // - Texto simples: para spooler Windows (UNC ou local) sem códigos de controle
            var cupomEscPos = GerarCupomEscPos(nota, request, empresaData);
            var cupomTexto = GerarCupomTexto(nota, request, empresaData);

            var config = _impressaoConfig ?? new ImpressaoConfig();
            var tipoImpressao = (config.Tipo ?? string.Empty).Trim();

            // Número de vias: 2 quando solicitado, senão 1
            var numeroVias = request.ImprimirDuasVias ? 2 : 1;
            if (numeroVias > 1)
                _logger.LogInformation("🖨️ Imprimindo {Vias} vias do pedido {Nota}", numeroVias, nota);

            bool sucesso = false;
            for (int via = 1; via <= numeroVias; via++)
            {
                if (numeroVias > 1)
                    _logger.LogInformation("🖨️ Imprimindo via {Via}/{Total}", via, numeroVias);

                if (tipoImpressao.Equals("Rede", StringComparison.OrdinalIgnoreCase))
                {
                    // 1) Se houver UNC, tenta primeiro via spooler do Windows
                    if (!string.IsNullOrEmpty(config.ImpressoraRede.CaminhoUNC))
                    {
#pragma warning disable CA1416 // Validar compatibilidade de plataforma
                        var sucessoUNC = await ImprimirWindowsUNCAsync(cupomTexto);
#pragma warning restore CA1416
                        if (sucessoUNC)
                        {
                            sucesso = true;
                            continue;
                        }

                        _logger.LogWarning("⚠️ Impressão via UNC falhou, tentando TCP/IP direto...");
                    }

                    // 2) Fallback: TCP/IP direto (porta 9100)
                    sucesso = await ImprimirRedeAsync(cupomEscPos);
                }
                else if (tipoImpressao.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
#pragma warning disable CA1416 // Validar compatibilidade de plataforma
                    sucesso = await ImprimirWindowsLocalAsync(cupomTexto);
#pragma warning restore CA1416
                }
                else
                {
                    _logger.LogWarning("⚠️ Tipo de impressão não configurado ou inválido: {Tipo}", _impressaoConfig?.Tipo);
                    return false;
                }

                if (!sucesso)
                    break;
            }

            return sucesso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao imprimir pedido {Nota}", nota);
            return false;
        }
    }

    private string GerarCupomTexto(string nota, ImprimirPedidoRequest request, EmpresaConfig empresaData)
    {
        // Pedido normal (comanda em aberto): usar formato igual ao WhatsApp
        if (!request.IsExtrato && !request.IsReciboCliente)
            return GerarCupomFormatoComandaTexto(nota, request, empresaData);

        var sb = new StringBuilder();
        int colunas = LARGURA_80MM;

        // Cabeçalho profissional para extrato
        if (request.IsExtrato)
        {
            sb.AppendLine();
            sb.AppendLine(new string('=', colunas));
            
            // Nome principal (apenas fantasia; se não houver, usa razão social)
            var nomeHeader = !string.IsNullOrWhiteSpace(empresaData?.NomeFantasia)
                ? empresaData.NomeFantasia
                : empresaData?.Nome;
            if (!string.IsNullOrWhiteSpace(nomeHeader))
            {
                var nomeCentralizado = LimitarLinha(nomeHeader, colunas).ToUpper();
                var espacos = (colunas - nomeCentralizado.Length) / 2;
                sb.AppendLine(new string(' ', espacos) + nomeCentralizado);
            }
            
            sb.AppendLine();
            
            // CNPJ
            if (!string.IsNullOrWhiteSpace(empresaData?.Cnpj))
            {
                var cnpj = $"CNPJ: {empresaData.Cnpj}";
                var espacos = (colunas - cnpj.Length) / 2;
                sb.AppendLine(new string(' ', espacos) + cnpj);
            }
            
            // Endereço completo
            if (!string.IsNullOrWhiteSpace(empresaData?.Endereco))
            {
                var endereco = LimitarLinha(empresaData.Endereco + (string.IsNullOrWhiteSpace(empresaData.Bairro) ? "" : $" - {empresaData.Bairro}"), colunas);
                var espacos = (colunas - endereco.Length) / 2;
                sb.AppendLine(new string(' ', espacos) + endereco);
            }
            
            if (!string.IsNullOrWhiteSpace(empresaData?.Cidade))
            {
                var cidade = LimitarLinha(empresaData.Cidade + (string.IsNullOrWhiteSpace(empresaData.Cep) ? "" : $" - CEP: {empresaData.Cep}"), colunas);
                var espacos = (colunas - cidade.Length) / 2;
                sb.AppendLine(new string(' ', espacos) + cidade);
            }
            
            // Telefone e Email
            if (!string.IsNullOrWhiteSpace(empresaData?.Telefone))
            {
                var telefone = LimitarLinha($"Tel: {empresaData.Telefone}", colunas);
                var espacos = (colunas - telefone.Length) / 2;
                sb.AppendLine(new string(' ', espacos) + telefone);
            }
            
            if (!string.IsNullOrWhiteSpace(empresaData?.Email))
            {
                var email = LimitarLinha(empresaData.Email, colunas);
                var espacos = (colunas - email.Length) / 2;
                sb.AppendLine(new string(' ', espacos) + email);
            }
            
            sb.AppendLine(new string('=', colunas));
            sb.AppendLine();
            
            // Título do extrato
            var titulo = LimitarLinha("EXTRATO DE COMANDA", colunas);
            var espacosTitulo = (colunas - titulo.Length) / 2;
            sb.AppendLine(new string(' ', espacosTitulo) + titulo);
            sb.AppendLine();
            
            // Número do extrato e data/hora
            sb.AppendLine(LimitarLinha($"EXTRATO: #{nota.PadLeft(6, '0')}", colunas));
            sb.AppendLine(LimitarLinha($"DATA/HORA: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", colunas));
            sb.AppendLine(new string('-', colunas));
        }
        else
        {
            if (request.IsReciboCliente)
            {
                var tituloRecibo = "RECIBO DO CLIENTE";
                var espacosRecibo = (colunas - tituloRecibo.Length) / 2;
                sb.AppendLine(new string(' ', Math.Max(0, espacosRecibo)) + tituloRecibo);
            }
            sb.AppendLine(LimitarLinha($"PEDIDO #{nota.PadLeft(6, '0')}", colunas));
            sb.AppendLine(LimitarLinha(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), colunas));
            sb.AppendLine(new string('=', colunas));
        }

        // Informações da comanda/mesa/cliente
        if (!string.IsNullOrEmpty(request.Comanda))
            sb.AppendLine(LimitarLinha($"COMANDA: {request.Comanda}", colunas));
        if (!string.IsNullOrEmpty(request.Mesa))
            sb.AppendLine(LimitarLinha($"MESA: {request.Mesa}", colunas));
        if (!string.IsNullOrEmpty(request.ClienteNome))
            sb.AppendLine(LimitarLinha($"CLIENTE: {request.ClienteNome}", colunas));

        if (!request.IsExtrato)
        {
            sb.AppendLine(new string('-', colunas));
            sb.AppendLine(LimitarLinha(request.ApenasNovosItens ? "NOVOS ITENS ADICIONADOS" : "ITENS DO PEDIDO", colunas));
            sb.AppendLine(new string('-', colunas));
        }
        else
        {
            sb.AppendLine();
        }

        var ptBR = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        decimal total = 0;
        foreach (var item in request.Itens)
        {
            var precoItem = item.Preco * item.Quantidade;
            total += precoItem;
            var valor = $"R$ {precoItem.ToString("F2", ptBR)}";
            int prefixoLen = item.Quantidade >= 10 ? 4 : 3;
            int maxDesc = Math.Max(8, colunas - prefixoLen);

            var linhas = QuebrarDescricao(item.Descricao ?? "", maxDesc);
            for (int i = 0; i < linhas.Count; i++)
            {
                var prefixo = i == 0 ? $"{item.Quantidade}x " : "   ";
                sb.AppendLine(LimitarLinha(prefixo + linhas[i], colunas));
            }
            sb.AppendLine(valor.PadLeft(colunas - 3));
            if (!string.IsNullOrWhiteSpace(item.Observacao))
            {
                var obsLinhas = QuebrarDescricao($"Obs: {item.Observacao}", colunas - 3);
                foreach (var obsLinha in obsLinhas)
                    sb.AppendLine("   " + LimitarLinha(obsLinha, colunas - 3));
            }
        }

        sb.AppendLine(new string('-', colunas));
        
        // Se for extrato, mostrar subtotal, desconto e acréscimo
        if (request.IsExtrato && request.Subtotal.HasValue)
        {
            var subtotalTxt = $"R$ {request.Subtotal.Value.ToString("F2", ptBR)}";
            sb.AppendLine("SUBTOTAL:".PadRight(colunas - 3 - subtotalTxt.Length) + subtotalTxt);
            if (request.Desconto.HasValue && request.Desconto.Value > 0)
            {
                var descontoTxt = $"R$ {request.Desconto.Value.ToString("F2", ptBR)}";
                sb.AppendLine("DESCONTO:".PadRight(colunas - 3 - descontoTxt.Length) + descontoTxt);
            }
            if (request.Acrescimo.HasValue && request.Acrescimo.Value > 0)
            {
                var acrescimoTxt = $"R$ {request.Acrescimo.Value.ToString("F2", ptBR)}";
                sb.AppendLine("ACRESCIMO:".PadRight(colunas - 3 - acrescimoTxt.Length) + acrescimoTxt);
            }
            total = request.Subtotal.Value - (request.Desconto ?? 0) + (request.Acrescimo ?? 0);
        }
        var totalTxt = $"R$ {total.ToString("F2", ptBR)}";
        sb.AppendLine("TOTAL:".PadRight(colunas - 3 - totalTxt.Length) + totalTxt);
        sb.AppendLine(new string('=', colunas));

        // Recibo do cliente: formas de pagamento e troco (texto)
        if (request.IsReciboCliente)
        {
            sb.AppendLine();
            sb.AppendLine("--- PAGAMENTO ---");
            if (request.FormasPagamento != null && request.FormasPagamento.Count > 0)
            {
                foreach (var fp in request.FormasPagamento)
                {
                    var fpValor = $"R$ {fp.Valor.ToString("F2", ptBR)}";
                    var fpDesc = (fp.Descricao ?? "").Trim();
                    if (fpDesc.Length > colunas - fpValor.Length - 1)
                        fpDesc = fpDesc.Substring(0, colunas - fpValor.Length - 1);
                    sb.AppendLine(fpDesc.PadRight(colunas - 3 - fpValor.Length) + fpValor);
                }
            }
            if (request.TrocoTotal.HasValue && request.TrocoTotal.Value > 0)
            {
                sb.AppendLine(new string('-', colunas));
                var trocoTxt = $"R$ {request.TrocoTotal.Value.ToString("F2", ptBR)}";
                sb.AppendLine("TROCO:".PadRight(colunas - 3 - trocoTxt.Length) + trocoTxt);
            }
            sb.AppendLine(new string('=', colunas));

            // Delivery: endereço de entrega no recibo (reimpressão)
            if (request.IsCupomDelivery || !string.IsNullOrWhiteSpace(request.EnderecoEntrega))
            {
                sb.AppendLine();
                sb.AppendLine(new string('-', colunas));
                sb.AppendLine(LimitarLinha("DELIVERY - ENTREGA", colunas));
                sb.AppendLine(new string('-', colunas));
                if (!string.IsNullOrWhiteSpace(request.ClienteNome))
                    foreach (var lin in QuebrarCampo("CLIENTE: ", request.ClienteNome, colunas))
                        sb.AppendLine(lin);
                if (!string.IsNullOrWhiteSpace(request.EnderecoEntrega))
                {
                    sb.AppendLine("ENDEREÇO:");
                    var linhasEnd = QuebrarDescricao(request.EnderecoEntrega.Trim(), colunas - 2);
                    foreach (var lin in linhasEnd)
                        sb.AppendLine("  " + lin);
                }
                if (!string.IsNullOrWhiteSpace(request.PontoReferencia))
                    foreach (var lin in QuebrarCampo("PT. REF.: ", request.PontoReferencia, colunas))
                        sb.AppendLine(lin);
                if (!string.IsNullOrWhiteSpace(request.TelefoneEntrega))
                    foreach (var lin in QuebrarCampo("TELEFONE: ", request.TelefoneEntrega, colunas))
                        sb.AppendLine(lin);
                if (!string.IsNullOrWhiteSpace(request.ObservacoesPedido))
                {
                    sb.AppendLine("OBSERVAÇÕES:");
                    var linhasObs = QuebrarDescricao(request.ObservacoesPedido.Trim(), colunas - 2);
                    foreach (var lin in linhasObs)
                        sb.AppendLine("  " + lin);
                }
                if (!string.IsNullOrWhiteSpace(request.FormaPgtoDelivery))
                {
                    var fPgto = request.FormaPgtoDelivery;
                    var jaPago = request.JaPagoDelivery ||
                        fPgto.IndexOf("PAGAMENTO", StringComparison.OrdinalIgnoreCase) >= 0;
                    sb.AppendLine(new string('-', colunas));
                    foreach (var lin in QuebrarCampo("PAGAMENTO: ", fPgto, colunas))
                        sb.AppendLine(lin);
                    var statusPgto = jaPago ? "** JA PAGO **" : "** COBRAR NA ENTREGA **";
                    sb.AppendLine(statusPgto);
                }
                sb.AppendLine(new string('-', colunas));
            }
        }
        
        // Rodapé profissional para extrato
        if (request.IsExtrato)
        {
            sb.AppendLine();
            sb.AppendLine(new string('-', colunas));
            var rodape1 = LimitarLinha("DOCUMENTO DE CONFERENCIA", colunas);
            sb.AppendLine(new string(' ', (colunas - rodape1.Length) / 2) + rodape1);
            var rodape2 = LimitarLinha("NAO E DOCUMENTO FISCAL", colunas);
            sb.AppendLine(new string(' ', (colunas - rodape2.Length) / 2) + rodape2);
            sb.AppendLine(new string('-', colunas));
            if (!string.IsNullOrWhiteSpace(empresaData?.Site))
            {
                sb.AppendLine();
                var site = LimitarLinha(empresaData.Site, colunas);
                sb.AppendLine(new string(' ', (colunas - site.Length) / 2) + site);
            }
        }
        
        // Recibo: mínimo de linha em branco no final (compacto). Pedido/extrato: mais espaço.
        if (request.IsReciboCliente)
            sb.AppendLine();
        else
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GerarCupomEscPos(string nota, ImprimirPedidoRequest request, EmpresaConfig empresaData)
    {
        // Pedido normal (comanda em aberto): usar formato igual ao WhatsApp
        if (!request.IsExtrato && !request.IsReciboCliente)
            return GerarCupomFormatoComandaEscPos(nota, request, empresaData);

        var sb = new StringBuilder();
        int colunas = LARGURA_80MM;

        sb.Append("\x1B\x40");
        sb.Append("\x1B\x45\x01");
        
        if (request.IsExtrato)
        {
            sb.Append("\x1B\x61\x01");
            sb.AppendLine();
            sb.AppendLine(new string('=', colunas));
            if (!string.IsNullOrWhiteSpace(empresaData?.Nome))
                sb.AppendLine(LimitarLinha(empresaData.Nome.ToUpper(), colunas));
            if (!string.IsNullOrWhiteSpace(empresaData?.NomeFantasia))
                sb.AppendLine(LimitarLinha(empresaData.NomeFantasia, colunas));
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(empresaData?.Cnpj))
                sb.AppendLine(LimitarLinha($"CNPJ: {empresaData.Cnpj}", colunas));
            if (!string.IsNullOrWhiteSpace(empresaData?.Endereco))
            {
                var end = empresaData.Endereco + (string.IsNullOrWhiteSpace(empresaData.Bairro) ? "" : $" - {empresaData.Bairro}");
                sb.AppendLine(LimitarLinha(end, colunas));
            }
            if (!string.IsNullOrWhiteSpace(empresaData?.Cidade))
            {
                var cid = empresaData.Cidade + (string.IsNullOrWhiteSpace(empresaData.Cep) ? "" : $" - CEP: {empresaData.Cep}");
                sb.AppendLine(LimitarLinha(cid, colunas));
            }
            if (!string.IsNullOrWhiteSpace(empresaData?.Telefone))
                sb.AppendLine(LimitarLinha($"Tel: {empresaData.Telefone}", colunas));
            if (!string.IsNullOrWhiteSpace(empresaData?.Email))
                sb.AppendLine(LimitarLinha(empresaData.Email, colunas));
            sb.AppendLine(new string('=', colunas));
            sb.AppendLine();
            sb.AppendLine(LimitarLinha("EXTRATO DE COMANDA", colunas));
            sb.AppendLine();
            sb.AppendLine(LimitarLinha($"EXTRATO: #{nota.PadLeft(6, '0')}", colunas));
            sb.AppendLine(LimitarLinha($"DATA/HORA: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", colunas));
            sb.Append("\x1B\x61\x00");
            sb.AppendLine(new string('-', colunas));
        }
        else
        {
            sb.Append("\x1B\x61\x01");
            if (request.IsReciboCliente)
                sb.AppendLine("RECIBO DO CLIENTE");
            sb.AppendLine(LimitarLinha($"PEDIDO #{nota.PadLeft(6, '0')}", colunas));
            sb.AppendLine(LimitarLinha(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), colunas));
            sb.AppendLine(new string('=', colunas));
            sb.Append("\x1B\x61\x00");
        }

        sb.Append("\x1B\x61\x00");
        if (!string.IsNullOrEmpty(request.Comanda))
            sb.AppendLine(LimitarLinha($"COMANDA: {request.Comanda}", colunas));
        if (!string.IsNullOrEmpty(request.Mesa))
            sb.AppendLine(LimitarLinha($"MESA: {request.Mesa}", colunas));
        if (!string.IsNullOrEmpty(request.ClienteNome))
            sb.AppendLine(LimitarLinha($"CLIENTE: {request.ClienteNome}", colunas));

        if (!request.IsExtrato)
        {
            sb.AppendLine(new string('-', colunas));
            sb.AppendLine(LimitarLinha(request.ApenasNovosItens ? "NOVOS ITENS ADICIONADOS" : "ITENS DO PEDIDO", colunas));
            sb.AppendLine(new string('-', colunas));
        }
        else
        {
            sb.AppendLine();
        }

        var ptBR = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        decimal total = 0;
        foreach (var item in request.Itens)
        {
            var precoItem = item.Preco * item.Quantidade;
            total += precoItem;
            var valor = $"R$ {precoItem.ToString("F2", ptBR)}";
            int prefixoLen = item.Quantidade >= 10 ? 4 : 3;
            int maxDesc = Math.Max(8, colunas - prefixoLen);

            var linhas = QuebrarDescricao(item.Descricao ?? "", maxDesc);
            for (int i = 0; i < linhas.Count; i++)
            {
                var prefixo = i == 0 ? $"{item.Quantidade}x " : "   ";
                sb.AppendLine(LimitarLinha(prefixo + linhas[i], colunas));
            }
            sb.AppendLine(valor.PadLeft(colunas - 3));
            if (!string.IsNullOrWhiteSpace(item.Observacao))
            {
                var obsLinhas = QuebrarDescricao($"Obs: {item.Observacao}", colunas - 3);
                foreach (var obsLinha in obsLinhas)
                    sb.AppendLine("   " + LimitarLinha(obsLinha, colunas - 3));
            }
        }

        sb.AppendLine(new string('-', colunas));
        
        if (request.IsExtrato && request.Subtotal.HasValue)
        {
            var subtotalTxt = $"R$ {request.Subtotal.Value.ToString("F2", ptBR)}";
            sb.AppendLine("SUBTOTAL:".PadRight(colunas - 3 - subtotalTxt.Length) + subtotalTxt);
            if (request.Desconto.HasValue && request.Desconto.Value > 0)
            {
                var descontoTxt = $"R$ {request.Desconto.Value.ToString("F2", ptBR)}";
                sb.AppendLine("DESCONTO:".PadRight(colunas - 3 - descontoTxt.Length) + descontoTxt);
            }
            if (request.Acrescimo.HasValue && request.Acrescimo.Value > 0)
            {
                var acrescimoTxt = $"R$ {request.Acrescimo.Value.ToString("F2", ptBR)}";
                sb.AppendLine("ACRESCIMO:".PadRight(colunas - 3 - acrescimoTxt.Length) + acrescimoTxt);
            }
            total = request.Subtotal.Value - (request.Desconto ?? 0) + (request.Acrescimo ?? 0);
        }
        var totalTxt = $"R$ {total.ToString("F2", ptBR)}";
        sb.AppendLine("TOTAL:".PadRight(colunas - 3 - totalTxt.Length) + totalTxt);
        sb.AppendLine(new string('=', colunas));

        // Recibo do cliente: formas de pagamento e troco (ESC/POS)
        if (request.IsReciboCliente)
        {
            sb.AppendLine();
            sb.AppendLine("--- PAGAMENTO ---");
            if (request.FormasPagamento != null && request.FormasPagamento.Count > 0)
            {
                foreach (var fp in request.FormasPagamento)
                {
                    var fpValor = $"R$ {fp.Valor.ToString("F2", ptBR)}";
                    var fpDesc = (fp.Descricao ?? "").Trim();
                    if (fpDesc.Length > colunas - fpValor.Length - 1)
                        fpDesc = fpDesc.Substring(0, colunas - fpValor.Length - 1);
                    sb.AppendLine(fpDesc.PadRight(colunas - 3 - fpValor.Length) + fpValor);
                }
            }
            if (request.TrocoTotal.HasValue && request.TrocoTotal.Value > 0)
            {
                sb.AppendLine(new string('-', colunas));
                var trocoTxt = $"R$ {request.TrocoTotal.Value.ToString("F2", ptBR)}";
                sb.AppendLine("TROCO:".PadRight(colunas - 3 - trocoTxt.Length) + trocoTxt);
            }
            sb.AppendLine(new string('=', colunas));

            // Delivery: endereço de entrega no recibo (reimpressão)
            if (request.IsCupomDelivery || !string.IsNullOrWhiteSpace(request.EnderecoEntrega))
            {
                sb.AppendLine();
                sb.AppendLine(new string('-', colunas));
                sb.AppendLine(LimitarLinha("DELIVERY - ENTREGA", colunas));
                sb.AppendLine(new string('-', colunas));
                if (!string.IsNullOrWhiteSpace(request.ClienteNome))
                    foreach (var lin in QuebrarCampo("CLIENTE: ", request.ClienteNome, colunas))
                        sb.AppendLine(lin);
                if (!string.IsNullOrWhiteSpace(request.EnderecoEntrega))
                {
                    sb.AppendLine("ENDEREÇO:");
                    var linhasEnd = QuebrarDescricao(request.EnderecoEntrega.Trim(), colunas - 2);
                    foreach (var lin in linhasEnd)
                        sb.AppendLine("  " + lin);
                }
                if (!string.IsNullOrWhiteSpace(request.PontoReferencia))
                    foreach (var lin in QuebrarCampo("PT. REF.: ", request.PontoReferencia, colunas))
                        sb.AppendLine(lin);
                if (!string.IsNullOrWhiteSpace(request.TelefoneEntrega))
                    foreach (var lin in QuebrarCampo("TELEFONE: ", request.TelefoneEntrega, colunas))
                        sb.AppendLine(lin);
                if (!string.IsNullOrWhiteSpace(request.ObservacoesPedido))
                {
                    sb.AppendLine("OBSERVAÇÕES:");
                    var linhasObs = QuebrarDescricao(request.ObservacoesPedido.Trim(), colunas - 2);
                    foreach (var lin in linhasObs)
                        sb.AppendLine("  " + lin);
                }
                if (!string.IsNullOrWhiteSpace(request.FormaPgtoDelivery))
                {
                    var fPgtoEsc = request.FormaPgtoDelivery;
                    var jaPagoEsc = request.JaPagoDelivery ||
                        fPgtoEsc.IndexOf("PAGAMENTO", StringComparison.OrdinalIgnoreCase) >= 0;
                    sb.AppendLine(new string('-', colunas));
                    foreach (var lin in QuebrarCampo("PAGAMENTO: ", fPgtoEsc, colunas))
                        sb.AppendLine(lin);
                    var statusPgtoEsc = jaPagoEsc ? "** JA PAGO **" : "** COBRAR NA ENTREGA **";
                    sb.AppendLine(statusPgtoEsc);
                }
                sb.AppendLine(new string('-', colunas));
            }
        }
        
        // Rodapé profissional para extrato
        if (request.IsExtrato)
        {
            sb.AppendLine();
            sb.AppendLine(new string('-', colunas));
            sb.Append("\x1B\x61\x01");
            sb.AppendLine(LimitarLinha("DOCUMENTO DE CONFERENCIA", colunas));
            sb.AppendLine(LimitarLinha("NAO E DOCUMENTO FISCAL", colunas));
            sb.Append("\x1B\x61\x00");
            sb.AppendLine(new string('-', colunas));
            if (!string.IsNullOrWhiteSpace(empresaData?.Site))
            {
                sb.AppendLine();
                sb.Append("\x1B\x61\x01");
                sb.AppendLine(LimitarLinha(empresaData.Site, colunas));
                sb.Append("\x1B\x61\x00");
            }
        }
        
        // Recibo: mínimo no final (compacto)
        if (request.IsReciboCliente)
            sb.AppendLine();
        else
        {
            sb.AppendLine();
            sb.AppendLine();
        }

        // Corte
        sb.Append("\x1D\x56\x00");
        return sb.ToString();
    }

    private List<string> QuebrarDescricao(string descricao, int maxLen)
    {
        if (maxLen < 8) maxLen = 8;
        var texto = (descricao ?? string.Empty).Trim();
        var linhas = new List<string>();
        
        if (string.IsNullOrEmpty(texto))
        {
            linhas.Add("");
            return linhas;
        }
        
        // Quebrar por palavras quando possível, senão quebrar por caractere
        int idx = 0;
        while (idx < texto.Length)
        {
            int len = Math.Min(maxLen, texto.Length - idx);
            string linha;
            
            // Se não é o final do texto, tentar quebrar em espaço
            if (idx + len < texto.Length)
            {
                int ultimoEspaco = texto.LastIndexOf(' ', idx + len - 1, len);
                if (ultimoEspaco > idx)
                {
                    len = ultimoEspaco - idx + 1;
                    linha = texto.Substring(idx, len).TrimEnd();
                    idx = ultimoEspaco + 1;
                }
                else
                {
                    linha = texto.Substring(idx, len);
                    idx += len;
                }
            }
            else
            {
                linha = texto.Substring(idx);
                idx = texto.Length;
            }
            
            linhas.Add(linha);
        }
        if (linhas.Count == 0)
            linhas.Add(string.Empty);
        return linhas;
    }

    // Formato "Comanda em Aberto": mesma margem de segurança que cupom (35 chars)
    private const int LARGURA_COMANDA = 35;
    private static readonly string SEP_COMANDA = new string('─', LARGURA_COMANDA);

    private static int CalcularLarguraComanda(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return 0;
        int n = 0;
        foreach (var c in texto)
            n += (c >= '\x20' && c <= '\x7E') || c == '─' ? 1 : 1; // emojis/codepoints contam 1 para impressora
        return texto.Length;
    }

    private static string CentralizarComanda(string texto, int largura = LARGURA_COMANDA)
    {
        var s = (texto ?? "").Trim();
        var larguraReal = CalcularLarguraComanda(s);
        if (larguraReal >= largura) return s; // Texto longo: exibir sem recortar
        var espacosEsq = Math.Max(0, (largura - larguraReal) / 2);
        return new string(' ', espacosEsq) + s;
    }

    private static List<string> QuebrarEmDuasLinhasComanda(string texto, int maxChars = LARGURA_COMANDA)
    {
        var t = (texto ?? "").Trim();
        if (string.IsNullOrEmpty(t)) return new List<string> { "" };
        if (CalcularLarguraComanda(t) <= maxChars) return new List<string> { t };
        int lastSpace = -1;
        int i = 0;
        for (; i < t.Length; i++)
        {
            if (t[i] == ' ') lastSpace = i;
            if (CalcularLarguraComanda(t.Substring(0, i + 1)) > maxChars) break;
        }
        if (lastSpace <= 0) return new List<string> { t };
        return new List<string>
        {
            t.Substring(0, lastSpace).Trim(),
            t.Substring(lastSpace).Trim()
        };
    }

    /// <summary>Gera cupom no formato "Comanda em Aberto" (igual ao WhatsApp) para impressão.</summary>
    private string GerarCupomFormatoComandaTexto(string nota, ImprimirPedidoRequest request, EmpresaConfig empresaData)
    {
        var sb = new StringBuilder();
        var notaFormatada = nota.PadLeft(6, '0');
        var saudacao = !string.IsNullOrWhiteSpace(request.ClienteNome) ? $"Olá {request.ClienteNome!.ToUpperInvariant()}!" : "Olá!";
        var dataHora = DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm:ss");
        var nomeRodape = !string.IsNullOrWhiteSpace(empresaData?.NomeFantasia) ? empresaData.NomeFantasia! : (empresaData?.Nome ?? "Estabelecimento");

        sb.AppendLine(CentralizarComanda($"{saudacao}"));
        var tituloCabecalho = !string.IsNullOrWhiteSpace(request.TituloSecao)
            ? request.TituloSecao
            : (request.IsCupomDelivery ? "PEDIDO DELIVERY" : "COMANDA EM ABERTO");
        sb.AppendLine(CentralizarComanda(tituloCabecalho));
        if (!string.IsNullOrWhiteSpace(request.TituloSecao))
            sb.AppendLine(SEP_COMANDA);
        sb.AppendLine();
        sb.AppendLine(CentralizarComanda($"{dataHora}"));
        sb.AppendLine(CentralizarComanda($"Nota {notaFormatada}"));
        if (!string.IsNullOrEmpty(request.Comanda))
            sb.AppendLine(CentralizarComanda($"Comanda {request.Comanda}"));
        if (!string.IsNullOrEmpty(request.Mesa))
            sb.AppendLine(CentralizarComanda($"Mesa {request.Mesa}"));
        sb.AppendLine();
        sb.AppendLine(SEP_COMANDA);
        sb.AppendLine(CentralizarComanda("PRODUTOS"));
        sb.AppendLine();

        decimal totalGeral = 0;
        int idx = 1;
        foreach (var item in request.Itens)
        {
            var descricao = (item.Descricao ?? "").Trim();
            if (string.IsNullOrEmpty(descricao)) descricao = $"Produto {item.Codigo}";
            var qtd = item.Quantidade;
            var totalItem = item.Preco * qtd;
            totalGeral += totalItem;
            var linhasDesc = QuebrarEmDuasLinhasComanda(descricao, LARGURA_COMANDA - 4);
            sb.AppendLine(CentralizarComanda($"{idx}. {linhasDesc[0]}"));
            if (linhasDesc.Count > 1)
                sb.AppendLine(CentralizarComanda(linhasDesc[1]));
            var qtdFmt = qtd.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            var precoFmt = item.Preco.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            var totalFmt = totalItem.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            sb.AppendLine(CentralizarComanda($"{qtdFmt} × R$ {precoFmt} = R$ {totalFmt}"));
            if (!string.IsNullOrWhiteSpace(item.Observacao))
            {
                var obsLinhas = QuebrarDescricao($"Obs: {item.Observacao.Trim()}", LARGURA_COMANDA - 2);
                foreach (var ol in obsLinhas)
                    sb.AppendLine("  " + LimitarLinha(ol, LARGURA_COMANDA - 2));
            }
            idx++;
        }

        var totalFormatado = totalGeral.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
        sb.AppendLine(SEP_COMANDA);
        sb.AppendLine(CentralizarComanda($"TOTAL  R$ {totalFormatado}"));
        sb.AppendLine(SEP_COMANDA);

        if (request.IsCupomDelivery)
        {
            sb.AppendLine();
            sb.AppendLine(SEP_COMANDA);
            sb.AppendLine(CentralizarComanda("PEDIDO DELIVERY - ENTREGADOR"));
            sb.AppendLine(SEP_COMANDA);
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(request.ClienteNome))
            {
                foreach (var lin in QuebrarCampo("CLIENTE: ", request.ClienteNome, LARGURA_COMANDA))
                    sb.AppendLine(lin);
                sb.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(request.EnderecoEntrega))
            {
                sb.AppendLine("ENDEREÇO:");
                var linhasEnd = QuebrarDescricao(request.EnderecoEntrega.Trim(), LARGURA_COMANDA - 2);
                foreach (var lin in linhasEnd)
                    sb.AppendLine("  " + lin);
                sb.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(request.PontoReferencia))
            {
                foreach (var lin in QuebrarCampo("PT. REF.: ", request.PontoReferencia, LARGURA_COMANDA))
                    sb.AppendLine(lin);
                sb.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(request.TelefoneEntrega))
            {
                foreach (var lin in QuebrarCampo("TELEFONE: ", request.TelefoneEntrega, LARGURA_COMANDA))
                    sb.AppendLine(lin);
                sb.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(request.ObservacoesPedido))
            {
                sb.AppendLine("OBSERVAÇÕES:");
                var linhasObs = QuebrarDescricao(request.ObservacoesPedido.Trim(), LARGURA_COMANDA - 2);
                foreach (var lin in linhasObs)
                    sb.AppendLine("  " + lin);
                sb.AppendLine();
            }
            if (!string.IsNullOrWhiteSpace(request.FormaPgtoDelivery))
            {
                var fPgtoCmd = request.FormaPgtoDelivery;
                var jaPagoCmd = request.JaPagoDelivery ||
                    fPgtoCmd.IndexOf("PAGAMENTO", StringComparison.OrdinalIgnoreCase) >= 0;
                foreach (var lin in QuebrarCampo("PAGAMENTO: ", fPgtoCmd, LARGURA_COMANDA))
                    sb.AppendLine(lin);
                var statusPgtoCmd = jaPagoCmd ? "JA PAGO" : "COBRAR NA ENTREGA";
                sb.AppendLine(statusPgtoCmd);
                sb.AppendLine();
            }
            sb.AppendLine(SEP_COMANDA);
        }

        sb.AppendLine();
        sb.AppendLine(CentralizarComanda("Obrigado pela preferencia."));
        sb.AppendLine(CentralizarComanda("Qualquer duvida, estamos a disposicao."));
        sb.AppendLine(CentralizarComanda(nomeRodape));
        sb.AppendLine();
        return sb.ToString();
    }

    /// <summary>Gera cupom no formato "Comanda em Aberto" em ESC/POS para impressora térmica.</summary>
    private string GerarCupomFormatoComandaEscPos(string nota, ImprimirPedidoRequest request, EmpresaConfig empresaData)
    {
        var cupomTexto = GerarCupomFormatoComandaTexto(nota, request, empresaData);
        var sb = new StringBuilder();
        sb.Append("\x1B\x40");   // ESC @ Initialize
        sb.Append("\x1B\x45\x01"); // ESC E 1 Bold on
        foreach (var linha in cupomTexto.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            sb.AppendLine(linha);
        sb.Append("\x1D\x56\x00"); // GS V 0 Cut
        return sb.ToString();
    }

    private async Task<bool> ImprimirRedeAsync(string cupom)
    {
        if (_impressaoConfig?.ImpressoraRede == null)
        {
            _logger.LogError("❌ Configuração de impressora de rede não encontrada");
            return false;
        }

        var ip = _impressaoConfig.ImpressoraRede.IP;
        var porta = _impressaoConfig.ImpressoraRede.Porta;
        var timeout = _impressaoConfig.ImpressoraRede.Timeout;

        try
        {
            _logger.LogInformation("🖨️ Conectando à impressora {IP}:{Porta}", ip, porta);

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ip, porta);
            var timeoutTask = Task.Delay(timeout);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                _logger.LogError("❌ Timeout ao conectar à impressora {IP}:{Porta}", ip, porta);
                return false;
            }

            if (!client.Connected)
            {
                _logger.LogError("❌ Não foi possível conectar à impressora {IP}:{Porta}", ip, porta);
                return false;
            }

            _logger.LogInformation("✅ Conectado à impressora {IP}:{Porta}", ip, porta);

            using var stream = client.GetStream();
            var bytes = Encoding.UTF8.GetBytes(cupom);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();

            _logger.LogInformation("✅ Cupom enviado para impressora (TCP/IP) com sucesso");
            return true;
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex, "❌ Erro de rede ao imprimir: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao imprimir: {Message}", ex.Message);
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private async Task<bool> ImprimirWindowsUNCAsync(string cupom)
    {
        if (_impressaoConfig?.ImpressoraRede == null || string.IsNullOrEmpty(_impressaoConfig.ImpressoraRede.CaminhoUNC))
        {
            _logger.LogError("❌ Caminho UNC da impressora não configurado");
            return false;
        }

        var caminhoUNC = _impressaoConfig.ImpressoraRede.CaminhoUNC;

        try
        {
            _logger.LogInformation("🖨️ Imprimindo via Windows UNC: {CaminhoUNC}", caminhoUNC);

            return await Task.Run(() =>
            {
                try
                {
                    using var printDoc = new PrintDocument();
                    printDoc.PrinterSettings.PrinterName = caminhoUNC;

                    // Verificar se a impressora existe
                    if (!printDoc.PrinterSettings.IsValid)
                    {
                        _logger.LogError("❌ Impressora não encontrada: {CaminhoUNC}", caminhoUNC);
                        return false;
                    }

                    // Configurar página (unidade: centésimos de polegada)
                    // Largura ~80mm => 3.15 pol => 315
                    // Altura grande para evitar corte (ex: 2000 = 20 pol)
                    printDoc.DefaultPageSettings.PaperSize = new PaperSize("Custom", 315, 2000);
                    printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                    string? textoParaImprimir = null;
                    var printHandler = new PrintPageEventHandler((sender, e) =>
                    {
                        if (textoParaImprimir != null && e.Graphics != null)
                        {
                            // Fonte 10pt Bold = mais forte e legível (antes 8pt Regular)
                            using var font = new Font("Courier New", 10, FontStyle.Bold);
                            var rect = new RectangleF(3, 0, e.PageBounds.Width - 5, e.PageBounds.Height);
                            e.Graphics.DrawString(textoParaImprimir, font, Brushes.Black, rect);
                        }
                        e.HasMorePages = false;
                    });

                    printDoc.PrintPage += printHandler;
                    textoParaImprimir = cupom;
                    printDoc.Print();
                    printDoc.PrintPage -= printHandler;

                    _logger.LogInformation("✅ Cupom impresso com sucesso via Windows UNC");
                    return true;
                }
                catch (InvalidPrinterException ex)
                {
                    _logger.LogError(ex, "❌ Impressora inválida ou não encontrada: {CaminhoUNC}", caminhoUNC);
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao imprimir via Windows UNC: {Message}", ex.Message);
                    return false;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao imprimir via Windows UNC: {Message}", ex.Message);
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private async Task<bool> ImprimirWindowsLocalAsync(string cupom)
    {
        if (_impressaoConfig?.ImpressoraLocal == null)
        {
            _logger.LogError("❌ Configuração da impressora local não encontrada");
            return false;
        }

        var nomeImpressoraConfigurado = (_impressaoConfig.ImpressoraLocal.Nome ?? string.Empty).Trim();
        var nomeImpressora = ResolverNomeImpressoraWindows(nomeImpressoraConfigurado);

        if (string.IsNullOrWhiteSpace(nomeImpressora))
        {
            _logger.LogError("❌ Nenhuma impressora local válida foi encontrada no Windows");
            return false;
        }

        try
        {
            _logger.LogInformation("🖨️ Imprimindo via impressora local: {Nome}", nomeImpressora);

            return await Task.Run(() =>
            {
                try
                {
                    using var printDoc = new PrintDocument();
                    printDoc.PrinterSettings.PrinterName = nomeImpressora;

                    // Verificar se a impressora existe
                    if (!printDoc.PrinterSettings.IsValid)
                    {
                        _logger.LogError("❌ Impressora local não encontrada: {Nome}", nomeImpressora);
                        return false;
                    }

                    // Configurar página (unidade: centésimos de polegada)
                    // Largura ~80mm => 3.15 pol => 315
                    printDoc.DefaultPageSettings.PaperSize = new PaperSize("Custom", 315, 2000);
                    printDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

                    string? textoParaImprimir = null;
                    var printHandler = new PrintPageEventHandler((sender, e) =>
                    {
                        if (textoParaImprimir != null && e.Graphics != null)
                        {
                            // Fonte 10pt Bold = mais forte e legível (antes 8pt Regular)
                            using var font = new Font("Courier New", 10, FontStyle.Bold);
                            var rect = new RectangleF(3, 0, e.PageBounds.Width - 5, e.PageBounds.Height);
                            e.Graphics.DrawString(textoParaImprimir, font, Brushes.Black, rect);
                        }
                        e.HasMorePages = false;
                    });

                    printDoc.PrintPage += printHandler;
                    textoParaImprimir = cupom;
                    printDoc.Print();
                    printDoc.PrintPage -= printHandler;

                    _logger.LogInformation("✅ Cupom impresso com sucesso via impressora local");
                    return true;
                }
                catch (InvalidPrinterException ex)
                {
                    _logger.LogError(ex, "❌ Impressora local inválida ou não encontrada: {Nome}", nomeImpressora);
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao imprimir via impressora local: {Message}", ex.Message);
                    return false;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao imprimir via impressora local: {Message}", ex.Message);
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private string? ResolverNomeImpressoraWindows(string? nomeImpressoraConfigurado)
    {
        var nomeConfig = (nomeImpressoraConfigurado ?? string.Empty).Trim();
        var impressoras = PrinterSettings.InstalledPrinters
            .Cast<string>()
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        if (impressoras.Count == 0)
        {
            _logger.LogWarning("⚠️ Nenhuma impressora instalada encontrada no Windows");
            return null;
        }

        if (!string.IsNullOrWhiteSpace(nomeConfig))
        {
            var matchExato = impressoras.FirstOrDefault(n =>
                string.Equals(n.Trim(), nomeConfig, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(matchExato))
                return matchExato;

            var matchContem = impressoras.FirstOrDefault(n =>
                n.Contains(nomeConfig, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(matchContem))
            {
                _logger.LogWarning(
                    "⚠️ Impressora local '{NomeConfigurado}' não encontrada exatamente. Usando correspondência: {NomeEncontrado}",
                    nomeConfig,
                    matchContem);
                return matchContem;
            }

            _logger.LogWarning(
                "⚠️ Impressora local configurada não encontrada: {NomeConfigurado}. Tentando impressora padrão do Windows...",
                nomeConfig);
        }
        else
        {
            _logger.LogWarning("⚠️ Nome da impressora local vazio. Tentando impressora padrão do Windows...");
        }

        var defaultPrinter = new PrinterSettings().PrinterName;
        if (!string.IsNullOrWhiteSpace(defaultPrinter))
        {
            _logger.LogInformation("ℹ️ Usando impressora padrão do Windows: {Nome}", defaultPrinter);
            return defaultPrinter;
        }

        var primeiraInstalada = impressoras.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(primeiraInstalada))
        {
            _logger.LogInformation("ℹ️ Sem impressora padrão definida. Usando primeira instalada: {Nome}", primeiraInstalada);
            return primeiraInstalada;
        }

        return null;
    }
}

