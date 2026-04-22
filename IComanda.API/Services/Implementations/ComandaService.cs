using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class ComandaService : IComandaService
{
    private readonly IVendaRepository _vendaRepository;
    private readonly ILogger<ComandaService> _logger;

    public ComandaService(
        IVendaRepository vendaRepository,
        ILogger<ComandaService> logger)
    {
        _vendaRepository = vendaRepository;
        _logger = logger;
    }

    /// <summary>
    /// Lista todas as comandas abertas com informações resumidas
    /// </summary>
    public async Task<IEnumerable<ComandaAbertaDto>> GetComandasAbertasAsync()
    {
        _logger.LogInformation("🔍 Buscando comandas abertas");

        var vendasAbertas = await _vendaRepository.GetVendasAbertasAsync();
        var comandas = vendasAbertas
            .Where(v => v.Comanda.HasValue && v.Comanda.Value > 0)
            .GroupBy(v => v.Comanda!.Value)
            .Select(g => new ComandaAbertaDto
            {
                Numero = g.Key,
                Nota = g.First().Nota,
                Cliente = g.First().Cliente,
                Total = g.Sum(v => v.Total),
                DataAbertura = g.Min(v => v.Emissao),
                Operador = g.First().Operador,
                Mesa = g.First().Mesa,
                QuantidadeItens = 0 // Seria necessário buscar itens para calcular
            })
            .OrderBy(c => c.Numero)
            .ToList();

        _logger.LogInformation("✅ Encontradas {Count} comandas abertas", comandas.Count);

        return comandas;
    }

    /// <summary>
    /// Verifica se uma comanda está aberta
    /// </summary>
    public async Task<bool> VerificarComandaAbertaAsync(int comanda)
    {
        var venda = await _vendaRepository.GetVendaAbertaPorComandaAsync(comanda);
        return venda != null;
    }

    /// <summary>
    /// Obtém informações resumidas de uma comanda
    /// </summary>
    public async Task<ComandaAbertaDto?> GetComandaAsync(int comanda)
    {
        var venda = await _vendaRepository.GetVendaAbertaPorComandaAsync(comanda);
        if (venda == null)
        {
            return null;
        }

        return new ComandaAbertaDto
        {
            Numero = comanda,
            Nota = venda.Nota,
            Cliente = venda.Cliente,
            Total = venda.Total,
            DataAbertura = venda.Emissao,
            Operador = venda.Operador,
            Mesa = venda.Mesa,
            QuantidadeItens = 0
        };
    }
}
