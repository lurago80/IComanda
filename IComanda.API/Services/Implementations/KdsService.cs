using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Services.Implementations;

public class KdsService : IKdsService
{
    private readonly IKdsRepository _kdsRepository;
    private readonly ILogger<KdsService> _logger;

    public KdsService(IKdsRepository kdsRepository, ILogger<KdsService> logger)
    {
        _kdsRepository = kdsRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<KdsPedidoDto>> GetPedidosAtivosAsync()
    {
        return await _kdsRepository.GetPedidosAtivosAsync();
    }

    public async Task<bool> AtualizarStatusAsync(string nota, string statusCozinha)
    {
        var statusValidos = new[] { "PENDENTE", "EM_PREPARO", "PRONTO", "ENTREGUE" };
        var statusUpper = statusCozinha.ToUpperInvariant();

        if (!statusValidos.Contains(statusUpper))
        {
            _logger.LogWarning("KDS: status inválido recebido: {Status}", statusCozinha);
            return false;
        }

        return await _kdsRepository.AtualizarStatusCozinhaAsync(nota, statusUpper);
    }
}
