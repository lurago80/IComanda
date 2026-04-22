using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class MesaRepository : IMesaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<MesaRepository> _logger;

    public MesaRepository(IDbConnectionFactory connectionFactory, ILogger<MesaRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Mesa>> GetMesasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Buscar todas as mesas únicas das vendas e criar status baseado em vendas abertas
        var sql = @"
            SELECT DISTINCT
                V.MESA AS Numero,
                CASE 
                    WHEN EXISTS (
                        SELECT 1 FROM VENDAS V2 
                        WHERE V2.MESA = V.MESA 
                          AND V2.LANCADO = 'ABERTO' 
                          AND V2.ORIGEM = 'BA'
                    ) THEN 'OCUPADA'
                    ELSE 'LIVRE'
                END AS Status,
                (SELECT FIRST 1 V3.COMANDA FROM VENDAS V3 
                 WHERE V3.MESA = V.MESA AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS ComandaAtual,
                (SELECT FIRST 1 V3.NOTA FROM VENDAS V3 
                 WHERE V3.MESA = V.MESA AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS NotaAtual,
                (SELECT FIRST 1 V3.DATA_SAIDA FROM VENDAS V3 
                 WHERE V3.MESA = V.MESA AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS DataOcupacao,
                (SELECT FIRST 1 V3.HORA_SAIDA FROM VENDAS V3 
                 WHERE V3.MESA = V.MESA AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS HoraOcupacao,
                (SELECT FIRST 1 V3.OPERADOR FROM VENDAS V3 
                 WHERE V3.MESA = V.MESA AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS Operador,
                (SELECT FIRST 1 V3.NUMERO_PESSOAS FROM VENDAS V3 
                 WHERE V3.MESA = V.MESA AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS NumeroPessoas,
                (SELECT FIRST 1 V3.CLIENTE FROM VENDAS V3 
                 WHERE V3.MESA = V.MESA AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS Cliente
            FROM VENDAS V
            WHERE V.MESA IS NOT NULL 
              AND V.MESA > 0
              AND V.ORIGEM = 'BA'
            ORDER BY V.MESA";

        _logger.LogInformation("🔍 Buscando todas as mesas");

        var mesas = await connection.QueryAsync<Mesa>(sql);
        var mesasList = mesas.ToList();

        // Calcular tempo de ocupação
        foreach (var mesa in mesasList)
        {
            if (mesa.Status == "OCUPADA" && mesa.DataOcupacao.HasValue && mesa.HoraOcupacao.HasValue)
            {
                var dataOcupacao = mesa.DataOcupacao.Value.Date.Add(mesa.HoraOcupacao.Value);
                var tempoOcupacao = DateTime.Now - dataOcupacao;
                if (tempoOcupacao.TotalHours >= 0)
                {
                    mesa.TempoOcupacao = tempoOcupacao;
                }
            }
        }

        return mesasList;
    }

    public async Task<Mesa?> GetMesaPorNumeroAsync(int numero)
    {
        using var connection = _connectionFactory.CreateConnection();

        var sql = @"
            SELECT FIRST 1
                V.MESA AS Numero,
                CASE 
                    WHEN EXISTS (
                        SELECT 1 FROM VENDAS V2 
                        WHERE V2.MESA = @Numero 
                          AND V2.LANCADO = 'ABERTO' 
                          AND V2.ORIGEM = 'BA'
                    ) THEN 'OCUPADA'
                    ELSE 'LIVRE'
                END AS Status,
                (SELECT FIRST 1 V3.COMANDA FROM VENDAS V3 
                 WHERE V3.MESA = @Numero AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS ComandaAtual,
                (SELECT FIRST 1 V3.NOTA FROM VENDAS V3 
                 WHERE V3.MESA = @Numero AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS NotaAtual,
                (SELECT FIRST 1 V3.DATA_SAIDA FROM VENDAS V3 
                 WHERE V3.MESA = @Numero AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS DataOcupacao,
                (SELECT FIRST 1 V3.HORA_SAIDA FROM VENDAS V3 
                 WHERE V3.MESA = @Numero AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS HoraOcupacao,
                (SELECT FIRST 1 V3.OPERADOR FROM VENDAS V3 
                 WHERE V3.MESA = @Numero AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS Operador,
                (SELECT FIRST 1 V3.NUMERO_PESSOAS FROM VENDAS V3 
                 WHERE V3.MESA = @Numero AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS NumeroPessoas,
                (SELECT FIRST 1 V3.CLIENTE FROM VENDAS V3 
                 WHERE V3.MESA = @Numero AND V3.LANCADO = 'ABERTO' AND V3.ORIGEM = 'BA' 
                 ORDER BY V3.DATA_SAIDA DESC, V3.HORA_SAIDA DESC) AS Cliente
            FROM VENDAS V
            WHERE V.MESA = @Numero
              AND V.ORIGEM = 'BA'
            ORDER BY V.DATA_SAIDA DESC, V.HORA_SAIDA DESC";

        _logger.LogInformation("🔍 Buscando mesa {Numero}", numero);

        var mesa = await connection.QueryFirstOrDefaultAsync<Mesa>(sql, new { Numero = numero });

        if (mesa != null && mesa.Status == "OCUPADA" && mesa.DataOcupacao.HasValue && mesa.HoraOcupacao.HasValue)
        {
            var dataOcupacao = mesa.DataOcupacao.Value.Date.Add(mesa.HoraOcupacao.Value);
            var tempoOcupacao = DateTime.Now - dataOcupacao;
            if (tempoOcupacao.TotalHours >= 0)
            {
                mesa.TempoOcupacao = tempoOcupacao;
            }
        }

        return mesa;
    }

    public Task<bool> AtualizarStatusMesaAsync(int numero, string status, int? comanda = null, string? nota = null, int? operador = null, int? cliente = null, int? numeroPessoas = null)
    {
        // Como não há tabela MESA, vamos apenas logar
        // O status é calculado dinamicamente baseado em vendas abertas
        _logger.LogInformation("📝 Atualizando status da mesa {Numero} para {Status}", numero, status);
        return Task.FromResult(true);
    }

    public async Task<IEnumerable<Mesa>> GetMesasOcupadasAsync()
    {
        var mesas = await GetMesasAsync();
        return mesas.Where(m => m.Status == "OCUPADA");
    }

    public async Task<IEnumerable<Mesa>> GetMesasLivresAsync()
    {
        var mesas = await GetMesasAsync();
        return mesas.Where(m => m.Status == "LIVRE");
    }
}

