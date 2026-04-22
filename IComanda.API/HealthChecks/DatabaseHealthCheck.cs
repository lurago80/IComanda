using Microsoft.Extensions.Diagnostics.HealthChecks;
using IComanda.API.Data;
using FirebirdSql.Data.FirebirdClient;

namespace IComanda.API.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseHealthCheck(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // CORREÇÃO: usar async/await para não bloquear thread do pool durante o health check
            using var connection = (FbConnection)_connectionFactory.CreateConnection();

            // Timeout de 5 segundos para não travar o health check indefinidamente
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            await connection.OpenAsync(cts.Token);

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM RDB$DATABASE";
            command.CommandTimeout = 5;
            await command.ExecuteScalarAsync(cts.Token);

            return HealthCheckResult.Healthy("Conexão com Firebird OK");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Health check do banco expirou (timeout 5s)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro na conexão com Firebird", ex);
        }
    }
}
