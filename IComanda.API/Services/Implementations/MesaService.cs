using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class MesaService : IMesaService
{
    private readonly IMesaRepository _mesaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly ILogger<MesaService> _logger;

    public MesaService(
        IMesaRepository mesaRepository,
        IClienteRepository clienteRepository,
        ILogger<MesaService> logger)
    {
        _mesaRepository = mesaRepository;
        _clienteRepository = clienteRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MesaDto>> GetMesasAsync()
    {
        _logger.LogInformation("🔍 Buscando todas as mesas");
        var mesas = await _mesaRepository.GetMesasAsync();
        return await MapToDtoListAsync(mesas);
    }

    public async Task<MesaDto?> GetMesaPorNumeroAsync(int numero)
    {
        _logger.LogInformation("🔍 Buscando mesa {Numero}", numero);
        var mesa = await _mesaRepository.GetMesaPorNumeroAsync(numero);
        if (mesa == null)
        {
            return null;
        }
        return await MapToDtoAsync(mesa);
    }

    public async Task<MesaDto> OcuparMesaAsync(int numero, int comanda, string nota, int operador, int? cliente = null, int? numeroPessoas = null)
    {
        _logger.LogInformation("🪑 Ocupando mesa {Numero} - Comanda: {Comanda}, Nota: {Nota}", numero, comanda, nota);
        
        await _mesaRepository.AtualizarStatusMesaAsync(numero, "OCUPADA", comanda, nota, operador, cliente, numeroPessoas);
        
        var mesa = await _mesaRepository.GetMesaPorNumeroAsync(numero);
        if (mesa == null)
        {
            throw new Exception($"Mesa {numero} não encontrada");
        }

        return await MapToDtoAsync(mesa);
    }

    public async Task<MesaDto> LiberarMesaAsync(int numero)
    {
        _logger.LogInformation("🪑 Liberando mesa {Numero}", numero);
        
        await _mesaRepository.AtualizarStatusMesaAsync(numero, "LIVRE");
        
        var mesa = await _mesaRepository.GetMesaPorNumeroAsync(numero);
        if (mesa == null)
        {
            throw new Exception($"Mesa {numero} não encontrada");
        }

        return await MapToDtoAsync(mesa);
    }

    public async Task<IEnumerable<MesaDto>> GetMesasOcupadasAsync()
    {
        _logger.LogInformation("🔍 Buscando mesas ocupadas");
        var mesas = await _mesaRepository.GetMesasOcupadasAsync();
        return await MapToDtoListAsync(mesas);
    }

    public async Task<IEnumerable<MesaDto>> GetMesasLivresAsync()
    {
        _logger.LogInformation("🔍 Buscando mesas livres");
        var mesas = await _mesaRepository.GetMesasLivresAsync();
        return await MapToDtoListAsync(mesas);
    }

    private async Task<MesaDto> MapToDtoAsync(Models.Entities.Mesa mesa)
    {
        string? nomeCliente = null;
        if (mesa.Cliente.HasValue && mesa.Cliente.Value > 0)
        {
            var cliente = await _clienteRepository.GetByIdAsync(mesa.Cliente.Value);
            nomeCliente = cliente?.Nome;
        }

        // Buscar total e quantidade de itens da venda atual
        decimal? totalAtual = null;
        int? quantidadeItens = null;
        if (!string.IsNullOrEmpty(mesa.NotaAtual))
        {
            // Aqui poderia buscar da venda, mas por enquanto deixar null
        }

        return new MesaDto
        {
            Numero = mesa.Numero,
            Status = mesa.Status,
            ComandaAtual = mesa.ComandaAtual,
            NotaAtual = mesa.NotaAtual,
            DataOcupacao = mesa.DataOcupacao,
            HoraOcupacao = mesa.HoraOcupacao,
            Operador = mesa.Operador,
            NumeroPessoas = mesa.NumeroPessoas,
            Cliente = mesa.Cliente,
            NomeCliente = nomeCliente,
            TempoOcupacao = mesa.TempoOcupacao,
            TotalAtual = totalAtual,
            QuantidadeItens = quantidadeItens
        };
    }

    private async Task<IEnumerable<MesaDto>> MapToDtoListAsync(IEnumerable<Models.Entities.Mesa> mesas)
    {
        var result = new List<MesaDto>();
        foreach (var mesa in mesas)
        {
            result.Add(await MapToDtoAsync(mesa));
        }
        return result;
    }
}

