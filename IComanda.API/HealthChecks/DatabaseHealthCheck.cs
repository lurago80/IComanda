using Microsoft.Extensions.Diagnostics.HealthChecks;
using IComanda.API.Data;

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
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            
            // Testa uma query simples
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM RDB$DATABASE";
            command.ExecuteScalar();
            
            return HealthCheckResult.Healthy("Conexão com Firebird OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro na conexão com Firebird", ex);
        }
    }
}
