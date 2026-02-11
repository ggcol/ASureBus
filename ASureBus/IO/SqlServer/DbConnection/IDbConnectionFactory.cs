namespace ASureBus.IO.SqlServer.DbConnection;

public interface IDbConnectionFactory
{
    System.Data.Common.DbConnection CreateConnection();
}