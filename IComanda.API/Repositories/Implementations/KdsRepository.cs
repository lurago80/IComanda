using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.DTOs;
using IComanda.API.Repositories.Interfaces;

namespace IComanda.API.Repositories.Implementations;

public class KdsRepository : IKdsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<KdsRepository> _logger;
    internal static bool StatusCozinhaColumnMissing = false;

    public KdsRepository(IDbConnectionFactory connectionFactory, ILogger<KdsRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<bool> EnsureStatusCozinhaColumnAsync()
    {
        if (StatusCozinhaColumnMissing) return false;
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "ALTER TABLE VENDAS ADD STATUS_COZINHA VARCHAR(20) DEFAULT 'PENDENTE'");
            _logger.LogInformation("KDS: coluna STATUS_COZINHA criada na tabela VENDAS.");
        }
        catch (Exception ex) when (ex.Message.Contains("already") || ex.Message.Contains("ALREADY") || ex.Message.Contains("exists") || ex.Message.Contains("duplicate") || ex.Message.Contains("COLUMN") || ex.Message.Contains("column"))
        {
            // Coluna já existe — normal
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "KDS: não foi possível garantir coluna STATUS_COZINHA.");
        }
        return true;
    }

    public async Task<IEnumerable<KdsPedidoDto>> GetPedidosAtivosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Busca pedidos abertos que ainda não foram entregues na cozinha
        const string sqlComStatus = @"
            SELECT
                v.NOTA,
                v.ORIGEM,
                v.COMANDA,
                v.MESA,
                v.NOME_CLIENTE,
                COALESCE(v.STATUS_COZINHA, 'PENDENTE') AS STATUS_COZINHA,
                v.LANCADO,
                v.EMISSAO,
                v.HORA
            FROM VENDAS v
            WHERE v.LANCADO IN ('ABERTO', 'SAINDO')
              AND COALESCE(v.STATUS_COZINHA, 'PENDENTE') <> 'ENTREGUE'
            ORDER BY v.EMISSAO ASC, v.HORA ASC";

        const string sqlSemStatus = @"
            SELECT
                v.NOTA,
                v.ORIGEM,
                v.COMANDA,
                v.MESA,
                v.NOME_CLIENTE,
                'PENDENTE' AS STATUS_COZINHA,
                v.LANCADO,
                v.EMISSAO,
                v.HORA
            FROM VENDAS v
            WHERE v.LANCADO IN ('ABERTO', 'SAINDO')
            ORDER BY v.EMISSAO ASC, v.HORA ASC";

        IEnumerable<dynamic> vendas;
        try
        {
            vendas = await connection.QueryAsync<dynamic>(StatusCozinhaColumnMissing ? sqlSemStatus : sqlComStatus);
        }
        catch (Exception ex) when (ex.Message.ToUpperInvariant().Contains("STATUS_COZINHA"))
        {
            StatusCozinhaColumnMissing = true;
            vendas = await connection.QueryAsync<dynamic>(sqlSemStatus);
        }

        var notas = vendas.Select(v => (string)v.NOTA).ToList();
        if (!notas.Any()) return Enumerable.Empty<KdsPedidoDto>();

        // Busca itens de todas as vendas encontradas de uma vez
        var sqlItens = @"
            SELECT i.NOTA, i.ITEM, i.CODIGO,
                   COALESCE(NULLIF(TRIM(p.DESCRICAO), ''), i.SERIAL, '') AS DESCRICAO,
                   i.QTD,
                   i.SERIAL AS OBSERVACAO
            FROM ITEVENDAS i
            LEFT JOIN PRODUTOESERVICO p ON p.ID = i.CODIGO
            WHERE i.NOTA IN @Notas
              AND CAST(i.CANCELADO AS INTEGER) = 0
            ORDER BY i.NOTA, i.ITEM";

        var itens = (await connection.QueryAsync<dynamic>(sqlItens, new { Notas = notas })).ToList();

        var resultado = new List<KdsPedidoDto>();
        var agora = DateTime.Now;

        foreach (var v in vendas)
        {
            var pedidoItens = itens
                .Where(i => i.NOTA == v.NOTA)
                .Select(i => new KdsItemDto
                {
                    Item = (int)i.ITEM,
                    Codigo = (int)i.CODIGO,
                    Descricao = (string)(i.DESCRICAO ?? ""),
                    Qtd = (decimal)i.QTD,
                    Observacao = string.IsNullOrWhiteSpace((string?)i.OBSERVACAO) ? null : (string)i.OBSERVACAO
                }).ToList();

            if (!pedidoItens.Any()) continue;

            var emissao = (DateTime)v.EMISSAO;
            var hora = v.HORA is TimeSpan ts ? ts : TimeSpan.Zero;
            var dataHoraPedido = emissao.Date.Add(hora);
            var minutos = (int)(agora - dataHoraPedido).TotalMinutes;

            resultado.Add(new KdsPedidoDto
            {
                Nota = (string)v.NOTA,
                Origem = (string)(v.ORIGEM ?? "BA"),
                Comanda = v.COMANDA is int c ? c : (int?)null,
                Mesa = v.MESA is int m ? m : (int?)null,
                NomeCliente = v.NOME_CLIENTE is string nc && !string.IsNullOrWhiteSpace(nc) ? nc : null,
                StatusCozinha = (string)(v.STATUS_COZINHA ?? "PENDENTE"),
                Lancado = (string)(v.LANCADO ?? "ABERTO"),
                Emissao = emissao,
                Hora = hora,
                MinutosEspera = Math.Max(0, minutos),
                Itens = pedidoItens
            });
        }

        return resultado;
    }

    public async Task<bool> AtualizarStatusCozinhaAsync(string nota, string statusCozinha)
    {
        if (StatusCozinhaColumnMissing)
        {
            _logger.LogWarning("KDS: tentativa de atualizar STATUS_COZINHA mas coluna não existe.");
            return false;
        }

        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE VENDAS SET STATUS_COZINHA = @Status WHERE NOTA = @Nota";
        var affected = await connection.ExecuteAsync(sql, new { Status = statusCozinha, Nota = nota });
        return affected > 0;
    }
}
