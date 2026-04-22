using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Enums;
using IComanda.API.Models.Requests;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class PedidoFVRepository : IPedidoFVRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<PedidoFVRepository> _logger;

    private const string SelectPedidoFV = @"
        SELECT
            P.ID            AS Id,
            P.ID_VENDEDOR   AS IdVendedor,
            V.NOME          AS NomeVendedor,
            P.ID_CLIENTE    AS IdCliente,
            C.NOME          AS NomeCliente,
            P.DATA_PEDIDO   AS DataPedido,
            P.HORA_PEDIDO   AS HoraPedido,
            P.STATUS        AS Status,
            P.SUBTOTAL      AS Subtotal,
            P.DESCONTO      AS Desconto,
            P.ACRESCIMO     AS Acrescimo,
            P.TOTAL         AS Total,
            P.OBS           AS Obs,
            P.CONDICAO_PGTO AS CondicaoPgto,
            P.TABELA_PRECO  AS TabelaPreco,
            P.DATA_APROVACAO AS DataAprovacao,
            P.ID_APROVADOR  AS IdAprovador,
            P.MOTIVO_CANCEL AS MotivoCancel,
            P.NOTA_FISCAL   AS NotaFiscal,
            P.DATA_FATURAMENTO AS DataFaturamento
        FROM PEDIDOS_FV P
        LEFT JOIN VENDEDOR V ON V.ID = P.ID_VENDEDOR
        LEFT JOIN CLIENTES   C ON C.ID = P.ID_CLIENTE";

    public PedidoFVRepository(IDbConnectionFactory connectionFactory, ILogger<PedidoFVRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<PedidoFV>> BuscarAsync(BuscarPedidoFVRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (request.IdVendedor.HasValue)
        {
            whereConditions.Add("P.ID_VENDEDOR = @IdVendedor");
            parameters.Add("@IdVendedor", request.IdVendedor.Value);
        }

        if (request.IdCliente.HasValue)
        {
            whereConditions.Add("P.ID_CLIENTE = @IdCliente");
            parameters.Add("@IdCliente", request.IdCliente.Value);
        }

        if (request.Status.HasValue)
        {
            whereConditions.Add("P.STATUS = @Status");
            parameters.Add("@Status", request.Status.Value);
        }

        if (request.DataInicio.HasValue)
        {
            whereConditions.Add("P.DATA_PEDIDO >= @DataInicio");
            parameters.Add("@DataInicio", request.DataInicio.Value.Date);
        }

        if (request.DataFim.HasValue)
        {
            whereConditions.Add("P.DATA_PEDIDO <= @DataFim");
            parameters.Add("@DataFim", request.DataFim.Value.Date);
        }

        var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

        var limit  = request.ItensPorPagina ?? 50;
        var offset = ((request.Pagina ?? 1) - 1) * limit;

        var sql = $@"
            SELECT FIRST {limit} SKIP {offset}
            {SelectPedidoFV.Replace("SELECT", "")}
            {whereClause}
            ORDER BY P.DATA_PEDIDO DESC, P.HORA_PEDIDO DESC";

        // Fix: rebuild the query properly
        sql = $@"
            SELECT FIRST {limit} SKIP {offset}
                P.ID            AS Id,
                P.ID_VENDEDOR   AS IdVendedor,
                V.NOME          AS NomeVendedor,
                P.ID_CLIENTE    AS IdCliente,
                C.NOME          AS NomeCliente,
                P.DATA_PEDIDO   AS DataPedido,
                P.HORA_PEDIDO   AS HoraPedido,
                P.STATUS        AS Status,
                P.SUBTOTAL      AS Subtotal,
                P.DESCONTO      AS Desconto,
                P.ACRESCIMO     AS Acrescimo,
                P.TOTAL         AS Total,
                P.OBS           AS Obs,
                P.CONDICAO_PGTO AS CondicaoPgto,
                P.TABELA_PRECO  AS TabelaPreco,
                P.DATA_APROVACAO AS DataAprovacao,
                P.ID_APROVADOR  AS IdAprovador,
                P.MOTIVO_CANCEL AS MotivoCancel,
                P.NOTA_FISCAL   AS NotaFiscal,
                P.DATA_FATURAMENTO AS DataFaturamento
            FROM PEDIDOS_FV P
            LEFT JOIN VENDEDOR V ON V.ID = P.ID_VENDEDOR
            LEFT JOIN CLIENTES   C ON C.ID = P.ID_CLIENTE
            {whereClause}
            ORDER BY P.DATA_PEDIDO DESC, P.HORA_PEDIDO DESC";

        var pedidos = (await connection.QueryAsync<PedidoFV>(sql, parameters)).ToList();

        // Carregar itens de cada pedido
        if (pedidos.Any())
        {
            var ids = pedidos.Select(p => p.Id).ToList();
            var sqlItens = @"
                SELECT
                    I.ID            AS Id,
                    I.ID_PEDIDO_FV  AS IdPedidoFV,
                    I.ID_PRODUTO    AS IdProduto,
                    I.CODIGO        AS CodigoProduto,
                    I.DESCRICAO     AS DescricaoProduto,
                    I.QUANTIDADE    AS Quantidade,
                    I.UNIDADE       AS Unidade,
                    I.PRECO_UNIT    AS PrecoUnitario,
                    I.DESCONTO      AS Desconto,
                    I.TOTAL         AS Total,
                    I.OBS           AS Obs
                FROM ITENS_PEDIDO_FV I
                WHERE I.ID_PEDIDO_FV IN @Ids";

            var todosItens = (await connection.QueryAsync<ItemPedidoFV>(sqlItens, new { Ids = ids })).ToList();

            foreach (var pedido in pedidos)
            {
                pedido.Itens = todosItens.Where(i => i.IdPedidoFV == pedido.Id).ToList();
            }
        }

        return pedidos;
    }

    public async Task<PedidoFV?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                P.ID            AS Id,
                P.ID_VENDEDOR   AS IdVendedor,
                V.NOME          AS NomeVendedor,
                P.ID_CLIENTE    AS IdCliente,
                C.NOME          AS NomeCliente,
                P.DATA_PEDIDO   AS DataPedido,
                P.HORA_PEDIDO   AS HoraPedido,
                P.STATUS        AS Status,
                P.SUBTOTAL      AS Subtotal,
                P.DESCONTO      AS Desconto,
                P.ACRESCIMO     AS Acrescimo,
                P.TOTAL         AS Total,
                P.OBS           AS Obs,
                P.CONDICAO_PGTO AS CondicaoPgto,
                P.TABELA_PRECO  AS TabelaPreco,
                P.DATA_APROVACAO AS DataAprovacao,
                P.ID_APROVADOR  AS IdAprovador,
                P.MOTIVO_CANCEL AS MotivoCancel,
                P.NOTA_FISCAL   AS NotaFiscal,
                P.DATA_FATURAMENTO AS DataFaturamento
            FROM PEDIDOS_FV P
            LEFT JOIN VENDEDOR V ON V.ID = P.ID_VENDEDOR
            LEFT JOIN CLIENTES   C ON C.ID = P.ID_CLIENTE
            WHERE P.ID = @Id";

        var pedido = await connection.QuerySingleOrDefaultAsync<PedidoFV>(sql, new { Id = id });

        if (pedido != null)
        {
            const string sqlItens = @"
                SELECT
                    I.ID            AS Id,
                    I.ID_PEDIDO_FV  AS IdPedidoFV,
                    I.ID_PRODUTO    AS IdProduto,
                    I.CODIGO        AS CodigoProduto,
                    I.DESCRICAO     AS DescricaoProduto,
                    I.QUANTIDADE    AS Quantidade,
                    I.UNIDADE       AS Unidade,
                    I.PRECO_UNIT    AS PrecoUnitario,
                    I.DESCONTO      AS Desconto,
                    I.TOTAL         AS Total,
                    I.OBS           AS Obs
                FROM ITENS_PEDIDO_FV I
                WHERE I.ID_PEDIDO_FV = @Id";

            pedido.Itens = (await connection.QueryAsync<ItemPedidoFV>(sqlItens, new { Id = id })).ToList();
        }

        return pedido;
    }

    public async Task<IEnumerable<PedidoFV>> GetByVendedorAsync(int idVendedor, int? status = null)
    {
        var request = new BuscarPedidoFVRequest
        {
            IdVendedor = idVendedor,
            Status = status,
            ItensPorPagina = 100
        };
        return await BuscarAsync(request);
    }

    public async Task<IEnumerable<PedidoFV>> GetPendenteAsync()
    {
        var request = new BuscarPedidoFVRequest
        {
            Status = (int)StatusPedidoFV.Pendente,
            ItensPorPagina = 200
        };
        return await BuscarAsync(request);
    }

    public async Task<int> CriarAsync(PedidoFV pedido, IEnumerable<ItemPedidoFV> itens)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string sqlPedido = @"
                INSERT INTO PEDIDOS_FV
                    (ID_VENDEDOR, ID_CLIENTE, DATA_PEDIDO, HORA_PEDIDO, STATUS,
                     SUBTOTAL, DESCONTO, ACRESCIMO, TOTAL, OBS, CONDICAO_PGTO, TABELA_PRECO)
                VALUES
                    (@IdVendedor, @IdCliente, @DataPedido, @HoraPedido, @Status,
                     @Subtotal, @Desconto, @Acrescimo, @Total, @Obs, @CondicaoPgto, @TabelaPreco)
                RETURNING ID";

            var id = await connection.ExecuteScalarAsync<int>(sqlPedido, new
            {
                pedido.IdVendedor,
                pedido.IdCliente,
                DataPedido  = pedido.DataPedido.Date,
                HoraPedido  = pedido.DataPedido.TimeOfDay,
                Status      = (int)pedido.Status,
                pedido.Subtotal,
                pedido.Desconto,
                pedido.Acrescimo,
                pedido.Total,
                pedido.Obs,
                pedido.CondicaoPgto,
                pedido.TabelaPreco
            }, transaction);

            const string sqlItem = @"
                INSERT INTO ITENS_PEDIDO_FV
                    (ID_PEDIDO_FV, ID_PRODUTO, CODIGO, DESCRICAO, QUANTIDADE, UNIDADE,
                     PRECO_UNIT, DESCONTO, TOTAL, OBS)
                VALUES
                    (@IdPedidoFV, @IdProduto, @CodigoProduto, @DescricaoProduto, @Quantidade, @Unidade,
                     @PrecoUnitario, @Desconto, @Total, @Obs)";

            foreach (var item in itens)
            {
                item.IdPedidoFV = id;
                await connection.ExecuteAsync(sqlItem, item, transaction);
            }

            transaction.Commit();
            _logger.LogInformation("Pedido FV #{Id} criado com {Count} itens", id, itens.Count());
            return id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> AtualizarStatusAsync(int id, int status, int? idAprovador, string? motivo, string? notaFiscal)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE PEDIDOS_FV SET
                STATUS          = @Status,
                ID_APROVADOR    = @IdAprovador,
                MOTIVO_CANCEL   = @Motivo,
                NOTA_FISCAL     = @NotaFiscal,
                DATA_APROVACAO  = CASE WHEN @Status IN (1, 3) THEN CURRENT_TIMESTAMP ELSE DATA_APROVACAO END,
                DATA_FATURAMENTO = CASE WHEN @Status = 2 THEN CURRENT_TIMESTAMP ELSE DATA_FATURAMENTO END
            WHERE ID = @Id";

        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status,
            IdAprovador = idAprovador,
            Motivo = motivo,
            NotaFiscal = notaFiscal
        });

        return rows > 0;
    }

    public async Task<int> ContarAsync(BuscarPedidoFVRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (request.IdVendedor.HasValue) { whereConditions.Add("P.ID_VENDEDOR = @IdVendedor"); parameters.Add("@IdVendedor", request.IdVendedor.Value); }
        if (request.Status.HasValue)    { whereConditions.Add("P.STATUS = @Status");           parameters.Add("@Status", request.Status.Value); }

        var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";
        var sql = $"SELECT COUNT(*) FROM PEDIDOS_FV P {whereClause}";
        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }
}
