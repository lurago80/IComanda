using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class RelatorioService : IRelatorioService
{
    private readonly IRelatorioRepository _relatorioRepository;
    private readonly ILogger<RelatorioService> _logger;

    public RelatorioService(
        IRelatorioRepository relatorioRepository,
        ILogger<RelatorioService> logger)
    {
        _relatorioRepository = relatorioRepository;
        _logger = logger;
    }

    public async Task<RelatorioClienteDto> GetRelatorioClienteAsync(int codigoCliente, DateTime? dataInicio = null, DateTime? dataFim = null, string? origem = null)
    {
        _logger.LogInformation("📊 Gerando relatório de cliente {CodigoCliente} - Período: {DataInicio} a {DataFim}, Origem: {Origem}", 
            codigoCliente, dataInicio, dataFim, origem ?? "todos");

        return await _relatorioRepository.GetRelatorioClienteAsync(codigoCliente, dataInicio, dataFim, origem);
    }

    public async Task<RelatorioVendasDto> GetRelatorioVendasAsync(DateTime data, string? origem = null)
    {
        _logger.LogInformation("📊 Gerando relatório de vendas - Data: {Data}, Origem: {Origem}", data, origem ?? "todos");

        return await _relatorioRepository.GetRelatorioVendasAsync(data, origem);
    }

    public async Task<RelatorioVendasDto> GetRelatorioVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null)
    {
        _logger.LogInformation("📊 Gerando relatório de vendas - Período: {DataInicio} a {DataFim}, Origem: {Origem}", dataInicio, dataFim, origem ?? "todos");

        return await _relatorioRepository.GetRelatorioVendasPorPeriodoAsync(dataInicio, dataFim, origem);
    }

    public async Task<RelatorioProdutosMaisVendidosDto> GetProdutosMaisVendidosAsync(DateTime dataInicio, DateTime dataFim, int top = 10, string? origem = null)
    {
        _logger.LogInformation("📊 Gerando relatório de produtos mais vendidos - Período: {DataInicio} a {DataFim}, Top: {Top}, Origem: {Origem}", 
            dataInicio, dataFim, top, origem ?? "todos");

        return await _relatorioRepository.GetProdutosMaisVendidosAsync(dataInicio, dataFim, top, origem);
    }

    public async Task<RelatorioPeriodoDto> GetRelatorioPeriodoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null)
    {
        _logger.LogInformation("📊 Gerando relatório por período - Data Inicio: {DataInicio}, Data Fim: {DataFim}, Origem: {Origem}", 
            dataInicio, dataFim, origem ?? "todos");

        if (dataInicio > dataFim)
        {
            throw new ArgumentException("Data inicial não pode ser maior que data final");
        }

        return await _relatorioRepository.GetRelatorioPeriodoAsync(dataInicio, dataFim, origem);
    }

    public async Task<RelatorioCaixaConsolidadoDto> GetRelatorioCaixaConsolidadoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null)
    {
        _logger.LogInformation("📊 Gerando relatório caixa consolidado - Período: {DataInicio} a {DataFim}, Origem: {Origem}", dataInicio, dataFim, origem ?? "todos");
        if (dataInicio > dataFim)
            throw new ArgumentException("Data inicial não pode ser maior que data final");
        return await _relatorioRepository.GetRelatorioCaixaConsolidadoAsync(dataInicio, dataFim, origem);
    }

    public async Task<RelatorioDashboardDto> GetDashboardAsync(DateTime dataInicio, DateTime dataFim, string? origem = null)
    {
        _logger.LogInformation("📊 Gerando dashboard - Período: {DataInicio} a {DataFim}", dataInicio, dataFim);
        if (dataInicio > dataFim)
            throw new ArgumentException("Data inicial não pode ser maior que data final");
        return await _relatorioRepository.GetDashboardAsync(dataInicio, dataFim, origem);
    }
}

