using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using IComanda.API.Data;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace IComanda.API.Services.Implementations;

public class ReceberService : IReceberService
{
    private readonly IReceberRepository _receberRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IRecebimentoRepository _recebimentoRepository;
    private readonly IFormaPagamentoRepository _formaPagamentoRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ReceberService> _logger;

    public ReceberService(
        IReceberRepository receberRepository,
        IClienteRepository clienteRepository,
        IRecebimentoRepository recebimentoRepository,
        IFormaPagamentoRepository formaPagamentoRepository,
        IDbConnectionFactory connectionFactory,
        ILogger<ReceberService> logger)
    {
        _receberRepository = receberRepository;
        _clienteRepository = clienteRepository;
        _recebimentoRepository = recebimentoRepository;
        _formaPagamentoRepository = formaPagamentoRepository;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<ReceberDto?> GetReceberPorNumeroOrdemAsync(string numero, string ordem)
    {
        _logger.LogInformation("🔍 Buscando conta a receber - Numero: {Numero}, Ordem: {Ordem}", numero, ordem);

        var receber = await _receberRepository.GetReceberPorNumeroOrdemAsync(numero, ordem);
        if (receber == null)
        {
            return null;
        }

        // Buscar nome e telefone do cliente
        string? nomeCliente = null;
        string? telefoneCliente = null;
        if (receber.Codigo > 0)
        {
            var cliente = await _clienteRepository.GetByIdAsync(receber.Codigo);
            if (cliente != null)
            {
                nomeCliente = cliente.Nome;
                telefoneCliente = !string.IsNullOrEmpty(cliente.Celular) 
                    ? cliente.Celular 
                    : cliente.Telefone;
            }
        }

        return MapToDto(receber, nomeCliente, telefoneCliente);
    }

    public async Task<IEnumerable<ReceberDto>> GetReceberPendentesAsync(int? codigoCliente = null, DateTime? dataVencimentoInicio = null, DateTime? dataVencimentoFim = null)
    {
        _logger.LogInformation("🔍 Buscando contas a receber pendentes - Cliente: {Cliente}, Data Inicio: {DataInicio}, Data Fim: {DataFim}",
            codigoCliente, dataVencimentoInicio, dataVencimentoFim);

        var receber = await _receberRepository.GetReceberPendentesAsync(codigoCliente, dataVencimentoInicio, dataVencimentoFim);
        var receberList = receber.ToList();

        // Buscar nomes e telefones dos clientes
        var codigosClientes = receberList.Select(r => r.Codigo).Distinct().ToList();
        var clientes = new Dictionary<int, (string? Nome, string? Telefone)>();

        foreach (var codigo in codigosClientes)
        {
            if (codigo > 0)
            {
                var cliente = await _clienteRepository.GetByIdAsync(codigo);
                if (cliente != null)
                {
                    var telefone = !string.IsNullOrEmpty(cliente.Celular) 
                        ? cliente.Celular 
                        : cliente.Telefone;
                    clientes[codigo] = (cliente.Nome, telefone);
                }
            }
        }

        return receberList.Select(r => 
        {
            var dadosCliente = clientes.GetValueOrDefault(r.Codigo);
            return MapToDto(r, dadosCliente.Nome, dadosCliente.Telefone);
        });
    }

    public async Task<IEnumerable<ReceberDto>> GetReceberPorClienteAsync(int codigoCliente, bool apenasPendentes = true)
    {
        _logger.LogInformation("🔍 Buscando contas a receber do cliente {CodigoCliente} - Apenas pendentes: {ApenasPendentes}",
            codigoCliente, apenasPendentes);

        var receber = await _receberRepository.GetReceberPorClienteAsync(codigoCliente, apenasPendentes);

        // Buscar nome e telefone do cliente
        string? nomeCliente = null;
        string? telefoneCliente = null;
        var cliente = await _clienteRepository.GetByIdAsync(codigoCliente);
        if (cliente != null)
        {
            nomeCliente = cliente.Nome;
            telefoneCliente = !string.IsNullOrEmpty(cliente.Celular) 
                ? cliente.Celular 
                : cliente.Telefone;
        }

        return receber.Select(r => MapToDto(r, nomeCliente, telefoneCliente));
    }

    public async Task<QuitarReceberResponseDto> QuitarReceberAsync(QuitarReceberRequest request)
    {
        _logger.LogInformation("💰 Quitando conta a receber - Numero: {Numero}, Ordem: {Ordem}, Valor: {Valor}",
            request.Numero, request.Ordem, request.ValorRecebido);

        // Validar request
        if (string.IsNullOrWhiteSpace(request.Numero))
        {
            throw new ArgumentException("Número da conta não pode ser vazio");
        }

        if (string.IsNullOrWhiteSpace(request.Ordem))
        {
            throw new ArgumentException("Ordem da conta não pode ser vazia");
        }

        if (request.ValorRecebido <= 0)
        {
            throw new ArgumentException("Valor a receber deve ser maior que zero");
        }

        // Buscar conta
        var conta = await _receberRepository.GetReceberPorNumeroOrdemAsync(request.Numero, request.Ordem);
        if (conta == null)
        {
            throw new InvalidOperationException($"Conta a receber {request.Numero}/{request.Ordem} não encontrada");
        }

        // Validar se já está quitada
        var valorPendente = conta.Valor - conta.ValorRecebido;
        if (valorPendente <= 0)
        {
            throw new InvalidOperationException($"Conta {request.Numero}/{request.Ordem} já está totalmente quitada");
        }

        // Validar valor
        if (request.ValorRecebido > valorPendente)
        {
            throw new InvalidOperationException(
                $"Valor a receber (R$ {request.ValorRecebido:N2}) é maior que o valor pendente (R$ {valorPendente:N2})");
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
            var dataRecebimento = request.DataRecebimento ?? DateTime.Now;
            var operador = request.Operador ?? 1;

            // Quitar a conta
            var quitado = await _receberRepository.QuitarReceberAsync(
                request.Numero,
                request.Ordem,
                request.ValorRecebido,
                dataRecebimento,
                operador,
                transaction);

            if (!quitado)
            {
                throw new Exception("Erro ao quitar conta a receber");
            }

            // Criar registros em RECEBIMENTO_VENDAS para cada forma de pagamento
            if (request.FormasPagamento != null && request.FormasPagamento.Any())
            {
                // Múltiplas formas de pagamento
                foreach (var formaRecebimento in request.FormasPagamento)
                {
                    if (formaRecebimento.Valor <= 0) continue;

                    var formaPagamento = await _formaPagamentoRepository.GetFormaPagamentoByIdAsync(formaRecebimento.IdFormaPagamento);
                    if (formaPagamento != null)
                    {
                        var recebimento = new RecebimentoVendas
                        {
                            IdFormaPagamento = formaRecebimento.IdFormaPagamento,
                            NCaixa = conta.Terminal > 0 ? conta.Terminal : 1,
                            Nota = conta.ControleNota ?? conta.NotaFiscal ?? "0",
                            Valor = formaRecebimento.Valor,
                            Troco = 0
                        };

                        await _recebimentoRepository.CriarRecebimentoAsync(recebimento, transaction);
                        _logger.LogInformation("✅ Registro de recebimento criado - Forma: {Forma}, Valor: {Valor}",
                            formaPagamento.Descricao, formaRecebimento.Valor);
                    }
                }
            }
            else if (request.IdFormaPagamento.HasValue)
            {
                // Forma única (compatibilidade com versão anterior)
                var formaPagamento = await _formaPagamentoRepository.GetFormaPagamentoByIdAsync(request.IdFormaPagamento.Value);
                if (formaPagamento != null)
                {
                    var recebimento = new RecebimentoVendas
                    {
                        IdFormaPagamento = request.IdFormaPagamento.Value,
                        NCaixa = conta.Terminal > 0 ? conta.Terminal : 1,
                        Nota = conta.ControleNota ?? conta.NotaFiscal ?? "0",
                        Valor = request.ValorRecebido,
                        Troco = 0
                    };

                    await _recebimentoRepository.CriarRecebimentoAsync(recebimento, transaction);
                    _logger.LogInformation("✅ Registro de recebimento criado - Forma: {Forma}, Valor: {Valor}",
                        formaPagamento.Descricao, request.ValorRecebido);
                }
            }

            // Commit da transação
            transaction.Commit();

            // Buscar conta atualizada
            var contaAtualizada = await _receberRepository.GetReceberPorNumeroOrdemAsync(request.Numero, request.Ordem);
            if (contaAtualizada == null)
            {
                throw new Exception("Erro ao buscar conta atualizada após quitamento");
            }

            var valorTotalRecebido = contaAtualizada.ValorRecebido;
            var valorPendenteFinal = contaAtualizada.Valor - valorTotalRecebido;
            var totalmenteQuitado = valorPendenteFinal <= 0;

            _logger.LogInformation("✅ Conta {Numero}/{Ordem} quitada com sucesso - Valor Recebido: {ValorRecebido}, Total Recebido: {TotalRecebido}, Quitado: {Quitado}",
                request.Numero, request.Ordem, request.ValorRecebido, valorTotalRecebido, totalmenteQuitado);

            // Verificar se o cliente ainda tem outras contas a receber em aberto
            ContasAbertoDto? contasAberto = null;
            if (conta.Codigo > 0)
            {
                try
                {
                    var (temContasAberto, valorTotalPendente, quantidadeContas) = 
                        await _receberRepository.VerificarContasAbertoAsync(conta.Codigo);
                    
                    if (temContasAberto)
                    {
                        contasAberto = new ContasAbertoDto
                        {
                            TemContasAberto = true,
                            ValorTotalPendente = valorTotalPendente,
                            QuantidadeContas = quantidadeContas,
                            Mensagem = $"Cliente ainda possui {quantidadeContas} conta(s) a receber em aberto no valor total de R$ {valorTotalPendente:N2}"
                        };
                        _logger.LogInformation("⚠️ Cliente {Cliente} ainda possui contas em aberto - Quantidade: {Quantidade}, Valor: {Valor}",
                            conta.Codigo, quantidadeContas, valorTotalPendente);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Erro ao verificar contas em aberto do cliente {Cliente}", conta.Codigo);
                    // Não falha o quitamento se houver erro na verificação
                }
            }

            return new QuitarReceberResponseDto
            {
                Numero = request.Numero,
                Ordem = request.Ordem,
                ValorOriginal = conta.Valor,
                ValorRecebido = request.ValorRecebido,
                ValorTotalRecebido = valorTotalRecebido,
                ValorPendente = valorPendenteFinal,
                TotalmenteQuitado = totalmenteQuitado,
                DataRecebimento = dataRecebimento,
                ContasAberto = contasAberto
            };
        }
        catch
        {
            // Rollback em caso de erro
            transaction.Rollback();
            _logger.LogError("❌ Erro ao quitar conta {Numero}/{Ordem} - Transação revertida", request.Numero, request.Ordem);
            throw;
        }
    }

    private ReceberDto MapToDto(Receber receber, string? nomeCliente = null, string? telefoneCliente = null)
    {
        return new ReceberDto
        {
            Numero = receber.Numero,
            Ordem = receber.Ordem,
            Codigo = receber.Codigo,
            NomeCliente = nomeCliente,
            TelefoneCliente = telefoneCliente,
            Tipo = receber.Tipo,
            Modelo = receber.Modelo,
            Serie = receber.Serie,
            Historico = receber.Historico,
            Emissao = receber.Emissao,
            Vencimento = receber.Vencimento,
            Valor = receber.Valor,
            Recebimento = receber.Recebimento,
            ValorRecebido = receber.ValorRecebido,
            Acrescimo = receber.Acrescimo,
            Desconto = receber.Desconto,
            Juros = receber.Juros,
            Operador = receber.Operador,
            Especie = receber.Especie,
            ControleNota = receber.ControleNota,
            NotaFiscal = receber.NotaFiscal
        };
    }

    public async Task<ContasAbertoDto> VerificarContasAbertoAsync(int codigoCliente)
    {
        _logger.LogInformation("🔍 Verificando contas em aberto do cliente {CodigoCliente}", codigoCliente);

        var (temContasAberto, valorTotalPendente, quantidadeContas) = 
            await _receberRepository.VerificarContasAbertoAsync(codigoCliente);

        var mensagem = temContasAberto
            ? $"Cliente possui {quantidadeContas} conta(s) a receber em aberto no valor total de R$ {valorTotalPendente:N2}"
            : string.Empty;

        _logger.LogInformation("✅ Verificação concluída - Cliente {CodigoCliente}: {TemContas}, Valor: {Valor}, Quantidade: {Quantidade}",
            codigoCliente, temContasAberto, valorTotalPendente, quantidadeContas);

        return new ContasAbertoDto
        {
            TemContasAberto = temContasAberto,
            ValorTotalPendente = valorTotalPendente,
            QuantidadeContas = quantidadeContas,
            Mensagem = mensagem
        };
    }
}

