using System.Data;

namespace IComanda.API.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
