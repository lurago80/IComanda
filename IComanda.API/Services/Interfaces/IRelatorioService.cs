using IComanda.API.Models.DTOs;

namespace IComanda.API.Services.Interfaces;

public interface IRelatorioService
{
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    Task<RelatorioClienteDto> GetRelatorioClienteAsync(int codigoCliente, DateTime? dataInicio = null, DateTime? dataFim = null, string? origem = null);
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    Task<RelatorioVendasDto> GetRelatorioVendasAsync(DateTime data, string? origem = null);
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    Task<RelatorioVendasDto> GetRelatorioVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null);
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    Task<RelatorioProdutosMaisVendidosDto> GetProdutosMaisVendidosAsync(DateTime dataInicio, DateTime dataFim, int top = 10, string? origem = null);
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    Task<RelatorioPeriodoDto> GetRelatorioPeriodoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null);
    /// <param name="origem">BA = só comandas, DL = só delivery, null = todos</param>
    Task<RelatorioCaixaConsolidadoDto> GetRelatorioCaixaConsolidadoAsync(DateTime dataInicio, DateTime dataFim, string? origem = null);
    Task<RelatorioDashboardDto> GetDashboardAsync(DateTime dataInicio, DateTime dataFim, string? origem = null);
    Task<RelatorioConsignacaoDto> GetRelatorioConsignacaoAsync(int grupoId, DateTime dataInicio, DateTime dataFim);
}

