using Microsoft.Data.SqlClient;

namespace ASureBus.IO.SqlServer.DbConnection;

public class SqlServerConnectionFactory : IDbConnectionFactory
{
    public System.Data.Common.DbConnection CreateConnection()
    {
        return new SqlConnection(AsbConfiguration.SqlServerSagaPersistence!.ConnectionString);
    }
}