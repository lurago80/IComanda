using IComanda.API.Models.DTOs;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using IComanda.API.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Services.Implementations;

public class CaixaService : ICaixaService
{
    private readonly ICaixaRepository _caixaRepository;
    private readonly ICaixaMovimentoRepository _caixaMovimentoRepository;
    private readonly ILogger<CaixaService> _logger;

    public CaixaService(
        ICaixaRepository caixaRepository,
        ICaixaMovimentoRepository caixaMovimentoRepository,
        ILogger<CaixaService> logger)
    {
        _caixaRepository = caixaRepository;
        _caixaMovimentoRepository = caixaMovimentoRepository;
        _logger = logger;
    }

    public async Task<CaixaDto> AbrirCaixaAsync(AbrirCaixaRequest request)
    {
        _logger.LogInformation("💰 Abrindo caixa {Numero} - Operador: {Operador}, Valor: {Valor}", 
            request.Numero, request.Operador, request.ValorAbertura);

        // Verificar se já existe caixa aberto
        var caixaAberto = await _caixaRepository.GetCaixaAbertoAsync(request.Numero);
        if (caixaAberto != null && caixaAberto.Status == "ABERTO")
        {
            throw new InvalidOperationException($"Caixa {request.Numero} já está aberto");
        }

        var caixa = new Caixa
        {
            Numero = request.Numero,
            DataAbertura = DateTime.Now,
            OperadorAbertura = request.Operador,
            ValorAbertura = request.ValorAbertura,
            Status = "ABERTO",
            Observacoes = request.Observacoes
        };

        await _caixaRepository.CriarCaixaAsync(caixa);

        _logger.LogInformation("✅ Caixa {Numero} aberto com sucesso", request.Numero);

        return await GetCaixaAbertoAsync(request.Numero) ?? 
            throw new Exception("Erro ao buscar caixa após abertura");
    }

    public async Task<CaixaDto> FecharCaixaAsync(FecharCaixaRequest request)
    {
        _logger.LogInformation("💰 Fechando caixa {Id} - Operador: {Operador}, Valor: {Valor}", 
            request.Id, request.Operador, request.ValorFechamento);

        var caixa = await _caixaRepository.GetCaixaPorIdAsync(request.Id);
        if (caixa == null)
        {
            throw new InvalidOperationException($"Caixa {request.Id} não encontrado");
        }

        // Verificar fechamento manual pelo registro na tabela CAIXA (não pelo status das vendas)
        var jaFechado = await _caixaMovimentoRepository.ExisteFechamentoAsync(caixa.Numero, DateTime.Today);
        if (jaFechado)
        {
            throw new InvalidOperationException($"Caixa {caixa.Numero} já foi fechado manualmente hoje");
        }

        caixa.DataFechamento = DateTime.Now;
        caixa.OperadorFechamento = request.Operador;
        caixa.ValorFechamento = request.ValorFechamento;
        caixa.Status = "FECHADO";
        caixa.Observacoes = request.Observacoes;

        await _caixaRepository.AtualizarCaixaAsync(caixa);

        // Gravar movimento de FECHAMENTO na tabela CAIXA para persistir o fechamento manual.
        // Isso impede que o caixa reapareça como "ABERTO" automaticamente caso ainda
        // existam vendas abertas ou quando o status é derivado da tabela VENDAS.
        var movimentoFechamento = new Models.Entities.CaixaMovimento
        {
            Data = DateTime.Now,
            Hora = DateTime.Now.TimeOfDay,
            Documento = $"FECHAMENTO-{caixa.Numero}-{DateTime.Now:yyyyMMdd}",
            Entrada = 0,
            Saida = 0,
            Origem = "FECHAMENTO",
            Operador = request.Operador,
            Historico = request.Observacoes ?? $"Fechamento manual de caixa - Terminal {caixa.Numero}",
            Gravacao = DateTime.Now,
            Terminal = caixa.Numero,
            Vendedor = 0
        };
        await _caixaMovimentoRepository.CriarMovimentoAsync(movimentoFechamento);

        _logger.LogInformation("✅ Caixa {Id} fechado manualmente com sucesso", request.Id);

        return await GetRelatorioCaixaAsync(request.Id);
    }

    public async Task<CaixaDto?> GetCaixaAbertoAsync(int numero)
    {
        // Verificar primeiro se houve fechamento manual hoje — nesse caso o caixa está fechado
        var fechamentoHoje = await _caixaMovimentoRepository.ExisteFechamentoAsync(numero, DateTime.Today);
        if (fechamentoHoje)
        {
            return null;
        }

        var caixa = await _caixaRepository.GetCaixaAbertoAsync(numero);
        if (caixa == null)
        {
            return null;
        }

        return await MapToDtoAsync(caixa);
    }

    public async Task<CaixaDto?> GetCaixaPorIdAsync(int id)
    {
        var caixa = await _caixaRepository.GetCaixaPorIdAsync(id);
        if (caixa == null)
        {
            return null;
        }

        return await MapToDtoAsync(caixa);
    }

    public async Task<IEnumerable<CaixaDto>> GetCaixasAsync(DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var caixas = await _caixaRepository.GetCaixasAsync(dataInicio, dataFim);
        var caixasList = caixas.ToList();

        // Incluir caixas que têm movimento de ABERTURA na tabela CAIXA (mesmo sem vendas)
        var aberturas = await _caixaMovimentoRepository.GetAberturasPorPeriodoAsync(dataInicio, dataFim);
        var aberturasPorTerminal = aberturas
            .GroupBy(m => m.Terminal)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.Data).ThenByDescending(m => m.Hora).First());

        foreach (var kv in aberturasPorTerminal)
        {
            var terminal = kv.Key;
            var movimento = kv.Value;
            if (caixasList.Any(c => c.Numero == terminal))
                continue;

            var dataHoraAbertura = movimento.Data.Date.Add(movimento.Hora);
            caixasList.Add(new Caixa
            {
                Id = terminal,
                Numero = terminal,
                DataAbertura = dataHoraAbertura,
                OperadorAbertura = movimento.Operador,
                Status = "ABERTO",
                ValorAbertura = movimento.Entrada
            });
        }

        // Determinar o status correto baseado em movimento de FECHAMENTO manual.
        // Um caixa só é considerado FECHADO se houver um registro explícito de FECHAMENTO
        // na tabela CAIXA — impedindo fechamento automático baseado no status das vendas.
        var fechamentos = await _caixaMovimentoRepository.GetFechamentosPorPeriodoAsync(dataInicio, dataFim);
        var terminaisComFechamento = new HashSet<int>(fechamentos.Select(f => f.Terminal));

        foreach (var caixa in caixasList)
        {
            if (terminaisComFechamento.Contains(caixa.Numero))
            {
                // Fechamento manual registrado → FECHADO
                caixa.Status = "FECHADO";
                var movFechamento = fechamentos
                    .Where(f => f.Terminal == caixa.Numero)
                    .OrderByDescending(f => f.Data)
                    .ThenByDescending(f => f.Hora)
                    .First();
                caixa.DataFechamento = movFechamento.Data.Date.Add(movFechamento.Hora);
                caixa.OperadorFechamento = movFechamento.Operador;
            }
            else
            {
                // Sem FECHAMENTO manual → sempre ABERTO, independente do status das vendas
                caixa.Status = "ABERTO";
                caixa.DataFechamento = null;
            }
        }

        var result = new List<CaixaDto>();
        foreach (var caixa in caixasList.OrderByDescending(c => c.Numero))
        {
            var dto = await MapToDtoAsync(caixa, dataInicio, dataFim);
            result.Add(dto);
        }

        return result;
    }

    public async Task<CaixaDto> GetRelatorioCaixaAsync(int caixaId, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        var caixa = await _caixaRepository.GetCaixaPorIdAsync(caixaId);
        if (caixa == null)
        {
            // Caixa pode existir só por abertura (sem vendas) – buscar na tabela CAIXA
            var aberturas = await _caixaMovimentoRepository.GetAberturasPorPeriodoAsync(dataInicio, dataFim);
            var abertura = aberturas.FirstOrDefault(m => m.Terminal == caixaId);
            if (abertura != null)
            {
                caixa = new Caixa
                {
                    Id = caixaId,
                    Numero = caixaId,
                    DataAbertura = abertura.Data.Date.Add(abertura.Hora),
                    OperadorAbertura = abertura.Operador,
                    Status = "ABERTO",
                    ValorAbertura = abertura.Entrada
                };
            }
        }

        if (caixa == null)
        {
            throw new InvalidOperationException($"Caixa {caixaId} não encontrado");
        }

        // Determinar status com base em FECHAMENTO manual (não nas vendas)
        var fechamentos = await _caixaMovimentoRepository.GetFechamentosPorPeriodoAsync(
            caixa.DataAbertura.Date, caixa.DataAbertura.Date.AddDays(1));
        var movFechamento = fechamentos.FirstOrDefault(f => f.Terminal == caixa.Numero);
        if (movFechamento != null)
        {
            caixa.Status = "FECHADO";
            caixa.DataFechamento = movFechamento.Data.Date.Add(movFechamento.Hora);
            caixa.OperadorFechamento = movFechamento.Operador;
        }
        else
        {
            caixa.Status = "ABERTO";
            caixa.DataFechamento = null;
        }

        var dto = await MapToDtoAsync(caixa, dataInicio, dataFim);
        return dto;
    }

    private async Task<CaixaDto> MapToDtoAsync(Caixa caixa, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        // Valor e data/hora de abertura: buscar na tabela CAIXA (movimento ABERTURA) para ter hora real
        var primeiraAbertura = await _caixaMovimentoRepository.GetPrimeiraAberturaAsync(caixa.Numero, caixa.DataAbertura);
        var valorAberturaUsar = primeiraAbertura != null ? primeiraAbertura.Entrada : (await _caixaMovimentoRepository.GetValorAberturaAsync(caixa.Numero, caixa.DataAbertura)) ?? caixa.ValorAbertura;
        var dataAberturaExibir = primeiraAbertura != null ? primeiraAbertura.Data.Date.Add(primeiraAbertura.Hora) : caixa.DataAbertura;

        // Calcular totais
        var totalVendas = await _caixaRepository.GetTotalVendasPorCaixaAsync(caixa.Numero, dataInicio, dataFim);
        var totalRecebimentos = await _caixaRepository.GetTotalRecebimentosPorCaixaAsync(caixa.Numero, dataInicio, dataFim);

        var dataInicioMov = dataInicio ?? caixa.DataAbertura.Date;
        var dataFimMov = dataFim ?? (caixa.DataFechamento?.Date ?? DateTime.Now.Date).AddDays(1).AddSeconds(-1);
        var movimentos = (await _caixaMovimentoRepository.GetMovimentosAsync(caixa.Numero, dataInicioMov, dataFimMov)).ToList();
        var movimentosDto = movimentos.Select(MapMovimentoToDto).ToList();
        var totalSaidas = movimentos.Sum(m => m.Saida);

        var saldoEsperado = valorAberturaUsar + totalRecebimentos - totalSaidas;
        var diferenca = (caixa.ValorFechamento ?? 0) - saldoEsperado;

        return new CaixaDto
        {
            Id = caixa.Id,
            Numero = caixa.Numero,
            DataAbertura = dataAberturaExibir,
            DataFechamento = caixa.DataFechamento,
            OperadorAbertura = caixa.OperadorAbertura,
            OperadorFechamento = caixa.OperadorFechamento,
            ValorAbertura = valorAberturaUsar,
            ValorFechamento = caixa.ValorFechamento,
            Status = caixa.Status,
            Observacoes = caixa.Observacoes,
            TotalVendas = totalVendas,
            TotalRecebimentos = totalRecebimentos,
            SaldoEsperado = saldoEsperado,
            Diferenca = diferenca,
            TotalSaidas = totalSaidas,
            Movimentos = movimentosDto
        };
    }

    private static CaixaMovimentoDto MapMovimentoToDto(CaixaMovimento m)
    {
        return new CaixaMovimentoDto
        {
            Codigo = m.Codigo,
            Data = m.Data,
            Hora = m.Hora,
            Documento = m.Documento,
            Custo = m.Custo,
            Conta = m.Conta,
            Entrada = m.Entrada,
            Saida = m.Saida,
            Saldo = m.Saldo,
            Origem = m.Origem,
            Operador = m.Operador,
            Historico = m.Historico,
            Gravacao = m.Gravacao,
            CodProf = m.CodProf,
            Terminal = m.Terminal,
            Vendedor = m.Vendedor
        };
    }
}

