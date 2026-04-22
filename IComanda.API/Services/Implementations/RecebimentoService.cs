using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using IComanda.API.Data;
using Microsoft.Extensions.Logging;
using FirebirdSql.Data.FirebirdClient;
using System.Data;

namespace IComanda.API.Services.Implementations;

public class RecebimentoService : IRecebimentoService
{
    private readonly IVendaRepository _vendaRepository;
    private readonly IFormaPagamentoRepository _formaPagamentoRepository;
    private readonly IRecebimentoRepository _recebimentoRepository;
    private readonly IReceberRepository _receberRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IItemVendaTemporarioRepository _itemTemporarioRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<RecebimentoService> _logger;

    public RecebimentoService(
        IVendaRepository vendaRepository,
        IFormaPagamentoRepository formaPagamentoRepository,
        IRecebimentoRepository recebimentoRepository,
        IReceberRepository receberRepository,
        IClienteRepository clienteRepository,
        IItemVendaTemporarioRepository itemTemporarioRepository,
        IDbConnectionFactory connectionFactory,
        ILogger<RecebimentoService> logger)
    {
        _vendaRepository = vendaRepository;
        _formaPagamentoRepository = formaPagamentoRepository;
        _recebimentoRepository = recebimentoRepository;
        _receberRepository = receberRepository;
        _clienteRepository = clienteRepository;
        _itemTemporarioRepository = itemTemporarioRepository;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<FecharComandaResponseDto> FecharComandaAsync(FecharComandaRequest request)
    {
        _logger.LogInformation("🔄 Iniciando fechamento - Comanda: {Comanda}, Nota: {Nota}", request.Comanda, request.Nota ?? "(nula)");

        // 1. Buscar venda aberta: por nota (Caixa Rápido/PDV) ou por comanda
        Venda? venda = null;
        if (!string.IsNullOrWhiteSpace(request.Nota))
        {
            venda = await _vendaRepository.GetVendaAbertaPorNotaAsync(request.Nota.Trim());
            if (venda == null)
            {
                _logger.LogError("❌ Nota {Nota} não possui venda aberta", request.Nota);
                throw new InvalidOperationException($"A nota {request.Nota} não possui uma venda em aberto.");
            }
        }
        else
        {
            venda = await _vendaRepository.GetVendaAbertaPorComandaAsync(request.Comanda);
            if (venda == null)
            {
                _logger.LogError("❌ Comanda {Comanda} não possui venda aberta", request.Comanda);
                throw new InvalidOperationException($"A comanda {request.Comanda} não possui uma venda em aberto.");
            }
        }

        _logger.LogInformation("✅ Venda encontrada - Nota: {Nota}, Total: {Total}", venda.Nota, venda.Total);

        // 2. Validar recebimentos
        if (request.Recebimentos == null || request.Recebimentos.Count == 0)
        {
            throw new ArgumentException("É necessário informar pelo menos uma forma de pagamento.");
        }

        // 3. Validar recebimentos e calcular totais
        var totalRecebido = request.Recebimentos.Sum(r => r.Valor);
        var totalTrocoRecebimentos = request.Recebimentos.Sum(r => r.Troco);
        var totalTrocoGeral = request.Troco;
        
        // O troco total é a soma dos trocos dos recebimentos OU o troco geral (não ambos)
        // Se o troco geral for maior que zero, usar ele; caso contrário, usar a soma dos trocos individuais
        var trocoTotal = totalTrocoGeral > 0 ? totalTrocoGeral : totalTrocoRecebimentos;
        
        // O valor efetivamente recebido (sem troco) é o total recebido menos o troco
        var totalComTroco = totalRecebido - trocoTotal;

        // Validar se o total cobre a venda
        if (totalComTroco < venda.Total)
        {
            _logger.LogWarning("⚠️ Total recebido ({TotalRecebido}) menor que o total da venda ({TotalVenda})",
                totalComTroco, venda.Total);
            throw new InvalidOperationException(
                $"O valor recebido (R$ {totalComTroco:N2}) é menor que o total da venda (R$ {venda.Total:N2}).");
        }

        // Validar se o troco total está correto
        var trocoTotalCalculado = totalRecebido - venda.Total;
        
        if (Math.Abs(trocoTotal - trocoTotalCalculado) > 0.01m)
        {
            _logger.LogWarning("⚠️ Troco total informado ({TrocoInformado}) diferente do calculado ({TrocoCalculado})",
                trocoTotal, trocoTotalCalculado);
            throw new InvalidOperationException(
                $"O troco total informado (R$ {trocoTotal:N2}) não confere com o calculado (R$ {trocoTotalCalculado:N2}). " +
                $"Valor recebido: R$ {totalRecebido:N2}, Total da venda: R$ {venda.Total:N2}.");
        }

        // 4. Validar formas de pagamento antes de processar
        var formasPagamento = new Dictionary<int, FormaPagamento>();
        foreach (var recebimentoItem in request.Recebimentos)
        {
            var formaPagamento = await _formaPagamentoRepository.GetFormaPagamentoByIdAsync(recebimentoItem.IdFormaPagamento);
            if (formaPagamento == null)
            {
                throw new InvalidOperationException($"Forma de pagamento com ID {recebimentoItem.IdFormaPagamento} não encontrada.");
            }

            if (!formaPagamento.IsAtivo)
            {
                throw new InvalidOperationException($"A forma de pagamento '{formaPagamento.Descricao}' está inativa.");
            }

            // Validar troco por forma de pagamento
            if (recebimentoItem.Troco > 0 && !formaPagamento.PermiteTrocoAtivo)
            {
                throw new InvalidOperationException(
                    $"A forma de pagamento '{formaPagamento.Descricao}' não permite troco, mas foi informado troco de R$ {recebimentoItem.Troco:N2}.");
            }

            // Validar se o valor é suficiente para o troco
            if (recebimentoItem.Troco > recebimentoItem.Valor)
            {
                throw new InvalidOperationException(
                    $"O troco informado (R$ {recebimentoItem.Troco:N2}) é maior que o valor recebido (R$ {recebimentoItem.Valor:N2}) para a forma '{formaPagamento.Descricao}'.");
            }

            formasPagamento[recebimentoItem.IdFormaPagamento] = formaPagamento;
        }

        // 5. Processar com transação para garantir atomicidade
        using var connection = _connectionFactory.CreateConnection() as FbConnection;
        if (connection == null)
        {
            throw new Exception("Falha ao criar conexão Firebird");
        }

        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var recebimentosCriados = new List<RecebimentoVendasDto>();
            bool criadoReceber = false;
            string? numeroReceber = null;

            foreach (var recebimentoItem in request.Recebimentos)
            {
                var formaPagamento = formasPagamento[recebimentoItem.IdFormaPagamento];

                _logger.LogInformation("💳 Processando recebimento - Forma: {Forma}, Valor: {Valor}, Troco: {Troco}",
                    formaPagamento.Descricao, recebimentoItem.Valor, recebimentoItem.Troco);

            // Criar recebimento
            var recebimento = new RecebimentoVendas
            {
                IdFormaPagamento = recebimentoItem.IdFormaPagamento,
                NCaixa = venda.Caixa > 0 ? venda.Caixa : 1, // Garantir que NCaixa não seja 0
                Nota = venda.Nota,
                Valor = recebimentoItem.Valor,
                Troco = recebimentoItem.Troco
            };

                _logger.LogInformation("📝 [RecebimentoService] Preparando recebimento - Nota: {Nota}, FormaPagamento: {FormaPagamento}, Valor: {Valor}, Troco: {Troco}, NCaixa: {NCaixa}",
                    recebimento.Nota, recebimento.IdFormaPagamento, recebimento.Valor, recebimento.Troco, recebimento.NCaixa);

                // Criar recebimento dentro da transação
                var recebimentoId = await _recebimentoRepository.CriarRecebimentoAsync(recebimento, transaction);
                
                _logger.LogInformation("✅ [RecebimentoService] Recebimento criado com ID: {RecebimentoId}", recebimentoId);

                recebimentosCriados.Add(new RecebimentoVendasDto
                {
                    Id = recebimentoId,
                    IdFormaPagamento = recebimentoItem.IdFormaPagamento,
                    FormaPagamentoDescricao = formaPagamento.Descricao,
                    NCaixa = venda.Caixa,
                    Nota = venda.Nota,
                    Valor = recebimentoItem.Valor,
                    Troco = recebimentoItem.Troco
                });

                // Se for CARTEIRA, criar registro em RECEBER com vencimento em 30 dias
                if (formaPagamento.IsCarteira)
                {
                    _logger.LogInformation("📝 Forma de pagamento é CARTEIRA - Criando registro em RECEBER para 30 dias");

                    // Buscar cliente da venda
                    var cliente = await _clienteRepository.GetByIdAsync(venda.Cliente);
                    if (cliente == null)
                    {
                        _logger.LogWarning("⚠️ Cliente {ClienteId} não encontrado, usando código 0", venda.Cliente);
                    }

                    var receber = new Receber
                    {
                        Codigo = venda.Cliente,
                        Tipo = 'C', // Cliente
                        Modelo = venda.Modelo,
                        Serie = venda.Serie,
                        Subserie = venda.Subserie,
                        Origem = venda.Origem,
                        Historico = $"VENDA {venda.Nota} - CARTEIRA",
                        Emissao = DateTime.Now,
                        Vencimento = DateTime.Now.AddDays(30), // 30 dias
                        Valor = recebimentoItem.Valor,
                        ValorRecebido = 0,
                        Operador = venda.Operador,
                        Especie = formaPagamento.Descricao,
                        ControleNota = venda.Nota,
                        Terminal = venda.Caixa,
                        NotaFiscal = venda.Nota,
                        IdVendedor = venda.Vendedor
                    };

                    await _receberRepository.CriarReceberAsync(receber, transaction);
                    criadoReceber = true;
                    numeroReceber = $"{receber.Numero}/{receber.Ordem}";

                    _logger.LogInformation("✅ Registro em RECEBER criado - Numero: {Numero}/{Ordem}, Vencimento: {Vencimento}",
                        receber.Numero, receber.Ordem, receber.Vencimento);
                }
            }

            // 6. Atualizar venda para EFETIVADO (fechar comanda) dentro da transação
            var statusAtualizado = await _vendaRepository.AtualizarStatusVendaAsync(venda.Nota, "EFETIVADO", transaction);
            if (!statusAtualizado)
            {
                throw new Exception($"Não foi possível atualizar o status da venda {venda.Nota} para EFETIVADO");
            }

            // 7. Copiar itens da tabela temporária (frente_tmpitvendas) para itevendas (baixa de produtos)
            //    e em seguida limpar a tabela temporária para que ao reabrir a comanda não puxe valores antigos
            var itensTemp = await _itemTemporarioRepository.GetItensPorCupomAsync(venda.Nota, venda.Operador);
            var itensTempList = itensTemp.ToList();
            if (itensTempList.Count > 0)
            {
                var itensVenda = itensTempList.Select((t, i) => new ItemVenda
                {
                    Nota = venda.Nota,
                    Modelo = venda.Modelo ?? "D2",
                    Serie = venda.Serie ?? "001",
                    Subserie = venda.Subserie ?? "01",
                    Origem = venda.Origem ?? "BA",
                    Emissao = venda.Emissao,
                    Item = t.Item,
                    Codigo = t.Codigo,
                    Barras = t.Barras ?? "",
                    Descricao = t.Descricao,
                    Cfop = 5102,
                    St = "0000",
                    Und = t.Und ?? "UN",
                    Qtd = t.Qtd,
                    Preco = t.Preco,
                    Desconto = t.Desconto,
                    Acrescimo = t.Acrescimo,
                    Total = t.Total,
                    Cancelado = "0",
                    Sequencia = t.Item,
                    PrecoCusto = 0,
                    Serial = !string.IsNullOrWhiteSpace(t.Observacao) ? t.Observacao : (t.Serial ?? ""),
                    Icms = 0,
                    Sinalm = -1 // -1 = baixa de estoque
                }).ToList();

                var itensInseridos = await _vendaRepository.CriarItensVendaAsync(itensVenda, transaction);
                if (!itensInseridos)
                {
                    throw new Exception($"Não foi possível gravar os itens da venda {venda.Nota} na tabela itevendas (baixa de produtos).");
                }
                _logger.LogInformation("✅ Itens copiados para itevendas (baixa de produtos): {Count} itens", itensVenda.Count);

                await _itemTemporarioRepository.LimparItensCupomAsync(venda.Nota, venda.Operador, transaction);
                _logger.LogInformation("✅ Itens temporários removidos da frente_tmpitvendas para nota {Nota}", venda.Nota);
            }

            // Commit da transação - tudo ou nada
            _logger.LogInformation("💾 [RecebimentoService] Fazendo commit da transação - Recebimentos criados: {Count}", recebimentosCriados.Count);
            transaction.Commit();
            _logger.LogInformation("✅ [RecebimentoService] Transação commitada com sucesso");

            _logger.LogInformation("✅ Comanda {Comanda} fechada com sucesso - Total: {Total}, Recebido: {Recebido}, Troco: {Troco}, Recebimentos: {Count}",
                request.Comanda, venda.Total, totalRecebido, trocoTotal, recebimentosCriados.Count);
            
            // Log detalhado dos recebimentos criados
            foreach (var rec in recebimentosCriados)
            {
                _logger.LogInformation("📋 [RecebimentoService] Recebimento criado - ID: {Id}, Nota: {Nota}, Forma: {Forma}, Valor: {Valor}",
                    rec.Id, rec.Nota, rec.FormaPagamentoDescricao, rec.Valor);
            }

            // Verificar se o cliente tem contas a receber em aberto
            ContasAbertoDto? contasAberto = null;
            if (venda.Cliente > 0)
            {
                try
                {
                    var (temContasAberto, valorTotalPendente, quantidadeContas) = 
                        await _receberRepository.VerificarContasAbertoAsync(venda.Cliente);
                    
                    if (temContasAberto)
                    {
                        contasAberto = new ContasAbertoDto
                        {
                            TemContasAberto = true,
                            ValorTotalPendente = valorTotalPendente,
                            QuantidadeContas = quantidadeContas,
                            Mensagem = $"Cliente possui {quantidadeContas} conta(s) a receber em aberto no valor total de R$ {valorTotalPendente:N2}"
                        };
                        _logger.LogInformation("⚠️ Cliente {Cliente} possui contas em aberto - Quantidade: {Quantidade}, Valor: {Valor}",
                            venda.Cliente, quantidadeContas, valorTotalPendente);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Erro ao verificar contas em aberto do cliente {Cliente}", venda.Cliente);
                    // Não falha o fechamento se houver erro na verificação
                }
            }

            var response = new FecharComandaResponseDto
            {
                Nota = venda.Nota,
                Total = venda.Total,
                TotalRecebido = totalRecebido,
                Troco = trocoTotal,
                Recebimentos = recebimentosCriados,
                CriadoReceber = criadoReceber,
                NumeroReceber = numeroReceber,
                ContasAberto = contasAberto
            };

            // Recibo do cliente: impressão é feita pelo frontend (pergunta "Deseja imprimir?" e exibe na tela para impressão)

            return response;
        }
        catch (Exception ex)
        {
            // Rollback em caso de erro
            _logger.LogError(ex, "❌ [RecebimentoService] Erro ao fechar comanda {Comanda} - Fazendo rollback da transação. Erro: {Erro}",
                request.Comanda, ex.Message);
            transaction.Rollback();
            _logger.LogError("❌ [RecebimentoService] Transação revertida (rollback) para comanda {Comanda}", request.Comanda);
            throw;
        }
    }

    public async Task<IEnumerable<FormaPagamentoDto>> GetFormasPagamentoAtivasAsync()
    {
        _logger.LogInformation("🔍 Buscando formas de pagamento ativas");

        var formas = await _formaPagamentoRepository.GetFormasPagamentoAtivasAsync();

        return formas.Select(f => new FormaPagamentoDto
        {
            Id = f.Id,
            Descricao = f.Descricao,
            Ativo = f.IsAtivo,
            Indice = f.Indice,
            Moeda = f.Moeda,
            MeioPagto = f.MeioPagto,
            PermiteTroco = f.PermiteTrocoAtivo,
            Tipo = f.Tipo
        });
    }

    public async Task<IEnumerable<RecebimentoVendasDto>> GetRecebimentosPorNotaAsync(string nota)
    {
        _logger.LogInformation("🔍 Buscando recebimentos por nota: {Nota}", nota);

        var recebimentos = await _recebimentoRepository.GetRecebimentosPorNotaAsync(nota);
        var recebimentosDto = new List<RecebimentoVendasDto>();

        foreach (var recebimento in recebimentos)
        {
            var formaPagamento = await _formaPagamentoRepository.GetFormaPagamentoByIdAsync(recebimento.IdFormaPagamento);

            recebimentosDto.Add(new RecebimentoVendasDto
            {
                Id = recebimento.Id,
                IdFormaPagamento = recebimento.IdFormaPagamento,
                FormaPagamentoDescricao = formaPagamento?.Descricao ?? "N/A",
                NCaixa = recebimento.NCaixa,
                Nota = recebimento.Nota,
                Valor = recebimento.Valor,
                Troco = recebimento.Troco
            });
        }

        return recebimentosDto;
    }

    public async Task<IEnumerable<RecebimentoVendasDto>> GetRecebimentosPorPeriodoAsync(DateTime? dataInicio, DateTime? dataFim)
    {
        _logger.LogInformation("🔍 Buscando recebimentos por período - Data Inicio: {DataInicio}, Data Fim: {DataFim}",
            dataInicio, dataFim);

        var recebimentos = await _recebimentoRepository.GetRecebimentosPorPeriodoAsync(dataInicio, dataFim);
        var recebimentosList = recebimentos.ToList();

        var recebimentosDto = new List<RecebimentoVendasDto>();

        foreach (var recebimento in recebimentosList)
        {
            var formaPagamento = await _formaPagamentoRepository.GetFormaPagamentoByIdAsync(recebimento.IdFormaPagamento);

            recebimentosDto.Add(new RecebimentoVendasDto
            {
                Id = recebimento.Id,
                IdFormaPagamento = recebimento.IdFormaPagamento,
                FormaPagamentoDescricao = formaPagamento?.Descricao ?? "N/A",
                NCaixa = recebimento.NCaixa,
                Nota = recebimento.Nota,
                Valor = recebimento.Valor,
                Troco = recebimento.Troco
            });
        }

        _logger.LogInformation("✅ Encontrados {Count} recebimentos no período", recebimentosDto.Count);

        return recebimentosDto;
    }
}

