using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace IComanda.API.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration, ILogger<DbConnectionFactory> logger)
    {
        var raw = configuration.GetConnectionString("Firebird")
            ?? throw new InvalidOperationException(
                "Connection string 'Firebird' não encontrada. " +
                "Configure em appsettings.json ou via variável de ambiente ConnectionStrings__Firebird.");

        // Garantir Charset=UTF8 para caracteres com acentuação
        var conn = raw.TrimEnd(' ', ';');
        if (!conn.Contains("Charset=", StringComparison.OrdinalIgnoreCase))
            conn += ";Charset=UTF8";

        // Timeout para evitar threads travadas quando o Firebird está lento ou indisponível
        if (!conn.Contains("ConnectionTimeout=", StringComparison.OrdinalIgnoreCase) &&
            !conn.Contains("connection timeout=", StringComparison.OrdinalIgnoreCase))
            conn += ";ConnectionTimeout=15";

        // ConnectionLifeTime=60 — descarta conexões do pool após 60s,
        // evitando o timeout TCP de 120s por conexões "zumbis" (causa do delay de 2 min)
        if (!conn.Contains("ConnectionLifeTime=", StringComparison.OrdinalIgnoreCase))
            conn += ";ConnectionLifeTime=60";

        // MinPoolSize=0 — permite que o pool feche conexões ociosas ao invés de mantê-las
        if (!conn.Contains("MinPoolSize=", StringComparison.OrdinalIgnoreCase))
            conn += ";MinPoolSize=0";

        _connectionString = conn;

        var dbPath = ExtractDatabasePath(_connectionString);
        logger.LogInformation("DbConnectionFactory inicializado. Banco: {DatabasePath} | Existe: {Exists}",
            dbPath, File.Exists(dbPath));

        if (!File.Exists(dbPath))
            logger.LogWarning(
                "Arquivo de banco NÃO encontrado em: {DatabasePath}. Verifique ConnectionStrings:Firebird no appsettings.json.",
                dbPath);
    }

    public IDbConnection CreateConnection() => new FbConnection(_connectionString);

    private static string ExtractDatabasePath(string cs)
    {
        var idx = cs.IndexOf("Database=", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return string.Empty;
        var start = idx + 9;
        var end = cs.IndexOf(';', start);
        return end > 0 ? cs[start..end] : cs[start..];
    }
}
