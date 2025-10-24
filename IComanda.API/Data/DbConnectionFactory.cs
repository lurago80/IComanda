using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace IComanda.API.Data;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Firebird")
            ?? throw new ArgumentNullException("Connection string 'Firebird' não encontrada");
    }

    public IDbConnection CreateConnection()
    {
        return new FbConnection(_connectionString);
    }
}
