using Dapper;
using IComanda.API.Data;
using IComanda.API.Models.Entities;
using IComanda.API.Models.Enums;
using IComanda.API.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace IComanda.API.Repositories.Implementations;

public class VisitaFVRepository : IVisitaFVRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<VisitaFVRepository> _logger;

    public VisitaFVRepository(IDbConnectionFactory connectionFactory, ILogger<VisitaFVRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    private const string SelectVisita = @"
        SELECT
            V.ID            AS Id,
            V.ID_VENDEDOR   AS IdVendedor,
            VD.NOME         AS NomeVendedor,
            V.ID_CLIENTE    AS IdCliente,
            C.NOME          AS NomeCliente,
            V.DATA_AGENDADA AS DataAgendada,
            V.DATA_CHECKIN  AS DataCheckin,
            V.DATA_CHECKOUT AS DataCheckout,
            V.LAT_CHECKIN   AS LatCheckin,
            V.LNG_CHECKIN   AS LngCheckin,
            V.LAT_CHECKOUT  AS LatCheckout,
            V.LNG_CHECKOUT  AS LngCheckout,
            V.STATUS        AS Status,
            V.OBS           AS Obs,
            V.RESULTADO     AS Resultado,
            V.ID_PEDIDO_FV  AS IdPedidoFV
        FROM VISITAS_FV V
        LEFT JOIN VENDEDOR VD ON VD.ID = V.ID_VENDEDOR
        LEFT JOIN CLIENTES   C  ON C.ID  = V.ID_CLIENTE";

    public async Task<IEnumerable<VisitaFV>> GetByVendedorAsync(int idVendedor, DateTime? dataInicio = null, DateTime? dataFim = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var whereConditions = new List<string> { "V.ID_VENDEDOR = @IdVendedor" };
        var parameters = new DynamicParameters();
        parameters.Add("@IdVendedor", idVendedor);

        if (dataInicio.HasValue) { whereConditions.Add("V.DATA_AGENDADA >= @DataInicio"); parameters.Add("@DataInicio", dataInicio.Value.Date); }
        if (dataFim.HasValue)   { whereConditions.Add("V.DATA_AGENDADA <= @DataFim");    parameters.Add("@DataFim",    dataFim.Value.Date); }

        var sql = $"{SelectVisita} WHERE {string.Join(" AND ", whereConditions)} ORDER BY V.DATA_AGENDADA";
        return await connection.QueryAsync<VisitaFV>(sql, parameters);
    }

    public async Task<IEnumerable<VisitaFV>> GetHojeAsync(int idVendedor)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $@"
            {SelectVisita}
            WHERE V.ID_VENDEDOR = @IdVendedor
              AND CAST(V.DATA_AGENDADA AS DATE) = CAST('NOW' AS DATE)
            ORDER BY V.DATA_AGENDADA";

        return await connection.QueryAsync<VisitaFV>(sql, new { IdVendedor = idVendedor });
    }

    public async Task<VisitaFV?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"{SelectVisita} WHERE V.ID = @Id";
        return await connection.QuerySingleOrDefaultAsync<VisitaFV>(sql, new { Id = id });
    }

    public async Task<int> AgendarAsync(VisitaFV visita)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO VISITAS_FV (ID_VENDEDOR, ID_CLIENTE, DATA_AGENDADA, STATUS, OBS)
            VALUES (@IdVendedor, @IdCliente, @DataAgendada, @Status, @Obs)
            RETURNING ID";

        var id = await connection.ExecuteScalarAsync<int>(sql, new
        {
            visita.IdVendedor,
            visita.IdCliente,
            visita.DataAgendada,
            Status = (int)visita.Status,
            visita.Obs
        });

        _logger.LogInformation("Visita FV #{Id} agendada para {Data}", id, visita.DataAgendada);
        return id;
    }

    public async Task<bool> CheckinAsync(int id, DateTime dataHora, decimal? lat, decimal? lng)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE VISITAS_FV SET
                DATA_CHECKIN = @DataHora,
                LAT_CHECKIN  = @Lat,
                LNG_CHECKIN  = @Lng,
                STATUS       = @Status
            WHERE ID = @Id AND STATUS = 0";

        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            DataHora = dataHora,
            Lat = lat,
            Lng = lng,
            Status = (int)StatusVisitaFV.EmAndamento
        });

        return rows > 0;
    }

    public async Task<bool> ConcluirAsync(int id, DateTime dataHora, decimal? lat, decimal? lng, string? resultado, string? obs, int? idPedidoFV)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE VISITAS_FV SET
                DATA_CHECKOUT = @DataHora,
                LAT_CHECKOUT  = @Lat,
                LNG_CHECKOUT  = @Lng,
                STATUS        = @Status,
                RESULTADO     = @Resultado,
                OBS           = COALESCE(@Obs, OBS),
                ID_PEDIDO_FV  = @IdPedidoFV
            WHERE ID = @Id AND STATUS = 1";

        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            DataHora = dataHora,
            Lat = lat,
            Lng = lng,
            Status = (int)StatusVisitaFV.Concluida,
            Resultado = resultado,
            Obs = obs,
            IdPedidoFV = idPedidoFV
        });

        return rows > 0;
    }

    public async Task<bool> MarcarNaoRealizadaAsync(int id, string? obs)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE VISITAS_FV SET
                STATUS = @Status,
                OBS    = COALESCE(@Obs, OBS)
            WHERE ID = @Id AND STATUS IN (0, 1)";

        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Status = (int)StatusVisitaFV.NaoRealizada,
            Obs = obs
        });

        return rows > 0;
    }
}
