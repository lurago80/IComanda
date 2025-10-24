using AutoMapper;
using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;

namespace IComanda.API.Services.Implementations;

public class VendaService : IVendaService
{
    private readonly IVendaRepository _vendaRepository;
    private readonly IMapper _mapper;

    public VendaService(IVendaRepository vendaRepository, IMapper mapper)
    {
        _vendaRepository = vendaRepository;
        _mapper = mapper;
    }

    public async Task<VendaDto> CriarVendaAsync(CriarVendaRequest request)
    {
        // Gerar próxima nota
        var nota = await _vendaRepository.GerarProximaNotaAsync();

        // Mapear request para entidade
        var venda = _mapper.Map<Venda>(request);
        venda.Nota = nota;
        venda.Sequencia = nota;

        // Criar itens da venda
        var itens = new List<ItemVenda>();
        for (int i = 0; i < request.Itens.Count; i++)
        {
            var itemRequest = request.Itens[i];
            var item = _mapper.Map<ItemVenda>(itemRequest);
            item.Nota = nota;
            item.Item = i + 1;
            item.Sequencia = int.Parse(nota);
            itens.Add(item);
        }

        venda.Itens = itens;

        // Salvar venda e itens
        var vendaCriada = await _vendaRepository.CriarVendaAsync(venda);
        if (!vendaCriada)
        {
            throw new Exception("Erro ao criar venda");
        }

        var itensCriados = await _vendaRepository.CriarItensVendaAsync(itens);
        if (!itensCriados)
        {
            throw new Exception("Erro ao criar itens da venda");
        }

        // Retornar venda criada
        return _mapper.Map<VendaDto>(venda);
    }

    public async Task<VendaDto?> GetVendaAsync(string nota)
    {
        var venda = await _vendaRepository.GetVendaAsync(nota);
        if (venda == null) return null;

        var itens = await _vendaRepository.GetItensVendaAsync(nota);
        venda.Itens = itens.ToList();

        return _mapper.Map<VendaDto>(venda);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasHojeAsync()
    {
        var vendas = await _vendaRepository.GetVendasHojeAsync();
        return _mapper.Map<IEnumerable<VendaDto>>(vendas);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorComandaAsync(int comanda)
    {
        var vendas = await _vendaRepository.GetVendasPorComandaAsync(comanda);
        return _mapper.Map<IEnumerable<VendaDto>>(vendas);
    }

    public async Task<IEnumerable<VendaDto>> GetVendasPorMesaAsync(int mesa)
    {
        var vendas = await _vendaRepository.GetVendasPorMesaAsync(mesa);
        return _mapper.Map<IEnumerable<VendaDto>>(vendas);
    }
}
