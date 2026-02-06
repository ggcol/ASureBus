using Microsoft.Data.SqlClient;

namespace ASureBus.IO.SqlServer.DbConnection;

public class InMemorySqlServerConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString =
        AsbConfiguration.SqlServerSagaPersistence!.ConnectionString;

    public System.Data.Common.DbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}