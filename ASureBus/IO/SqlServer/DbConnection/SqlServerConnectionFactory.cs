using Microsoft.Data.SqlClient;

namespace ASureBus.IO.SqlServer.DbConnection;

public class SqlServerConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public System.Data.Common.DbConnection CreateConnection()
    {
        return new SqlConnection(connectionString);
    }
}