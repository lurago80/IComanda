using AutoMapper;
using Dapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Repositories.Implementations;
using IComanda.API.Services.Interfaces;
using IComanda.API.Data;
using System.Text.RegularExpressions;
using FirebirdSql.Data.FirebirdClient;
using System.Data;

namespace IComanda.API.Services.Implementations;

public class VendaService : IVendaService
{
    private readonly IVendaRepository _vendaRepository;
    private readonly IItemVendaTemporarioRepository _itemTemporarioRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IReceberRepository _receberRepository;
    private readonly IRecebimentoService _recebimentoService;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<VendaService> _logger;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IImpressaoService _impressaoService;
    private readonly IEmitenteRepository _emitenteRepository;

    public VendaService(
        IVendaRepository vendaRepository,
        IItemVendaTemporarioRepository itemTemporarioRepository,
        IProdutoRepository produtoRepository,
        IClienteRepository clienteRepository,
        IReceberRepository receberRepository,
        IRecebimentoService recebimentoService,
        IDbConnectionFactory connectionFactory,
        IMapper mapper,
        ILogger<VendaService> logger,
        IWhatsAppService whatsAppService,
        IImpressaoService impressaoService,
        IEmitenteRepository emitenteRepository)
    {
        _vendaRepository = vendaRepository;
        _itemTemporarioRepository = itemTemporarioRepository;
        _produtoRepository = produtoRepository;
        _clienteRepository = clienteRepository;
        _receberRepository = receberRepository;
        _recebimentoService = recebimentoService;
        _connectionFactory = connectionFactory;
        _mapper = mapper;
        _logger = logger;
        _whatsAppService = whatsAppService;
        _impressaoService = impressaoService;
        _emitenteRepository = emitenteRepository;
    }

    /// <summary>
    /// Monta o endereço completo do cliente para o cupom de entrega.
    /// </summary>
    private static string? MontarEnderecoCliente(Cliente c)
    {
        if (c == null) return null;
        var partes = new List<string>();
        var end = (c.Endereco1 ?? "").Trim();
        if (!string.IsNullOrEmpty(end))
        {
            if (!string.IsNullOrWhiteSpace(c.Numero1))
                end += ", " + c.Numero1.Trim();
            // Complemento1 é impresso separado como "Ponto de Referência" no cupom delivery
            partes.Add(end);
        }
        if (!string.IsNullOrWhiteSpace(c.Bairro1))
            partes.Add(c.Bairro1.Trim());
        if (!string.IsNullOrWhiteSpace(c.Cidade1))
        {
            var cidadeUf = c.Cidade1.Trim();
            if (!string.IsNullOrWhiteSpace(c.Uf1))
                cidadeUf += " - " + c.Uf1.Trim();
            partes.Add(cidadeUf);
        }
        if (!string.IsNullOrWhiteSpace(c.Cep1))
            partes.Add("CEP: " + c.Cep1.Trim());
        return partes.Count > 0 ? string.Join(", ", partes) : null;
    }

    /// <summary>
    /// Formata telefone para WhatsApp (apenas dígitos; adiciona 55 se for Brasil 10/11 dígitos).
    /// </summary>
    private static string? FormatarTelefoneWhatsApp(string? telefone, string? celular)
    {
        var numero = Regex.Replace(telefone ?? celular ?? "", @"\D", "");
        if (string.IsNullOrEmpty(numero) || numero.Length < 10) return null;
        if (numero.Length == 10 || numero.Length == 11)
        {
            if (!numero.StartsWith("55"))
                numero = "55" + numero;
        }
        return numero;
    }

    /// <summary>
    /// Monta a mensagem de WhatsApp para pedido delivery aprovado (formato completo com estabelecimento, controle, endereço, itens, totais).
    /// </summary>
    private async Task<string?> MontarMensagemWhatsAppPedidoAprovadoAsync(string nota, CriarVendaRequest request, Cliente cliente)
    {
        var ci = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
        var nomeCliente = string.IsNullOrWhiteSpace(cliente.Nome) ? "Cliente" : cliente.Nome.Trim().ToUpperInvariant();
        var emitente = await _emitenteRepository.GetEmitenteAsync();
        var nomeEstabelecimento = (emitente?.NomeFantasia ?? emitente?.Nome ?? "Estabelecimento").Trim().ToUpperInvariant();
        var controle = nota.TrimStart('0');
        if (string.IsNullOrEmpty(controle)) controle = nota;
        var endereco = MontarEnderecoCliente(cliente) ?? "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Olá {nomeCliente}, seu pedido foi aprovado! 🤗 Em breve estaremos saindo para entrega");
        sb.AppendLine();
        sb.AppendLine($"🏬 {nomeEstabelecimento}");
        sb.AppendLine($"📝 Controle: {controle}");
        sb.AppendLine();
        sb.AppendLine($"🏠 {endereco}");
        sb.AppendLine();
        sb.AppendLine("Produtos:");

        decimal totalItens = 0;
        decimal valorTaxa = 0;
        foreach (var it in request.Itens ?? new List<CriarItemVendaRequest>())
        {
            var prod = await _produtoRepository.GetProdutoAsync(it.Codigo);
            var descricao = prod?.Descricao?.Trim() ?? $"Produto {it.Codigo}";
            var totalItem = it.Preco * it.Qtd;
            totalItens += totalItem;
            var qtd = (int)it.Qtd;
            var precoFmt = it.Preco.ToString("N2", ci).Replace(".", ",");
            var totalFmt = totalItem.ToString("N2", ci).Replace(".", ",");
            sb.AppendLine($"➡️ {qtd} {descricao.ToUpperInvariant()} R$ {totalFmt}");
            if (!string.IsNullOrWhiteSpace(it.Observacao))
            {
                var linhas = it.Observacao!.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var linha in linhas)
                    sb.AppendLine("      " + linha.Trim());
            }
            sb.AppendLine();
            if (descricao.IndexOf("taxa", StringComparison.OrdinalIgnoreCase) >= 0 || descricao.IndexOf("entrega", StringComparison.OrdinalIgnoreCase) >= 0)
                valorTaxa += totalItem;
        }

        var desconto = request.Desconto;
        var totalFinal = totalItens - desconto;
        if (valorTaxa > 0)
        {
            sb.AppendLine($"🛵 Taxa: R$ {valorTaxa.ToString("N2", ci).Replace(".", ",")}");
        }
        if (desconto > 0)
        {
            sb.AppendLine($"💵 Desconto: R$ {desconto.ToString("N2", ci).Replace(".", ",")}");
        }
        sb.AppendLine($"💰 Total: R$ {totalFinal.ToString("N2", ci).Replace(".", ",")}");
        sb.AppendLine();
        var formaPgto = string.IsNullOrWhiteSpace(request.FormasPgto) ? "A DEFINIR" : request.FormasPgto.Trim().ToUpperInvariant();
        sb.AppendLine($"💲 PAGAMENTO {formaPgto}");
        sb.AppendLine();
        sb.AppendLine("     *** " + nomeEstabelecimento + " ***");

        return sb.ToString();
    }

    public async Task<VendaDto> CriarVendaAsync(CriarVendaRequest request)
    {
        _logger.LogInformation("🔄 Criando venda temporária (será finalizada pelo Delphi Desktop)");
        
        // 1. Validações iniciais
        if (request.Itens == null || request.Itens.Count == 0)
        {
            throw new ArgumentException("A venda deve conter pelo menos um item.");
        }

        // Validar origem da venda (BA = balcão/comanda, DL = delivery)
        var origemVenda = string.IsNullOrWhiteSpace(request.Origem) ? "BA" : request.Origem.Trim().ToUpperInvariant();
        
        if (origemVenda == "DL")
        {
            // 🚚 DELIVERY: Validações específicas para pedidos de entrega
            _logger.LogInformation("🚚 Criando pedido DELIVERY");
            
            // Cliente OBRIGATÓRIO para delivery
            if (request.Cliente <= 0)
            {
                _logger.LogWarning("❌ Tentativa de criar pedido delivery sem cliente obrigatório");
                throw new InvalidOperationException("Cliente é obrigatório para pedidos de delivery. Selecione ou cadastre um cliente antes de criar o pedido.");
            }
            
            // Comanda e Mesa devem estar vazios/null para delivery
            if (request.Comanda.HasValue && request.Comanda.Value > 0)
            {
                _logger.LogWarning("❌ Tentativa de criar pedido delivery com comanda (não permitido)");
                throw new InvalidOperationException("Pedidos de delivery não utilizam comanda. Deixe o campo em branco.");
            }
            
            if (request.Mesa.HasValue && request.Mesa.Value > 0)
            {
                _logger.LogWarning("❌ Tentativa de criar pedido delivery com mesa (não permitido)");
                throw new InvalidOperationException("Pedidos de delivery não utilizam mesa. Deixe o campo em branco.");
            }
            
            _logger.LogInformation("✅ Validações de delivery OK - Cliente: {Cliente}", request.Cliente);
        }
        else if (origemVenda == "BA")
        {
            // 🍽️ BALCÃO/COMANDA: Validações específicas para comandas
            _logger.LogInformation("🍽️ Criando venda de BALCÃO/COMANDA");
            if (!request.Comanda.HasValue || request.Comanda.Value <= 0)
            {
                _logger.LogWarning("❌ Tentativa de abrir comanda sem número válido (obrigatório para venda comanda)");
                throw new InvalidOperationException("É obrigatório informar um número de comanda válido (maior que zero) para abrir uma comanda.");
            }
            var comandaAberta = await VerificarComandaAbertaAsync(request.Comanda.Value);
            if (comandaAberta)
            {
                _logger.LogWarning($"❌ Tentativa de abrir comanda {request.Comanda.Value} que já está aberta");
                throw new InvalidOperationException($"A comanda {request.Comanda.Value} já possui uma venda em aberto. Feche ou exclua a comanda existente antes de abrir uma nova.");
            }
            _logger.LogInformation("✅ Validações de comanda OK - Comanda: {Comanda}", request.Comanda);
        }
        else if (origemVenda == "PD")
        {
            // PDV rápido: não exige comanda, cliente opcional
            _logger.LogInformation("🟢 Criando venda PDV rápido (PD)");
            // Sem validação de comanda, cliente pode ser 0
        }
        else
        {
            _logger.LogWarning($"❌ Origem de venda inválida: {origemVenda}");
            throw new InvalidOperationException($"Origem de venda inválida: '{origemVenda}'. Deve ser 'BA' (Balcão), 'DL' (Delivery) ou 'PD' (PDV rápido).");
        }

        // 2. Validar produtos antes de processar
        var codigosProdutos = request.Itens.Select(i => i.Codigo).Distinct().ToList();
        var produtos = new Dictionary<int, Produto>();
        var produtosInvalidos = new List<string>();
        var estoqueInsuficiente = new List<string>();
        
        foreach (var codigo in codigosProdutos)
        {
            var produto = await _produtoRepository.GetProdutoAsync(codigo);
            if (produto == null)
            {
                produtosInvalidos.Add($"Produto com código {codigo} não encontrado");
            }
            else if (produto.Ativo != 1)
            {
                produtosInvalidos.Add($"Produto {produto.Descricao} (código {codigo}) está inativo");
            }
            else
            {
                produtos[codigo] = produto;
                
                // Validar estoque (se produto não for pesável, verificar quantidade)
                if (produto.Pesavel != 1 && produto.Quantidade > 0)
                {
                    var quantidadeSolicitada = request.Itens
                        .Where(i => i.Codigo == codigo)
                        .Sum(i => i.Qtd);
                    
                    if (quantidadeSolicitada > produto.Quantidade)
                    {
                        estoqueInsuficiente.Add(
                            $"Produto {produto.Descricao} (código {codigo}): " +
                            $"Estoque disponível: {produto.Quantidade}, " +
                            $"Solicitado: {quantidadeSolicitada}");
                    }
                }
            }
        }

        if (produtosInvalidos.Any())
        {
            throw new InvalidOperationException($"Produtos inválidos: {string.Join("; ", produtosInvalidos)}");
        }

        if (estoqueInsuficiente.Any())
        {
            _logger.LogWarning("⚠️ Estoque insuficiente para os seguintes produtos: {Produtos}", 
                string.Join("; ", estoqueInsuficiente));
            throw new InvalidOperationException(
                $"Estoque insuficiente: {string.Join("; ", estoqueInsuficiente)}");
        }

        // 3. Validar itens
        foreach (var item in request.Itens)
        {
            if (item.Codigo <= 0)
            {
                throw new ArgumentException($"Item com código inválido: {item.Codigo}");
            }
            if (item.Qtd <= 0)
            {
                throw new ArgumentException($"Item {item.Codigo} possui quantidade inválida: {item.Qtd}");
            }
            if (item.Preco < 0)
            {
                throw new ArgumentException($"Item {item.Codigo} possui preço inválido: {item.Preco}");
            }
        }
        
        // Gerar próxima nota
        var nota = await _vendaRepository.GerarProximaNotaAsync();
        var operadorId = request.Operador == 0 ? 1 : request.Operador;

        _logger.LogInformation($"📝 Nota gerada: {nota} | Operador: {operadorId}");

        // Calcular totais
        decimal totalProdutos = request.Itens.Sum(i => i.Preco * i.Qtd);
        decimal totalFinal = totalProdutos - request.Desconto + request.Acrescimo;

        // Garantir data e hora atual para o lançamento
        var dataHoraAtual = DateTime.Now;
        var horaAtual = dataHoraAtual.TimeOfDay;
        
        // Criar venda com STATUS = ABERTO (será finalizada pelo Delphi)
        var venda = new Venda
        {
            Nota = nota,
            Sequencia = nota,
            Cliente = request.Cliente,
            Vendedor = request.Vendedor,
            Operador = operadorId,
            Caixa = request.Caixa == 0 ? 1 : request.Caixa,
            TotProdutos = totalProdutos,
            Total = totalFinal,
            Modelo = "D2",
            Serie = "001",
            Subserie = "01",
            Origem = origemVenda, // BA = comanda, DL = delivery (vem do request)
            Emissao = dataHoraAtual, // ✅ Sempre com data e hora
            Hora = horaAtual, // ✅ Sempre com data e hora
            DataSaida = dataHoraAtual, // ✅ Sempre com data e hora
            HoraSaida = horaAtual, // ✅ Sempre com data e hora
            Saida = 'X',
            Cfops = "5.102",
            Natureza = "Venda de Mercadoria",
            FormasPgto = string.IsNullOrEmpty(request.FormasPgto) ? "À VISTA" : request.FormasPgto,
            Especie = string.IsNullOrEmpty(request.Especie) ? "DINHEIRO" : request.Especie,
            Loja = "",
            Avista = "1",
            Lancado = "ABERTO", // ⚠️ IMPORTANTE: Mudado de "EFETIVADO" para "ABERTO"
            Quantidade = request.Itens.Count.ToString(),
            Desconto = request.Desconto,
            Acrescimo = request.Acrescimo,
            Comanda = request.Comanda,
            Mesa = request.Mesa,
            NumeroPessoas = request.NumeroPessoas,
            NomeCliente = string.IsNullOrWhiteSpace(request.NomeCliente) ? null : request.NomeCliente.Trim()
        };
        
        // Validação: garantir que sempre tenha data e hora
        if (venda.Emissao == default(DateTime))
        {
            venda.Emissao = DateTime.Now;
            _logger.LogWarning("⚠️ Data de emissão estava vazia - corrigida para data atual");
        }
        if (venda.Hora == default(TimeSpan))
        {
            venda.Hora = DateTime.Now.TimeOfDay;
            _logger.LogWarning("⚠️ Hora estava vazia - corrigida para hora atual");
        }

        // 4. Criar itens TEMPORÁRIOS (frente_tmpitvendas) - NÃO vai para ITVENDAS ainda
        var itensTemporarios = new List<ItemVendaTemporario>();
        for (int i = 0; i < request.Itens.Count; i++)
        {
            var itemRequest = request.Itens[i];
            var produto = produtos.ContainsKey(itemRequest.Codigo) ? produtos[itemRequest.Codigo] : null;
            
            // Normalizar descrição - remover quebras de linha e espaços múltiplos
            var descricao = produto?.Descricao ?? "";
            if (!string.IsNullOrWhiteSpace(descricao))
            {
                descricao = System.Text.RegularExpressions.Regex.Replace(descricao, @"\s+", " ").Trim();
            }
            
            // Garantir data e hora atual para cada item lançado
            // Preferir o timestamp do frontend (adicionadoEm) para preservar a hora real do lançamento
            var dataHoraItem = itemRequest.AdicionadoEm.HasValue
                ? itemRequest.AdicionadoEm.Value.ToLocalTime()
                : DateTime.Now;
            var horaItem = dataHoraItem.TimeOfDay;
            
            var itemTemp = new ItemVendaTemporario
            {
                Cupom = nota,
                NCaixa = "1",
                Data = dataHoraItem.Date, // ✅ Sempre com data
                Hora = horaItem, // ✅ Sempre com hora
                Operador = operadorId,
                Item = i + 1,
                Codigo = itemRequest.Codigo,
                Barras = produto?.CodigoBarra ?? "",
                Descricao = descricao,
                Qtd = itemRequest.Qtd,
                Preco = itemRequest.Preco,
                Tributacao = "",
                Icms = 0,
                Iss = 0,
                Und = produto?.UnMedida ?? "UN",
                Desconto = 0,
                Acrescimo = 0,
                Total = itemRequest.Preco * itemRequest.Qtd,
                // Tabela temporária usa coluna SERIAL para guardar observação do item
                Serial = !string.IsNullOrWhiteSpace(itemRequest.Observacao) ? itemRequest.Observacao.Trim() : (itemRequest.Serial ?? ""),
                Observacao = itemRequest.Observacao,
                Tipo = 1
            };
            
            // Validação: garantir que sempre tenha data e hora
            if (itemTemp.Data == default(DateTime))
            {
                itemTemp.Data = DateTime.Now.Date;
                _logger.LogWarning($"⚠️ Data do item {i + 1} estava vazia - corrigida para data atual");
            }
            if (itemTemp.Hora == default(TimeSpan))
            {
                itemTemp.Hora = DateTime.Now.TimeOfDay;
                _logger.LogWarning($"⚠️ Hora do item {i + 1} estava vazia - corrigida para hora atual");
            }
            itensTemporarios.Add(itemTemp);
        }

        // 5. Salvar tudo dentro de uma transação para garantir atomicidade
        using var connection = _connectionFactory.CreateConnection() as FbConnection;
        if (connection == null)
        {
            throw new Exception("Falha ao criar conexão Firebird");
        }

        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            _logger.LogInformation($"💾 Salvando venda ABERTA na tabela VENDAS (transação iniciada)");
            
            // Salvar venda (VENDAS) com status ABERTO dentro da transação
            var vendaCriada = await _vendaRepository.CriarVendaAsync(venda, transaction);
            if (!vendaCriada)
            {
                throw new Exception("Erro ao criar venda");
            }

            _logger.LogInformation($"📦 Salvando {itensTemporarios.Count} itens na tabela TEMPORÁRIA (frente_tmpitvendas)");
            
            // Salvar itens TEMPORÁRIOS (frente_tmpitvendas) dentro da transação
            var itensTemporariosCriados = await _itemTemporarioRepository.CriarItensTemporariosAsync(itensTemporarios, transaction);
            if (!itensTemporariosCriados)
            {
                throw new Exception("Erro ao criar itens temporários da venda");
            }

            // Commit da transação
            transaction.Commit();

            _logger.LogInformation($"✅ Venda {nota} criada com sucesso! Status: ABERTO (aguardando finalização pelo Delphi Desktop)");

            // Retornar venda criada
            var vendaDto = _mapper.Map<VendaDto>(venda);

            // Verificar se o cliente tem contas a receber em aberto
            if (request.Cliente > 0)
            {
                try
                {
                    var (temContasAberto, valorTotalPendente, quantidadeContas) = 
                        await _receberRepository.VerificarContasAbertoAsync(request.Cliente);
                    
                    if (temContasAberto)
                    {
                        vendaDto.ContasAberto = new ContasAbertoDto
                        {
                            TemContasAberto = true,
                            ValorTotalPendente = valorTotalPendente,
                            QuantidadeContas = quantidadeContas,
                            Mensagem = $"Cliente possui {quantidadeContas} conta(s) a receber em aberto no valor total de R$ {valorTotalPendente:N2}"
                        };
                        _logger.LogInformation("⚠️ Cliente {Cliente} possui contas em aberto - Quantidade: {Quantidade}, Valor: {Valor}",
                            request.Cliente, quantidadeContas, valorTotalPendente);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Erro ao verificar contas em aberto do cliente {Cliente}", request.Cliente);
                    // Não falha a criação se houver erro na verificação
                }
            }

            // Delivery: o envio de WhatsApp é feito manualmente pelo operador no frontend (não automático)
            var origemDelivery = string.IsNullOrWhiteSpace(request.Origem) ? "BA" : request.Origem.Trim().ToUpperInvariant();
            if (origemDelivery == "DL" && request.Cliente > 0)
            {
                _logger.LogInformation("📦 Delivery criado: pedido {Nota} - WhatsApp será enviado manualmente pelo operador", nota);

                // Delivery: imprimir cupom para o entregador (endereço, telefone, itens, observações)
                var notaCopy = nota;
                var requestCopy = request;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var cliente = await _clienteRepository.GetByIdAsync(requestCopy.Cliente);
                        if (cliente == null)
                        {
                            _logger.LogWarning("Delivery cupom: cliente {ClienteId} não encontrado", requestCopy.Cliente);
                            return;
                        }
                        var endereco = MontarEnderecoCliente(cliente);
                        var telefone = (cliente.Telefone ?? cliente.Celular ?? "").Trim();
                        var itensImpressao = new List<ItemImpressao>();
                        foreach (var it in requestCopy.Itens ?? new List<CriarItemVendaRequest>())
                        {
                            var prod = await _produtoRepository.GetProdutoAsync(it.Codigo);
                            var descricao = prod?.Descricao?.Trim() ?? $"Produto {it.Codigo}";
                            itensImpressao.Add(new ItemImpressao
                            {
                                Codigo = it.Codigo,
                                Descricao = descricao,
                                Quantidade = (int)it.Qtd,
                                Preco = it.Preco,
                                Observacao = it.Observacao
                            });
                        }
                        if (itensImpressao.Count == 0)
                        {
                            _logger.LogWarning("Delivery cupom: pedido {Nota} sem itens para imprimir", notaCopy);
                            return;
                        }
                        var imprimirReq = new ImprimirPedidoRequest
                        {
                            Itens = itensImpressao,
                            ApenasNovosItens = false,
                            ClienteNome = cliente.Nome?.Trim() ?? "",
                            IsCupomDelivery = true,
                            EnderecoEntrega = endereco,
                            PontoReferencia = string.IsNullOrWhiteSpace(cliente.Complemento1) ? null : cliente.Complemento1.Trim(),
                            TelefoneEntrega = string.IsNullOrWhiteSpace(telefone) ? null : telefone,
                            ObservacoesPedido = null,
                            FormaPgtoDelivery = string.IsNullOrWhiteSpace(requestCopy.FormasPgto) ? null : requestCopy.FormasPgto.Trim().ToUpperInvariant(),
                            JaPagoDelivery = requestCopy.JaPagoDelivery
                        };
                        var impresso = await _impressaoService.ImprimirPedidoAsync(notaCopy, imprimirReq);
                        if (impresso)
                            _logger.LogInformation("✅ Cupom delivery impresso para o pedido {Nota}", notaCopy);
                        else
                            _logger.LogWarning("⚠️ Falha ao imprimir cupom delivery do pedido {Nota}", notaCopy);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Erro ao imprimir cupom delivery do pedido {Nota}", notaCopy);
                    }
                });
            }

            return vendaDto;
        }
        catch
        {
            // Rollback em caso de erro
            transaction.Rollback();
            _logger.LogError($"❌ Erro ao criar venda {nota} - Transação revertida");
            throw;
        }
    }

    public async Task<VendaDto> AtualizarVendaAsync(string nota, CriarVendaRequest request)
    {
        if (string.IsNullOrWhiteSpace(nota))
        {
            throw new ArgumentException("Nota não pode ser nula ou vazia", nameof(nota));
        }

        _logger.LogInformation($"🔄 Atualizando venda - Nota recebida: '{nota}' (tamanho: {nota.Length})");

        // Verificar se a venda existe - o repositório já faz a normalização internamente
        var vendaExistente = await _vendaRepository.GetVendaAsync(nota);
        
        if (vendaExistente == null)
        {
            // Tentar também sem zeros à esquerda
            var notaSemZeros = nota.TrimStart('0');
            if (string.IsNullOrEmpty(notaSemZeros))
            {
                notaSemZeros = "0";
            }
            
            if (notaSemZeros != nota)
            {
                _logger.LogInformation($"🔍 Tentando buscar com nota sem zeros: '{notaSemZeros}'");
                vendaExistente = await _vendaRepository.GetVendaAsync(notaSemZeros);
            }
        }
        
        if (vendaExistente == null)
        {
            // Buscar sem filtro de status para verificar se a venda existe
            var vendaQualquerStatus = await _vendaRepository.GetVendaSemFiltroStatusAsync(nota);
            
            if (vendaQualquerStatus != null)
            {
                // Delivery com status 'SAINDO' (motoboy a caminho) pode ser editado
                var statusUpper = vendaQualquerStatus.Lancado?.ToUpper() ?? "";
                var origemUpper = vendaQualquerStatus.Origem?.ToUpper() ?? "";
                var requestOrigemUpper = request.Origem?.ToUpper() ?? "";
                
                if (statusUpper == "SAINDO" && (origemUpper == "DL" || requestOrigemUpper == "DL"))
                {
                    _logger.LogInformation($"🚚 Venda '{nota}' é delivery com status 'SAINDO' - permitindo edição");
                    vendaExistente = vendaQualquerStatus;
                }
                else
                {
                    _logger.LogError($"❌ Venda '{nota}' encontrada mas com status '{vendaQualquerStatus.Lancado}' (esperado: 'ABERTO')");
                    throw new Exception($"Venda '{nota}' existe mas está com status '{vendaQualquerStatus.Lancado}'. Apenas vendas com status 'ABERTO' podem ser atualizadas.");
                }
            }
            else
            {
                _logger.LogError($"❌ Venda '{nota}' não encontrada no banco de dados.");
                _logger.LogError($"   Tentativas realizadas:");
                _logger.LogError($"   - Busca com nota: '{nota}'");
                if (nota.TrimStart('0') != nota)
                {
                    _logger.LogError($"   - Busca com nota sem zeros: '{nota.TrimStart('0')}'");
                }
                _logger.LogError($"   - Busca com nota com zeros: '{nota.PadLeft(6, '0')}'");
                throw new Exception($"Venda '{nota}' não encontrada no banco de dados. Verifique se a nota está correta.");
            }
        }
        
        _logger.LogInformation($"✅ Venda encontrada: '{vendaExistente.Nota}' (status: {vendaExistente.Lancado}) - buscada com: '{nota}'");

        // Usar a nota normalizada da venda existente (como está no banco)
        var notaNormalizada = vendaExistente.Nota;
        _logger.LogInformation($"📝 Usando nota normalizada: '{notaNormalizada}' (original recebida: '{nota}')");

        var operadorId = request.Operador == 0 ? 1 : request.Operador;

        // Buscar produtos para obter descrições e códigos de barras
        var codigosProdutos = request.Itens.Select(i => i.Codigo).Distinct().ToList();
        var produtos = new Dictionary<int, Produto>();
        
        foreach (var codigo in codigosProdutos)
        {
            var produto = await _produtoRepository.GetProdutoAsync(codigo);
            if (produto != null)
            {
                produtos[codigo] = produto;
            }
        }

        // Validar request
        if (request.Itens == null || request.Itens.Count == 0)
        {
            _logger.LogError("❌ Request sem itens para atualizar");
            throw new Exception("A venda deve conter pelo menos um item");
        }

        // O repositório faz o merge inteligente: preserva timestamps das linhas existentes
        // e cria novas linhas (com timestamp atual) apenas para os acréscimos. O timestamp
        // que passamos aqui é usado somente quando o item NÃO existe ainda no banco.

        // Criar lista descritiva dos novos itens para o repositório
        var novosItensTemporarios = new List<ItemVendaTemporario>();
        for (int i = 0; i < request.Itens.Count; i++)
        {
            var itemRequest = request.Itens[i];

            // Validar item
            if (itemRequest.Codigo <= 0)
            {
                _logger.LogError($"❌ Item {i + 1} com código inválido: {itemRequest.Codigo}");
                throw new Exception($"Item {i + 1} possui código inválido");
            }
            if (itemRequest.Qtd <= 0)
            {
                _logger.LogError($"❌ Item {i + 1} com quantidade inválida: {itemRequest.Qtd}");
                throw new Exception($"Item {i + 1} possui quantidade inválida");
            }
            if (itemRequest.Preco < 0)
            {
                _logger.LogError($"❌ Item {i + 1} com preço inválido: {itemRequest.Preco}");
                throw new Exception($"Item {i + 1} possui preço inválido");
            }
            
            var produto = produtos.ContainsKey(itemRequest.Codigo) ? produtos[itemRequest.Codigo] : null;
            
            // Normalizar descrição - remover quebras de linha e espaços múltiplos
            var descricao = produto?.Descricao ?? "";
            if (!string.IsNullOrWhiteSpace(descricao))
            {
                descricao = System.Text.RegularExpressions.Regex.Replace(descricao, @"\s+", " ").Trim();
            }
            
            // Se não tiver descrição do produto, usar uma descrição padrão
            if (string.IsNullOrWhiteSpace(descricao))
            {
                descricao = $"Produto {itemRequest.Codigo}";
                _logger.LogWarning($"⚠️ Produto {itemRequest.Codigo} sem descrição - usando descrição padrão");
            }

            var dataHoraItem = itemRequest.AdicionadoEm.HasValue
                ? itemRequest.AdicionadoEm.Value.ToLocalTime()
                : DateTime.Now;
            var horaItem = dataHoraItem.TimeOfDay;

            var itemTemp = new ItemVendaTemporario
            {
                Cupom = notaNormalizada, // Usar nota normalizada da venda existente
                Data = dataHoraItem.Date,
                Hora = horaItem,
                Operador = operadorId,
                Item = i + 1,
                Codigo = itemRequest.Codigo,
                Barras = produto?.CodigoBarra ?? "",
                Descricao = descricao,
                Qtd = itemRequest.Qtd,
                Preco = itemRequest.Preco,
                Tributacao = "",
                Icms = 0,
                Iss = 0,
                Und = produto?.UnMedida ?? "UN",
                Desconto = 0,
                Acrescimo = 0,
                Total = itemRequest.Preco * itemRequest.Qtd,
                // Tabela temporária usa coluna SERIAL para guardar observação do item
                Serial = !string.IsNullOrWhiteSpace(itemRequest.Observacao) ? itemRequest.Observacao.Trim() : "",
                Observacao = itemRequest.Observacao,
                Tipo = 1
            };
            
            // Validação: garantir que sempre tenha data e hora
            if (itemTemp.Data == default(DateTime))
            {
                itemTemp.Data = DateTime.Now.Date;
                _logger.LogWarning($"⚠️ Data do item {i + 1} estava vazia - corrigida para data atual");
            }
            if (itemTemp.Hora == default(TimeSpan))
            {
                itemTemp.Hora = DateTime.Now.TimeOfDay;
                _logger.LogWarning($"⚠️ Hora do item {i + 1} estava vazia - corrigida para hora atual");
            }
            
            // Validar item criado
            if (string.IsNullOrEmpty(itemTemp.Cupom))
            {
                throw new Exception($"Item {i + 1}: Cupom não pode ser vazio");
            }
            if (itemTemp.Operador <= 0)
            {
                throw new Exception($"Item {i + 1}: Operador inválido: {itemTemp.Operador}");
            }
            
            novosItensTemporarios.Add(itemTemp);
            _logger.LogInformation($"✅ Item {i + 1} criado: Cod:{itemTemp.Codigo}, Qtd:{itemTemp.Qtd}, Preco:{itemTemp.Preco}, Total:{itemTemp.Total}");
        }

        _logger.LogInformation($"📦 Atualizando {novosItensTemporarios.Count} itens na tabela TEMPORÁRIA (frente_tmpitvendas) para venda {notaNormalizada}");
        _logger.LogInformation($"📋 Detalhes dos itens: {string.Join(", ", novosItensTemporarios.Select(i => $"Cod:{i.Codigo} Qtd:{i.Qtd}"))}");
        
        // Atualizar tudo dentro de uma transação
        using var connection = _connectionFactory.CreateConnection() as FbConnection;
        if (connection == null)
        {
            throw new Exception("Falha ao criar conexão Firebird");
        }

        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Atualizar itens temporários (limpar antigos e criar novos) - usar nota normalizada
            var itensAtualizados = await _itemTemporarioRepository.AtualizarItensTemporariosAsync(notaNormalizada, operadorId, novosItensTemporarios, transaction);
            if (!itensAtualizados)
            {
                throw new Exception("Erro ao atualizar itens temporários da venda");
            }
            _logger.LogInformation($"✅ Itens temporários atualizados com sucesso!");

            // Recalcular totais baseado nos novos itens
            decimal totalProdutos = novosItensTemporarios.Sum(i => i.Total);
            decimal totalFinal = totalProdutos - request.Desconto + request.Acrescimo;
            
            _logger.LogInformation($"💰 Recalculando totais - Total Produtos: {totalProdutos}, Desconto: {request.Desconto}, Acrescimo: {request.Acrescimo}, Total Final: {totalFinal}");
            
            // Atualizar total, cliente e nome do cliente na tabela vendas dentro da transação
            // Fallback: se a coluna nome_cliente não existir no banco legado, atualiza sem ela
            const string sqlAtualizarVendaComNome = @"
                UPDATE vendas 
                SET tot_produtos = @TotalProdutos, total = @Total, cliente = @Cliente, nome_cliente = @NomeCliente
                WHERE nota = @Nota AND lancado IN ('ABERTO', 'SAINDO')";
            const string sqlAtualizarVendaSemNome = @"
                UPDATE vendas 
                SET tot_produtos = @TotalProdutos, total = @Total, cliente = @Cliente
                WHERE nota = @Nota AND lancado IN ('ABERTO', 'SAINDO')";

            var paramsComNome = new
            {
                Nota = notaNormalizada,
                TotalProdutos = totalProdutos,
                Total = totalFinal,
                Cliente = request.Cliente >= 0 ? request.Cliente : 0,
                NomeCliente = string.IsNullOrWhiteSpace(request.NomeCliente) ? null : request.NomeCliente.Trim()
            };
            var paramsSemNome = new
            {
                Nota = notaNormalizada,
                TotalProdutos = totalProdutos,
                Total = totalFinal,
                Cliente = request.Cliente >= 0 ? request.Cliente : 0
            };

            int linhasAtualizadas;
            if (VendaRepository.NomeClienteColumnMissing)
            {
                linhasAtualizadas = await connection.ExecuteAsync(sqlAtualizarVendaSemNome, paramsSemNome, transaction: transaction);
            }
            else
            {
                try
                {
                    linhasAtualizadas = await connection.ExecuteAsync(sqlAtualizarVendaComNome, paramsComNome, transaction: transaction);
                }
                catch (Exception ex) when (ex.Message?.ToUpperInvariant().Contains("NOME_CLIENTE") == true
                                        || (ex.Message?.ToUpperInvariant().Contains("COLUMN") == true
                                            && ex.Message?.ToUpperInvariant().Contains("UNKNOWN") == true))
                {
                    VendaRepository.NomeClienteColumnMissing = true;
                    _logger.LogWarning("⚠️ Coluna nome_cliente não existe na tabela vendas; atualizando sem ela.");
                    linhasAtualizadas = await connection.ExecuteAsync(sqlAtualizarVendaSemNome, paramsSemNome, transaction: transaction);
                }
            }

            if (linhasAtualizadas == 0)
            {
                throw new Exception($"Não foi possível atualizar o total da venda {notaNormalizada}");
            }

            _logger.LogInformation($"✅ Total da venda {notaNormalizada} atualizado com sucesso para R$ {totalFinal}");

            // Commit da transação
            transaction.Commit();

            _logger.LogInformation($"✅ Venda {nota} atualizada com sucesso!");

            // Buscar venda atualizada para retornar com os valores corretos
            var vendaAtualizada = await _vendaRepository.GetVendaAsync(notaNormalizada);
            if (vendaAtualizada == null)
            {
                _logger.LogWarning($"⚠️ Não foi possível buscar venda atualizada {notaNormalizada}, retornando venda existente");
                return _mapper.Map<VendaDto>(vendaExistente);
            }

            return _mapper.Map<VendaDto>(vendaAtualizada);
        }
        catch
        {
            // Rollback em caso de erro
            transaction.Rollback();
            _logger.LogError($"❌ Erro ao atualizar venda {notaNormalizada} - Transação revertida");
            throw;
        }
    }

    public async Task<VendaDto?> GetVendaAsync(string nota)
    {
        var venda = await _vendaRepository.GetVendaAsync(nota);
        // Para reimpressão de recibos e relatórios: se não encontrou venda aberta, buscar por qualquer status (fechada)
        if (venda == null)
        {
            venda = await _vendaRepository.GetVendaSemFiltroStatusAsync(nota);
        }
        if (venda == null) return null;

        // Se a venda está aberta ou saindo para entrega, buscar itens temporários
        // Se estiver fechada, buscar itens finalizados
        if (venda.Lancado == "ABERTO" || venda.Lancado == "SAINDO")
        {
            // Buscar itens temporários sem filtrar por operador (edições podem ser por operadores diferentes do criador)
            var itensTemp = await _itemTemporarioRepository.GetItensPorCupomAsync(nota);
            // Converter ItemVendaTemporario para ItemVenda e buscar descrições
            var itensList = new List<ItemVenda>();
            foreach (var it in itensTemp)
            {
                // Descrição vem diretamente de frente_tmpitvendas.descricao — sem N+1
                itensList.Add(new ItemVenda
                {
                    Nota = it.Cupom,
                    Modelo = "D2",
                    Serie = "",
                    Subserie = "",
                    Origem = "BA",
                    Emissao = it.Data,
                    Item = it.Item,
                    Codigo = it.Codigo,
                    Barras = it.Barras ?? "",
                    Descricao = it.Descricao,
                    Cfop = 5102,
                    St = "0000",
                    Und = it.Und ?? "UN",
                    Qtd = it.Qtd,
                    Preco = it.Preco,
                    Desconto = it.Desconto,
                    Acrescimo = it.Acrescimo,
                    Total = it.Total,
                    Cancelado = "0",
                    Sequencia = 0,
                    PrecoCusto = 0,
                    Serial = it.Serial ?? "",
                    Icms = it.Icms,
                    Sinalm = -1
                });
            }
            venda.Itens = itensList;
        }
        else
        {
            var itens = await _vendaRepository.GetItensVendaAsync(nota);
            venda.Itens = itens.ToList();
        }

        var vendaDto = _mapper.Map<VendaDto>(venda);
        
        // Fallback: busca em lote as descrições ausentes (uma única query para todos os itens)
        if (vendaDto.Itens != null)
        {
            var itensSemDescricao = vendaDto.Itens
                .Where(i => string.IsNullOrWhiteSpace(i.Descricao))
                .ToList();
            if (itensSemDescricao.Count > 0)
            {
                try
                {
                    var codigos = itensSemDescricao.Select(i => i.Codigo).Distinct();
                    var descricoes = await _produtoRepository.GetDescricoesPorCodigosAsync(codigos);
                    foreach (var itemDto in itensSemDescricao)
                    {
                        if (descricoes.TryGetValue(itemDto.Codigo, out var desc) && !string.IsNullOrEmpty(desc))
                            itemDto.Descricao = desc;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao buscar descrições em lote para itens sem descrição");
                }
            }
        }
        
        // Garantir nome do cliente: quando Cliente=0 usar nome_cliente da venda; quando Cliente>0 buscar do cadastro
        if (vendaDto.Cliente > 0)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(vendaDto.Cliente);
                if (cliente != null)
                {
                    if (!string.IsNullOrEmpty(cliente.Nome))
                    {
                        vendaDto.NomeCliente = cliente.Nome;
                    }
                    // Buscar telefone (prioriza celular, depois telefone)
                    vendaDto.TelefoneCliente = !string.IsNullOrEmpty(cliente.Celular) 
                        ? cliente.Celular 
                        : cliente.Telefone;
                    // Para delivery: endereço de entrega (reimpressão de recibo com endereço)
                    var origemVenda = (venda.Origem ?? "").Trim().ToUpperInvariant();
                    if (origemVenda == "DL")
                    {
                        vendaDto.EnderecoEntrega = MontarEnderecoCliente(cliente);
                        vendaDto.PontoReferencia = string.IsNullOrWhiteSpace(cliente.Complemento1) ? null : cliente.Complemento1.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar cliente {ClienteId} para venda {Nota}", vendaDto.Cliente, nota);
            }

            // Verificar se o cliente possui contas a receber em aberto (valor devido)
            try
            {
                var (temContasAberto, valorTotalPendente, quantidadeContas) = 
                    await _receberRepository.VerificarContasAbertoAsync(vendaDto.Cliente);
                if (temContasAberto)
                {
                    vendaDto.ContasAberto = new ContasAbertoDto
                    {
                        TemContasAberto = true,
                        ValorTotalPendente = valorTotalPendente,
                        QuantidadeContas = quantidadeContas,
                        Mensagem = $"Cliente possui {quantidadeContas} conta(s) a receber em aberto no valor total de R$ {valorTotalPendente:N2}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao verificar contas em aberto do cliente {ClienteId} para venda {Nota}", vendaDto.Cliente, nota);
            }
        }
        else
        {
            // Cliente não cadastrado (0): usar nome_cliente gravado na venda (entidade ou consulta direta)
            if (!string.IsNullOrWhiteSpace(venda.NomeCliente))
                vendaDto.NomeCliente = venda.NomeCliente.Trim();
            else
            {
                var nomeClienteDb = await _vendaRepository.GetNomeClientePorNotaAsync(nota);
                if (!string.IsNullOrWhiteSpace(nomeClienteDb))
                    vendaDto.NomeCliente = nomeClienteDb;
            }
        }

        // Popular nome do estabelecimento para mensagem WhatsApp
        try
        {
            var emitente = await _emitenteRepository.GetEmitenteAsync();
            vendaDto.NomeEstabelecimento = (!string.IsNullOrWhiteSpace(emitente?.NomeFantasia)
                ? emitente.NomeFantasia
                : emitente?.Nome)?.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao buscar nome do emitente para venda {Nota}", nota);
        }

        return vendaDto;
    }

    private async Task PreencherNomesClientesAsync(List<VendaDto> vendasDto)
    {
        var clientesIds = vendasDto.Where(v => v.Cliente > 0).Select(v => v.Cliente).Distinct().ToList();
        if (!clientesIds.Any()) return;

        // Buscar clientes em lote
        var clientesDict = new Dictionary<int, (string Nome, string? Telefone)>();
        foreach (var clienteId in clientesIds)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(clienteId);
                if (cliente != null)
                {
                    var nome = cliente.Nome ?? string.Empty;
                    var telefone = !string.IsNullOrEmpty(cliente.Celular) 
                        ? cliente.Celular 
                        : cliente.Telefone;
                    clientesDict[clienteId] = (nome, telefone);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar cliente {ClienteId}", clienteId);
            }
        }

        // Preencher nomes e telefones nas vendas
        foreach (var vendaDto in vendasDto)
        {
            if (vendaDto.Cliente > 0 && clientesDict.TryGetValue(vendaDto.Cliente, out var dadosCliente))
            {
                vendaDto.NomeCliente = dadosCliente.Nome;
                vendaDto.TelefoneCliente = dadosCliente.Telefone;
            }
        }
    }

    public async Task<IEnumerable<VendaDto>> GetVendasHojeAsync()
    {
        var vendas = await _vendaRepository.GetVendasHojeAsync();
        var vendasDto = _mapper.Map<IEnumerable<VendaDto>>(vendas).ToList();
        await PreencherNomesClientesAsync(vendasDto);
        return vendasDto;
    }

    public async Task<IEnumerable<VendaFechadaReciboDto>> GetVendasFechadasPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        var vendas = await _vendaRepository.GetVendasFechadasPorPeriodoAsync(dataInicio, dataFim);
        var resultado = new List<VendaFechadaReciboDto>();
        foreach (var venda in vendas)
        {
            var recebimentos = (await _recebimentoService.GetRecebimentosPorNotaAsync(venda.Nota)).ToList();
            string? nomeCliente = null;
            if (venda.Cliente > 0)
            {
                try
                {
                    var cliente = await _clienteRepository.GetByIdAsync(venda.Cliente);
                    nomeCliente = cliente?.Nome;
                }
                catch { /* ignorar */ }
            }
            // Data/hora do fechamento (data_saida + hora_saida) para exibir na reimpressão
            var dataHoraFechamento = venda.DataSaida.Add(venda.HoraSaida);

            resultado.Add(new VendaFechadaReciboDto
            {
                Nota = venda.Nota,
                Comanda = venda.Comanda,
                Total = venda.Total,
                Emissao = dataHoraFechamento,
                NomeCliente = nomeCliente,
                Recebimentos = recebimentos
            });
        }
        return resultado;
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorComandaAsync(int comanda)
    {
        var vendas = await _vendaRepository.GetVendasPorComandaAsync(comanda);
        var vendasDto = _mapper.Map<IEnumerable<VendaDto>>(vendas).ToList();
        await PreencherNomesClientesAsync(vendasDto);
        return vendasDto;
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorMesaAsync(int mesa)
    {
        var vendas = await _vendaRepository.GetVendasPorMesaAsync(mesa);
        var vendasDto = _mapper.Map<IEnumerable<VendaDto>>(vendas).ToList();
        await PreencherNomesClientesAsync(vendasDto);
        return vendasDto;
    }

    public async Task<IEnumerable<VendaDto>> GetVendasAbertasAsync(string? origem = null)
    {
        var vendas = await _vendaRepository.GetVendasAbertasAsync(origem);
        var vendasList = vendas.ToList();
        
        // Para cada venda, verificar se o total está correto baseado nos itens temporários
        var vendasDto = new List<VendaDto>();
        foreach (var venda in vendasList)
        {
            try
            {
                // Buscar itens temporários para recalcular o total e incluir na resposta
                // Sem filtro por operador: edições por operadores diferentes do criador devem ser visíveis
                var itensTemporarios = await _itemTemporarioRepository.GetItensPorCupomAsync(venda.Nota);
                var itensList = itensTemporarios.ToList();
                
                // Converter itens temporários para ItemVenda
                venda.Itens = itensList.Select(it => new ItemVenda
                {
                    Nota = it.Cupom,
                    Modelo = "D2",
                    Serie = "",
                    Subserie = "",
                    Origem = "BA",
                    Emissao = it.Data,
                    Item = it.Item,
                    Codigo = it.Codigo,
                    Barras = it.Barras ?? "",
                    Cfop = 5102,
                    St = "0000",
                    Und = it.Und ?? "UN",
                    Qtd = it.Qtd,
                    Preco = it.Preco,
                    Desconto = it.Desconto,
                    Acrescimo = it.Acrescimo,
                    Total = it.Total,
                    Cancelado = "0",
                    Sequencia = 0,
                    PrecoCusto = 0,
                    Serial = it.Serial ?? "",
                    Icms = it.Icms,
                    Sinalm = -1
                }).ToList();
                
                if (itensList.Count > 0)
                {
                    // Recalcular total baseado nos itens temporários
                    var totalCalculado = itensList.Sum(i => i.Total);
                    var totalFinal = totalCalculado - venda.Desconto + venda.Acrescimo;
                    
                    // Se o total no banco estiver diferente do calculado, atualizar
                    var diferenca = Math.Abs(venda.Total - totalFinal);
                    if (diferenca > 0.01m)
                    {
                        _logger.LogWarning($"⚠️ Total inconsistente para venda {venda.Nota}: Banco={venda.Total}, Calculado={totalFinal}. Atualizando...");
                        
                        // Atualizar o total no banco
                        var atualizado = await _vendaRepository.AtualizarTotalVendaAsync(venda.Nota, totalCalculado, totalFinal);
                        if (atualizado)
                        {
                            venda.TotProdutos = totalCalculado;
                            venda.Total = totalFinal;
                            _logger.LogInformation($"✅ Total da venda {venda.Nota} corrigido de {venda.Total} para {totalFinal}");
                        }
                    }
                }
                
                vendasDto.Add(_mapper.Map<VendaDto>(venda));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ Erro ao processar venda {venda.Nota}, usando valor do banco");
                // Continuar com o valor do banco se houver erro
                vendasDto.Add(_mapper.Map<VendaDto>(venda));
            }
        }
        
        await PreencherNomesClientesAsync(vendasDto);
        return vendasDto;
    }

    public async Task<IEnumerable<VendaDto>> GetVendasCanceladasAsync(string? origem = null, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var vendas = await _vendaRepository.GetVendasCanceladasAsync(origem, dataInicio, dataFim);
        var vendasDto = _mapper.Map<IEnumerable<VendaDto>>(vendas).ToList();
        await PreencherNomesClientesAsync(vendasDto);
        return vendasDto;
    }

    public async Task<ConferenciaMesaDto?> GetConferenciaMesaAsync(int mesa)
    {
        _logger.LogInformation($"📋 Buscando conferência da mesa {mesa}");

        // Buscar venda aberta da mesa
        var venda = await _vendaRepository.GetVendaAbertaPorMesaAsync(mesa);
        if (venda == null)
        {
            _logger.LogWarning($"❌ Nenhuma venda aberta encontrada para mesa {mesa}");
            return null;
        }

        // Buscar itens temporários sem filtrar por operador (correção bug dupla edição)
        var itensTemp = await _itemTemporarioRepository.GetItensPorCupomAsync(venda.Nota);
        
        // Buscar produtos para preencher descrições vazias
        var codigosProdutos = itensTemp.Select(i => i.Codigo).Distinct().ToList();
        var produtos = new Dictionary<int, Produto>();
        
        foreach (var codigo in codigosProdutos)
        {
            var produto = await _produtoRepository.GetProdutoAsync(codigo);
            if (produto != null)
            {
                produtos[codigo] = produto;
            }
        }
        
        // Buscar cliente da venda
        ClienteConferenciaDto? clienteDto = null;
        if (venda.Cliente > 0)
        {
            var cliente = await _clienteRepository.GetByIdAsync(venda.Cliente);
            if (cliente != null)
            {
                clienteDto = new ClienteConferenciaDto
                {
                    Id = cliente.Id,
                    Nome = cliente.Nome ?? "",
                    CpfCnpj = cliente.CpfCnpj,
                    Telefone = cliente.Telefone ?? cliente.Celular
                };
            }
        }

        var conferencia = new ConferenciaMesaDto
        {
            // Garantir que a nota seja formatada com 6 dígitos (zeros à esquerda)
            Nota = venda.Nota.PadLeft(6, '0'),
            Mesa = mesa,
            Comanda = venda.Comanda,
            Garcom = $"Operador {venda.Operador}", // TODO: buscar nome do usuário
            DataHora = venda.Emissao.Add(venda.Hora),
            Itens = itensTemp.Select(i => 
            {
                // Se a descrição estiver vazia, buscar do produto
                var descricao = i.Descricao;
                if (string.IsNullOrWhiteSpace(descricao) && produtos.ContainsKey(i.Codigo))
                {
                    descricao = produtos[i.Codigo].Descricao ?? "";
                    // Normalizar descrição
                    if (!string.IsNullOrWhiteSpace(descricao))
                    {
                        descricao = System.Text.RegularExpressions.Regex.Replace(descricao, @"\s+", " ").Trim();
                    }
                }
                
                return new ItemConferenciaDto
                {
                    Codigo = i.Codigo,
                    Descricao = descricao,
                    Qtd = i.Qtd,
                    PrecoUnitario = i.Preco,
                    Total = i.Total,
                    Observacao = !string.IsNullOrWhiteSpace(i.Observacao) ? i.Observacao : i.Serial,
                    HoraLancamento = (i.Data.Date + i.Hora).ToString("HH:mm:ss")
                };
            }).ToList(),
            Subtotal = venda.TotProdutos,
            Desconto = venda.Desconto,
            Acrescimo = venda.Acrescimo,
            Total = venda.Total,
            TotalItens = itensTemp.Count(),
            Cliente = clienteDto
        };

        _logger.LogInformation($"✅ Conferência da mesa {mesa}: {conferencia.TotalItens} itens, Total: R$ {conferencia.Total:N2}");

        return conferencia;
    }

    public async Task<ConferenciaMesaDto?> GetConferenciaComandaAsync(int comanda)
    {
        _logger.LogInformation($"📋 Buscando conferência da comanda {comanda}");

        // Buscar venda aberta da comanda
        var venda = await _vendaRepository.GetVendaAbertaPorComandaAsync(comanda);
        if (venda == null)
        {
            _logger.LogWarning($"❌ Nenhuma venda aberta encontrada para comanda {comanda}");
            return null;
        }

        // Buscar itens temporários sem filtrar por operador (correção bug dupla edição)
        var itensTemp = await _itemTemporarioRepository.GetItensPorCupomAsync(venda.Nota);
        
        // Buscar produtos para preencher descrições vazias
        var codigosProdutos = itensTemp.Select(i => i.Codigo).Distinct().ToList();
        var produtos = new Dictionary<int, Produto>();
        
        foreach (var codigo in codigosProdutos)
        {
            var produto = await _produtoRepository.GetProdutoAsync(codigo);
            if (produto != null)
            {
                produtos[codigo] = produto;
            }
        }
        
        // Buscar cliente da venda
        ClienteConferenciaDto? clienteDto = null;
        if (venda.Cliente > 0)
        {
            var cliente = await _clienteRepository.GetByIdAsync(venda.Cliente);
            if (cliente != null)
            {
                clienteDto = new ClienteConferenciaDto
                {
                    Id = cliente.Id,
                    Nome = cliente.Nome ?? "",
                    CpfCnpj = cliente.CpfCnpj,
                    Telefone = cliente.Telefone ?? cliente.Celular
                };
            }
        }

        var conferencia = new ConferenciaMesaDto
        {
            // Garantir que a nota seja formatada com 6 dígitos (zeros à esquerda)
            Nota = venda.Nota.PadLeft(6, '0'),
            Mesa = venda.Mesa,
            Comanda = comanda,
            Garcom = $"Operador {venda.Operador}", // TODO: buscar nome do usuário
            DataHora = venda.Emissao.Add(venda.Hora),
            Itens = itensTemp.Select(i => 
            {
                // Se a descrição estiver vazia, buscar do produto
                var descricao = i.Descricao;
                if (string.IsNullOrWhiteSpace(descricao) && produtos.ContainsKey(i.Codigo))
                {
                    descricao = produtos[i.Codigo].Descricao ?? "";
                    // Normalizar descrição
                    if (!string.IsNullOrWhiteSpace(descricao))
                    {
                        descricao = System.Text.RegularExpressions.Regex.Replace(descricao, @"\s+", " ").Trim();
                    }
                }
                
                return new ItemConferenciaDto
                {
                    Codigo = i.Codigo,
                    Descricao = descricao,
                    Qtd = i.Qtd,
                    PrecoUnitario = i.Preco,
                    Total = i.Total,
                    Observacao = !string.IsNullOrWhiteSpace(i.Observacao) ? i.Observacao : i.Serial,
                    HoraLancamento = (i.Data.Date + i.Hora).ToString("HH:mm:ss")
                };
            }).ToList(),
            Subtotal = venda.TotProdutos,
            Desconto = venda.Desconto,
            Acrescimo = venda.Acrescimo,
            Total = venda.Total,
            TotalItens = itensTemp.Count(),
            Cliente = clienteDto
        };

        _logger.LogInformation($"✅ Conferência da comanda {comanda}: {conferencia.TotalItens} itens, Total: R$ {conferencia.Total:N2}");

        return conferencia;
    }

    public async Task<bool> VerificarComandaAbertaAsync(int comanda)
    {
        _logger.LogInformation($"🔍 Verificando se comanda {comanda} está aberta");

        var venda = await _vendaRepository.GetVendaAbertaPorComandaAsync(comanda);
        var estaAberta = venda != null;

        _logger.LogInformation($"{(estaAberta ? "✅" : "❌")} Comanda {comanda} está {(estaAberta ? "ABERTA" : "FECHADA")}");

        return estaAberta;
    }

    public async Task<VendaDto> TransferirItemAsync(TransferirItemRequest request)
    {
        _logger.LogInformation($"🔄 Transferindo item {request.ItemOrigem} da venda {request.NotaOrigem} para {request.NotaDestino}");

        // Validar request
        if (string.IsNullOrWhiteSpace(request.NotaOrigem))
        {
            throw new ArgumentException("Nota de origem não pode ser nula ou vazia", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.NotaDestino))
        {
            throw new ArgumentException("Nota de destino não pode ser nula ou vazia", nameof(request));
        }

        if (request.NotaOrigem == request.NotaDestino)
        {
            throw new ArgumentException("A nota de origem e destino não podem ser iguais");
        }

        if (request.ItemOrigem <= 0)
        {
            throw new ArgumentException("Item de origem deve ser maior que zero", nameof(request));
        }

        // Verificar se ambas as vendas existem e estão abertas
        var vendaOrigem = await _vendaRepository.GetVendaAsync(request.NotaOrigem);
        if (vendaOrigem == null)
        {
            throw new Exception($"Venda de origem {request.NotaOrigem} não encontrada");
        }

        if (vendaOrigem.Lancado != "ABERTO")
        {
            throw new Exception($"Venda de origem {request.NotaOrigem} não está aberta (status: {vendaOrigem.Lancado})");
        }

        var vendaDestino = await _vendaRepository.GetVendaAsync(request.NotaDestino);
        if (vendaDestino == null)
        {
            throw new Exception($"Venda de destino {request.NotaDestino} não encontrada");
        }

        if (vendaDestino.Lancado != "ABERTO")
        {
            throw new Exception($"Venda de destino {request.NotaDestino} não está aberta (status: {vendaDestino.Lancado})");
        }

        // Usar as notas normalizadas do banco (o repositório já retorna a nota no formato correto)
        var notaOrigemNormalizada = vendaOrigem.Nota;
        var notaDestinoNormalizada = vendaDestino.Nota;

        if (notaOrigemNormalizada == notaDestinoNormalizada)
        {
            throw new ArgumentException("A nota de origem e destino não podem ser iguais");
        }

        // Verificar se o item existe na venda origem (usar nota normalizada)
        var item = await _itemTemporarioRepository.GetItemPorCupomEItemAsync(
            notaOrigemNormalizada, 
            request.ItemOrigem, 
            request.Operador);

        if (item == null)
        {
            throw new Exception($"Item {request.ItemOrigem} não encontrado na venda {notaOrigemNormalizada}");
        }

        // Transferir o item (usar notas normalizadas)
        var transferido = await _itemTemporarioRepository.TransferirItemAsync(
            notaOrigemNormalizada,
            request.ItemOrigem,
            notaDestinoNormalizada,
            request.Operador);

        if (!transferido)
        {
            throw new Exception("Erro ao transferir item");
        }

        _logger.LogInformation($"✅ Item {request.ItemOrigem} transferido com sucesso de {notaOrigemNormalizada} para {notaDestinoNormalizada}");

        // Recalcular totais de ambas as vendas (usar notas normalizadas)
        await RecalcularTotaisVendaAsync(notaOrigemNormalizada);
        await RecalcularTotaisVendaAsync(notaDestinoNormalizada);

        // Retornar a venda de destino atualizada
        var vendaAtualizada = await GetVendaAsync(notaDestinoNormalizada);
        if (vendaAtualizada == null)
        {
            throw new Exception("Erro ao buscar venda atualizada após transferência");
        }

        return vendaAtualizada;
    }

    private async Task RecalcularTotaisVendaAsync(string nota)
    {
        // Buscar todos os itens temporários da venda (sem filtro de operador)
        var venda = await _vendaRepository.GetVendaAsync(nota);
        if (venda == null) return;

        var itens = await _itemTemporarioRepository.GetItensPorCupomAsync(nota);
        
        var totProdutos = itens.Sum(i => i.Total);
        var total = totProdutos + venda.Acrescimo - venda.Desconto;

        // Atualizar totais na venda
        await _vendaRepository.AtualizarTotalVendaAsync(nota, totProdutos, total);
    }

    public async Task<VendaDto> CancelarItemAsync(CancelarItemRequest request)
    {
        _logger.LogInformation($"🗑️ Cancelando item {request.Item} da venda {request.Nota}");

        // Validar request
        if (string.IsNullOrWhiteSpace(request.Nota))
        {
            throw new ArgumentException("Nota não pode ser nula ou vazia");
        }

        if (request.Item <= 0)
        {
            throw new ArgumentException("Item deve ser maior que zero");
        }

        // Buscar venda
        var venda = await _vendaRepository.GetVendaAsync(request.Nota);
        if (venda == null)
        {
            throw new InvalidOperationException($"Venda {request.Nota} não encontrada ou não está aberta");
        }

        if (venda.Lancado != "ABERTO")
        {
            throw new InvalidOperationException($"Venda {request.Nota} não está aberta (status: {venda.Lancado}). Apenas vendas abertas podem ter itens cancelados.");
        }

        // Verificar se o item existe
        var item = await _itemTemporarioRepository.GetItemPorCupomEItemAsync(
            venda.Nota, 
            request.Item, 
            request.Operador);

        if (item == null)
        {
            throw new InvalidOperationException($"Item {request.Item} não encontrado na venda {request.Nota}");
        }

        // Usar transação para garantir atomicidade
        using var connection = _connectionFactory.CreateConnection() as FbConnection;
        if (connection == null)
        {
            throw new Exception("Falha ao criar conexão Firebird");
        }

        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Cancelar o item
            var cancelado = await _itemTemporarioRepository.CancelarItemAsync(
                venda.Nota, 
                request.Item, 
                request.Operador, 
                transaction);

            if (!cancelado)
            {
                throw new Exception("Erro ao cancelar item");
            }

            // Recalcular totais (sem filtro por operador, para incluir itens de qualquer operador)
            var itensRestantes = await _itemTemporarioRepository.GetItensPorCupomAsync(venda.Nota);
            var totProdutos = itensRestantes.Sum(i => i.Total);
            var totalFinal = totProdutos - venda.Desconto + venda.Acrescimo;

            // Se não houver mais itens, cancelar a venda também
            if (!itensRestantes.Any())
            {
                _logger.LogWarning($"⚠️ Nenhum item restante na venda {request.Nota} após cancelamento - cancelando venda");
                await _vendaRepository.AtualizarStatusVendaAsync(venda.Nota, "CANCELADO", transaction);
            }
            else
            {
                // Atualizar totais
                var sqlAtualizarTotal = @"
                    UPDATE vendas 
                    SET tot_produtos = @TotalProdutos, total = @Total, quantidade = @Quantidade
                    WHERE nota = @Nota AND lancado = 'ABERTO'";

                await connection.ExecuteAsync(sqlAtualizarTotal, new
                {
                    Nota = venda.Nota,
                    TotalProdutos = totProdutos,
                    Total = totalFinal,
                    Quantidade = itensRestantes.Count().ToString()
                }, transaction: transaction);
            }

            transaction.Commit();

            _logger.LogInformation($"✅ Item {request.Item} cancelado com sucesso da venda {request.Nota}");

            // Retornar venda atualizada
            var vendaAtualizada = await GetVendaAsync(venda.Nota);
            if (vendaAtualizada == null)
            {
                throw new Exception("Erro ao buscar venda atualizada após cancelamento");
            }

            return vendaAtualizada;
        }
        catch
        {
            transaction.Rollback();
            _logger.LogError($"❌ Erro ao cancelar item {request.Item} da venda {request.Nota} - Transação revertida");
            throw;
        }
    }

    public async Task<bool> CancelarVendaAsync(string nota, int operador, string justificativa = "")
    {
        _logger.LogInformation($"🗑️ Cancelando venda {nota}");

        if (string.IsNullOrWhiteSpace(nota))
        {
            throw new ArgumentException("Nota não pode ser nula ou vazia");
        }

        // Buscar venda
        var venda = await _vendaRepository.GetVendaAsync(nota);
        if (venda == null)
        {
            throw new InvalidOperationException($"Venda {nota} não encontrada ou não está aberta");
        }

        if (venda.Lancado != "ABERTO" && venda.Lancado != "SAINDO")
        {
            throw new InvalidOperationException($"Venda {nota} não pode ser cancelada (status: {venda.Lancado}). Apenas vendas abertas ou em trânsito (delivery) podem ser canceladas.");
        }

        // Usar transação
        using var connection = _connectionFactory.CreateConnection() as FbConnection;
        if (connection == null)
        {
            throw new Exception("Falha ao criar conexão Firebird");
        }

        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Limpar todos os itens temporários
            var itens = await _itemTemporarioRepository.GetItensPorCupomAsync(nota, operador);
            foreach (var item in itens)
            {
                await _itemTemporarioRepository.CancelarItemAsync(nota, item.Item, operador, transaction);
            }

            // Atualizar status da venda para CANCELADO, gravando justificativa
            var cancelado = !string.IsNullOrWhiteSpace(justificativa)
                ? await _vendaRepository.CancelarComComandaAsync(nota, justificativa, transaction)
                : await _vendaRepository.AtualizarStatusVendaAsync(nota, "CANCELADO", transaction);

            transaction.Commit();

            _logger.LogInformation($"✅ Venda {nota} cancelada com sucesso");

            return cancelado;
        }
        catch
        {
            transaction.Rollback();
            _logger.LogError($"❌ Erro ao cancelar venda {nota} - Transação revertida");
            throw;
        }
    }

    public async Task<bool> NotificarSaiuParaEntregaAsync(string nota)
    {
        _logger.LogInformation("🚚 [SAIU_ENTREGA] Iniciando notificação para nota: {Nota}", nota);
        
        if (string.IsNullOrWhiteSpace(nota))
        {
            _logger.LogWarning("NotificarSaiuParaEntrega: nota vazia");
            return false;
        }

        var venda = await _vendaRepository.GetVendaAsync(nota);
        if (venda == null)
        {
            _logger.LogWarning("NotificarSaiuParaEntrega: venda {Nota} não encontrada", nota);
            return false;
        }

        _logger.LogInformation("🚚 [SAIU_ENTREGA] Venda encontrada - Origem: {Origem}, Cliente: {Cliente}", venda.Origem, venda.Cliente);

        var origem = (venda.Origem ?? "").Trim().ToUpperInvariant();
        if (origem != "DL")
        {
            _logger.LogWarning("NotificarSaiuParaEntrega: venda {Nota} não é delivery (origem: {Origem})", nota, venda.Origem);
            return false;
        }

        if (venda.Cliente <= 0)
        {
            _logger.LogWarning("NotificarSaiuParaEntrega: pedido {Nota} sem cliente", nota);
            return false;
        }

        try
        {
            var cliente = await _clienteRepository.GetByIdAsync(venda.Cliente);
            if (cliente == null)
            {
                _logger.LogWarning("NotificarSaiuParaEntrega: cliente {ClienteId} não encontrado", venda.Cliente);
                return false;
            }

            _logger.LogInformation("🚚 [SAIU_ENTREGA] Cliente encontrado - Nome: {Nome}, Tel: {Telefone}, Cel: {Celular}", cliente.Nome, cliente.Telefone, cliente.Celular);

            var telefone = FormatarTelefoneWhatsApp(cliente.Telefone, cliente.Celular);
            
            // Atualizar status da venda ANTES de enviar mensagem (para UI atualizar imediatamente)
            // Usando "SAINDO" que cabe no campo lancado (10 caracteres)
            _logger.LogInformation("🚚 [SAIU_ENTREGA] Atualizando status para SAINDO...");
            await _vendaRepository.AtualizarStatusVendaAsync(nota, "SAINDO");
            _logger.LogInformation("✅ Status da venda {Nota} atualizado para SAINDO", nota);

            // Se cliente não tem telefone, retorna true mesmo assim (status já foi atualizado)
            if (string.IsNullOrEmpty(telefone))
            {
                _logger.LogWarning("NotificarSaiuParaEntrega: cliente {ClienteId} sem telefone - status atualizado mas mensagem não enviada", venda.Cliente);
                return true;
            }

            // Buscar nome fantasia da empresa
            var emitente = await _emitenteRepository.GetEmitenteAsync();
            var nomeEmpresa = !string.IsNullOrWhiteSpace(emitente?.NomeFantasia) 
                ? emitente.NomeFantasia.Trim() 
                : "Restaurante";

            var nomeCliente = string.IsNullOrWhiteSpace(cliente.Nome) ? "Cliente" : cliente.Nome.Trim();
            var mensagem = $"Olá {nomeCliente}, o motoboy saiu nesse momento para fazer sua entrega. Bom apetite.\n\nObrigado pela preferência...\n\n➡ {nomeEmpresa}";
            
            var enviado = await _whatsAppService.EnviarMensagemAsync(telefone, mensagem);
            if (enviado)
            {
                _logger.LogInformation("✅ WhatsApp (saiu para entrega) enviado para o cliente do pedido {Nota}", nota);
            }
            else
            {
                _logger.LogWarning("⚠️ WhatsApp não enviado para o cliente do pedido {Nota}", nota);
            }
            
            // Retorna true pois o status já foi atualizado com sucesso
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar WhatsApp saiu para entrega do pedido {Nota}", nota);
            return false;
        }
    }

    /// <summary>
    /// Cria um novo pedido de delivery (wrapper de CriarVendaAsync com validações e formato específicos para delivery)
    /// </summary>
    public async Task<PedidoDeliveryDto> CriarPedidoDeliveryAsync(CriarPedidoDeliveryRequest request)
    {
        _logger.LogInformation("🚚 [DELIVERY] Criando novo pedido de delivery");

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // Converter CriarPedidoDeliveryRequest para CriarVendaRequest com origem DL
        var vendaRequest = new CriarVendaRequest
        {
            Cliente = request.Cliente,  // Obrigatório para delivery
            Comanda = null,             // Delivery não usa comanda
            Mesa = null,                // Delivery não usa mesa
            NumeroPessoas = null,
            FormasPgto = request.FormasPgto,
            Especie = request.Especie,
            Dinheiro = request.Dinheiro,
            Cartao = request.Cartao,
            Boleto = 0,                 // Delivery não usa boleto normalmente
            Troco = request.Troco,
            Desconto = request.Desconto,
            Acrescimo = request.Acrescimo,
            Operador = request.Operador,
            Vendedor = request.Vendedor,
            Caixa = 1,                  // Caixa padrão
            Itens = request.Itens,
            Origem = "DL"               // 🚚 Marcar como delivery
        };

        // Calcular totais
        var totalProdutos = request.Itens.Sum(i => i.Preco * i.Qtd);
        var totalFinal = totalProdutos - request.Desconto + request.Acrescimo;
        
        vendaRequest.TotProdutos = totalProdutos;
        vendaRequest.Total = totalFinal;

        // Chamar serviço de criação de venda (que já fazará todas as validações de delivery)
        var vendaCriada = await CriarVendaAsync(vendaRequest);

        _logger.LogInformation("✅ [DELIVERY] Pedido criado com sucesso - Nota: {Nota}", vendaCriada.Nota);

        // Converter VendaDto para PedidoDeliveryDto
        var pedidoDto = await ConvertVendaToPedidoDeliveryDtoAsync(vendaCriada);

        return pedidoDto;
    }

    /// <summary>
    /// Lista todos os pedidos de delivery abertos
    /// </summary>
    public async Task<IEnumerable<PedidoDeliveryDto>> GetPedidosDeliveryAbertosAsync()
    {
        _logger.LogInformation("🚚 Listando pedidos de delivery em aberto");

        // Buscar vendas abertas com origem DL
        var vendas = await GetVendasAbertasAsync("DL");
        var vendasList = vendas.ToList();

        _logger.LogInformation("✅ Encontrados {Count} pedidos de delivery em aberto", vendasList.Count);

        // Converter VendaDto para PedidoDeliveryDto
        var pedidos = new List<PedidoDeliveryDto>();
        foreach (var venda in vendasList)
        {
            try
            {
                var pedido = await ConvertVendaToPedidoDeliveryDtoAsync(venda);
                pedidos.Add(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Erro ao converter venda {Nota} para pedido delivery", venda.Nota);
                // Continuar com próximas vendas
            }
        }

        return pedidos;
    }

    /// <summary>
    /// Converte uma VendaDto em PedidoDeliveryDto (formatação para delivery)
    /// </summary>
    private async Task<PedidoDeliveryDto> ConvertVendaToPedidoDeliveryDtoAsync(VendaDto venda)
    {
        // Buscar informações do cliente para endereço
        var cliente = await _clienteRepository.GetByIdAsync(venda.Cliente);
        var enderecoEntrega = cliente != null ? MontarEnderecoCliente(cliente) : string.Empty;
        var telefoneCliente = string.IsNullOrEmpty(cliente?.Celular) 
            ? cliente?.Telefone 
            : cliente?.Celular;

        var pedido = new PedidoDeliveryDto
        {
            Nota = venda.Nota,
            DataHora = venda.Emissao.Add(venda.Hora),
            Cliente = venda.Cliente,
            NomeCliente = venda.NomeCliente,
            TelefoneCliente = telefoneCliente,
            EnderecoEntrega = enderecoEntrega,
            Subtotal = venda.TotProdutos,
            Desconto = venda.Desconto,
            Acrescimo = venda.Acrescimo,
            Total = venda.Total,
            FormasPgto = venda.FormasPgto,
            TotalItens = venda.Itens?.Count ?? 0,
            Lancado = venda.Lancado,
            Operador = venda.Operador,
            Itens = venda.Itens?.Select(i => new ItemPedidoDeliveryDto
            {
                Codigo = i.Codigo,
                Descricao = i.Descricao,
                Quantidade = i.Qtd,
                Preco = i.Preco,
                Total = i.Total,
                Observacao = null // Pode ser adicionado do campo Serial se necessário
            }).ToList() ?? new()
        };

        return pedido;
    }
}
