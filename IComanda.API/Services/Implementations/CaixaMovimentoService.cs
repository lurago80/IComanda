using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class CaixaMovimentoService : ICaixaMovimentoService
{
    private readonly ICaixaMovimentoRepository _caixaMovimentoRepository;
    private readonly ILogger<CaixaMovimentoService> _logger;

    public CaixaMovimentoService(
        ICaixaMovimentoRepository caixaMovimentoRepository,
        ILogger<CaixaMovimentoService> logger)
    {
        _caixaMovimentoRepository = caixaMovimentoRepository;
        _logger = logger;
    }

    public async Task<CaixaMovimentoDto> AbrirCaixaAsync(CaixaMovimentoRequest request)
    {
        _logger.LogInformation("💰 Abrindo caixa - Terminal: {Terminal}, Valor: {Valor}, Operador: {Operador}",
            request.Terminal, request.Valor, request.Operador);

        if (request.Valor < 0)
        {
            throw new ArgumentException("Valor de abertura não pode ser negativo");
        }

        // Verificar se já existe abertura hoje
        var movimentosHoje = await _caixaMovimentoRepository.GetMovimentosPorOrigemAsync(
            request.Terminal, "ABERTURA", DateTime.Today, DateTime.Today);

        if (movimentosHoje.Any())
        {
            throw new InvalidOperationException($"Caixa {request.Terminal} já foi aberto hoje");
        }

        var movimento = new CaixaMovimento
        {
            Data = DateTime.Now,
            Hora = DateTime.Now.TimeOfDay,
            Documento = request.Documento ?? $"ABERTURA-{request.Terminal}-{DateTime.Now:yyyyMMdd}",
            Custo = request.Custo,
            Conta = request.Conta,
            Entrada = request.Valor,
            Saida = 0,
            Origem = "ABERTURA",
            Operador = request.Operador,
            Historico = request.Historico ?? $"Abertura de caixa - Terminal {request.Terminal}",
            Gravacao = DateTime.Now,
            CodProf = request.CodProf,
            Terminal = request.Terminal,
            Vendedor = request.Vendedor ?? 0
        };

        var codigo = await _caixaMovimentoRepository.CriarMovimentoAsync(movimento);

        _logger.LogInformation("✅ Caixa {Terminal} aberto com sucesso - Codigo: {Codigo}, Saldo: {Saldo}",
            request.Terminal, codigo, movimento.Saldo);

        return MapToDto(movimento);
    }

    public async Task<CaixaMovimentoDto> RegistrarSuprimentoAsync(CaixaMovimentoRequest request)
    {
        _logger.LogInformation("💰 Registrando suprimento - Terminal: {Terminal}, Valor: {Valor}, Operador: {Operador}",
            request.Terminal, request.Valor, request.Operador);

        if (request.Valor <= 0)
        {
            throw new ArgumentException("Valor do suprimento deve ser maior que zero");
        }

        var movimento = new CaixaMovimento
        {
            Data = DateTime.Now,
            Hora = DateTime.Now.TimeOfDay,
            Documento = request.Documento ?? $"SUPRIMENTO-{request.Terminal}-{DateTime.Now:yyyyMMddHHmmss}",
            Custo = request.Custo,
            Conta = request.Conta,
            Entrada = request.Valor,
            Saida = 0,
            Origem = "SUPRIMENTO",
            Operador = request.Operador,
            Historico = request.Historico ?? $"Suprimento de caixa - Terminal {request.Terminal}",
            Gravacao = DateTime.Now,
            CodProf = request.CodProf,
            Terminal = request.Terminal,
            Vendedor = request.Vendedor ?? 0
        };

        var codigo = await _caixaMovimentoRepository.CriarMovimentoAsync(movimento);

        _logger.LogInformation("✅ Suprimento registrado com sucesso - Codigo: {Codigo}, Valor: {Valor}, Saldo: {Saldo}",
            codigo, request.Valor, movimento.Saldo);

        return MapToDto(movimento);
    }

    public async Task<CaixaMovimentoDto> RegistrarSangriaAsync(CaixaMovimentoRequest request)
    {
        _logger.LogInformation("💰 Registrando sangria - Terminal: {Terminal}, Valor: {Valor}, Operador: {Operador}",
            request.Terminal, request.Valor, request.Operador);

        if (request.Valor <= 0)
        {
            throw new ArgumentException("Valor da sangria deve ser maior que zero");
        }

        // Verificar saldo disponível
        var saldoAtual = await _caixaMovimentoRepository.GetSaldoAtualAsync(request.Terminal);
        if (saldoAtual < request.Valor)
        {
            throw new InvalidOperationException(
                $"Saldo insuficiente. Saldo atual: R$ {saldoAtual:N2}, Valor solicitado: R$ {request.Valor:N2}");
        }

        var movimento = new CaixaMovimento
        {
            Data = DateTime.Now,
            Hora = DateTime.Now.TimeOfDay,
            Documento = request.Documento ?? $"SANGRIA-{request.Terminal}-{DateTime.Now:yyyyMMddHHmmss}",
            Custo = request.Custo,
            Conta = request.Conta,
            Entrada = 0,
            Saida = request.Valor,
            Origem = "SANGRIA",
            Operador = request.Operador,
            Historico = request.Historico ?? $"Sangria de caixa - Terminal {request.Terminal}",
            Gravacao = DateTime.Now,
            CodProf = request.CodProf,
            Terminal = request.Terminal,
            Vendedor = request.Vendedor ?? 0
        };

        var codigo = await _caixaMovimentoRepository.CriarMovimentoAsync(movimento);

        _logger.LogInformation("✅ Sangria registrada com sucesso - Codigo: {Codigo}, Valor: {Valor}, Saldo: {Saldo}",
            codigo, request.Valor, movimento.Saldo);

        return MapToDto(movimento);
    }

    public async Task<CaixaMovimentoDto> RegistrarPagamentoDespesaAsync(CaixaMovimentoRequest request)
    {
        _logger.LogInformation("💰 Registrando pagamento de despesa - Terminal: {Terminal}, Valor: {Valor}, Operador: {Operador}",
            request.Terminal, request.Valor, request.Operador);

        if (request.Valor <= 0)
        {
            throw new ArgumentException("Valor da despesa deve ser maior que zero");
        }

        // Verificar saldo disponível
        var saldoAtual = await _caixaMovimentoRepository.GetSaldoAtualAsync(request.Terminal);
        if (saldoAtual < request.Valor)
        {
            throw new InvalidOperationException(
                $"Saldo insuficiente. Saldo atual: R$ {saldoAtual:N2}, Valor solicitado: R$ {request.Valor:N2}");
        }

        var movimento = new CaixaMovimento
        {
            Data = DateTime.Now,
            Hora = DateTime.Now.TimeOfDay,
            Documento = request.Documento ?? $"DESPESA-{request.Terminal}-{DateTime.Now:yyyyMMddHHmmss}",
            Custo = request.Custo,
            Conta = request.Conta,
            Entrada = 0,
            Saida = request.Valor,
            Origem = "DESPESA",
            Operador = request.Operador,
            Historico = request.Historico ?? $"Pagamento de despesa - Terminal {request.Terminal}",
            Gravacao = DateTime.Now,
            CodProf = request.CodProf,
            Terminal = request.Terminal,
            Vendedor = request.Vendedor ?? 0
        };

        var codigo = await _caixaMovimentoRepository.CriarMovimentoAsync(movimento);

        _logger.LogInformation("✅ Pagamento de despesa registrado com sucesso - Codigo: {Codigo}, Valor: {Valor}, Saldo: {Saldo}",
            codigo, request.Valor, movimento.Saldo);

        return MapToDto(movimento);
    }

    public async Task<CaixaResumoDto> GetResumoAsync(int terminal, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        _logger.LogInformation("🔍 Buscando resumo de caixa - Terminal: {Terminal}, Data Inicio: {DataInicio}, Data Fim: {DataFim}",
            terminal, dataInicio, dataFim);

        var movimentos = await _caixaMovimentoRepository.GetMovimentosAsync(terminal, dataInicio, dataFim);
        var movimentosList = movimentos.ToList();

        var saldoAtual = await _caixaMovimentoRepository.GetSaldoAtualAsync(terminal);
        var totalEntradas = movimentosList.Sum(m => m.Entrada);
        var totalSaidas = movimentosList.Sum(m => m.Saida);

        return new CaixaResumoDto
        {
            Terminal = terminal,
            SaldoAtual = saldoAtual,
            TotalEntradas = totalEntradas,
            TotalSaidas = totalSaidas,
            QuantidadeMovimentos = movimentosList.Count,
            Movimentos = movimentosList.Select(MapToDto).ToList()
        };
    }

    public async Task<IEnumerable<CaixaMovimentoDto>> GetMovimentosAsync(int terminal, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        _logger.LogInformation("🔍 Buscando movimentos de caixa - Terminal: {Terminal}, Data Inicio: {DataInicio}, Data Fim: {DataFim}",
            terminal, dataInicio, dataFim);

        var movimentos = await _caixaMovimentoRepository.GetMovimentosAsync(terminal, dataInicio, dataFim);
        return movimentos.Select(MapToDto);
    }

    private CaixaMovimentoDto MapToDto(CaixaMovimento movimento)
    {
        return new CaixaMovimentoDto
        {
            Codigo = movimento.Codigo,
            Data = movimento.Data,
            Hora = movimento.Hora,
            Documento = movimento.Documento,
            Custo = movimento.Custo,
            Conta = movimento.Conta,
            Entrada = movimento.Entrada,
            Saida = movimento.Saida,
            Saldo = movimento.Saldo,
            Origem = movimento.Origem,
            Operador = movimento.Operador,
            Historico = movimento.Historico,
            Gravacao = movimento.Gravacao,
            CodProf = movimento.CodProf,
            Terminal = movimento.Terminal,
            Vendedor = movimento.Vendedor
        };
    }
}
