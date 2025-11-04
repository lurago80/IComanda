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
    private readonly IItemVendaTemporarioRepository _itemTemporarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<VendaService> _logger;

    public VendaService(
        IVendaRepository vendaRepository, 
        IItemVendaTemporarioRepository itemTemporarioRepository,
        IMapper mapper,
        ILogger<VendaService> logger)
    {
        _vendaRepository = vendaRepository;
        _itemTemporarioRepository = itemTemporarioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<VendaDto> CriarVendaAsync(CriarVendaRequest request)
    {
        _logger.LogInformation("🔄 Criando venda temporária (será finalizada pelo Delphi Desktop)");
        
        // Gerar próxima nota
        var nota = await _vendaRepository.GerarProximaNotaAsync();
        var operadorId = request.Operador == 0 ? 1 : request.Operador;

        _logger.LogInformation($"📝 Nota gerada: {nota} | Operador: {operadorId}");

        // Calcular totais
        decimal totalProdutos = request.Itens.Sum(i => i.Preco * i.Qtd);
        decimal totalFinal = totalProdutos - request.Desconto + request.Acrescimo;

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
            Origem = "BA",
            Emissao = DateTime.Now,
            Hora = DateTime.Now.TimeOfDay,
            DataSaida = DateTime.Now,
            HoraSaida = DateTime.Now.TimeOfDay,
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
            NumeroPessoas = request.NumeroPessoas
        };

        // Criar itens TEMPORÁRIOS (frente_tmpitvendas) - NÃO vai para ITVENDAS ainda
        var itensTemporarios = new List<ItemVendaTemporario>();
        for (int i = 0; i < request.Itens.Count; i++)
        {
            var itemRequest = request.Itens[i];
            var itemTemp = new ItemVendaTemporario
            {
                Cupom = nota,
                NCaixa = "1",
                Data = DateTime.Now.Date,
                Hora = DateTime.Now.TimeOfDay,
                Operador = operadorId,
                Item = i + 1,
                Codigo = itemRequest.Codigo,
                Barras = "", // TODO: buscar do produto se necessário
                Descricao = "", // TODO: buscar do produto se necessário
                Qtd = itemRequest.Qtd,
                Preco = itemRequest.Preco,
                Tributacao = "",
                Icms = 0,
                Iss = 0,
                Und = "UN",
                Desconto = 0,
                Acrescimo = 0,
                Total = itemRequest.Preco * itemRequest.Qtd,
                Serial = "",
                Tipo = 1
            };
            itensTemporarios.Add(itemTemp);
        }

        _logger.LogInformation($"💾 Salvando venda ABERTA na tabela VENDAS");
        
        // Salvar venda (VENDAS) com status ABERTO
        var vendaCriada = await _vendaRepository.CriarVendaAsync(venda);
        if (!vendaCriada)
        {
            _logger.LogError("❌ Erro ao criar venda");
            throw new Exception("Erro ao criar venda");
        }

        _logger.LogInformation($"📦 Salvando {itensTemporarios.Count} itens na tabela TEMPORÁRIA (frente_tmpitvendas)");
        
        // Salvar itens TEMPORÁRIOS (frente_tmpitvendas) - NÃO em ITVENDAS
        var itensTemporariosCriados = await _itemTemporarioRepository.CriarItensTemporariosAsync(itensTemporarios);
        if (!itensTemporariosCriados)
        {
            _logger.LogError("❌ Erro ao criar itens temporários");
            throw new Exception("Erro ao criar itens temporários da venda");
        }

        _logger.LogInformation($"✅ Venda {nota} criada com sucesso! Status: ABERTO (aguardando finalização pelo Delphi Desktop)");

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
