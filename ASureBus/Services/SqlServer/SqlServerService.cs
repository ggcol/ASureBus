using ASureBus.Core;
using ASureBus.Services.SqlServer.DbConnection;
using Microsoft.Data.SqlClient;

namespace ASureBus.Services.SqlServer;

internal sealed class SqlServerService(IDbConnectionFactory connectionFactory) : ISqlServerService
{
    private readonly string _schema = AsbConfiguration.SqlServerSagaPersistence!.Schema!;
    
    private const string CORR_ID_HEADER = "CorrelationId";
    private const string SAGA_HEADER = "Saga";
    private const string CORR_ID_PARAM = "@" + CORR_ID_HEADER;
    private const string SAGA_PARAM = "@" + SAGA_HEADER;

    public async Task Save(string serializedItem, string tableName, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        if (!await SchemaExists(cancellationToken).ConfigureAwait(false))
        {
            await CreateSchema(cancellationToken).ConfigureAwait(false);
        }

        if (!await TableExists(tableName, cancellationToken).ConfigureAwait(false))
        {
            await CreateTable(tableName, cancellationToken).ConfigureAwait(false);
        }

        var updateQuery =
            $"UPDATE {_schema}.{tableName} SET {SAGA_HEADER} = {SAGA_PARAM} WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}";
        var insertQuery =
            $"INSERT INTO {_schema}.{tableName} ({CORR_ID_HEADER}, {SAGA_HEADER}) VALUES ({CORR_ID_PARAM}, {SAGA_PARAM})";

        var queryParams = new Dictionary<string, object>
        {
            { SAGA_PARAM, serializedItem },
            { CORR_ID_PARAM, correlationId }
        };

        var result = await ExecuteNonQuery(updateQuery, cancellationToken, queryParams)
            .ConfigureAwait(false);

        if (result is not 1)
        {
            result = await ExecuteNonQuery(insertQuery, cancellationToken, queryParams)
                .ConfigureAwait(false);

            if (result is not 1)
            {
                throw new Exception("Failed to insert saga data.");
            }
        }
    }

    public async Task<string?> Get(string tableName, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        if (!await SchemaExists(cancellationToken).ConfigureAwait(false))
        {
            throw new Exception("Schema does not exist.");
        }

        if (!await TableExists(tableName, cancellationToken).ConfigureAwait(false))
        {
            throw new Exception("Table does not exist.");
        }

        var query =
            $"SELECT {SAGA_HEADER} FROM {_schema}.{tableName} WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}";

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        cmd.Parameters.Add(new SqlParameter(CORR_ID_PARAM, correlationId));

        await using var reader =
            await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (!reader.HasRows) return null;

        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        return reader.GetString(0);
    }

    public async Task Delete(string tableName, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        if (!await SchemaExists(cancellationToken).ConfigureAwait(false))
        {
            throw new Exception("Schema does not exist.");
        }

        if (!await TableExists(tableName, cancellationToken).ConfigureAwait(false))
        {
            throw new Exception("Table does not exist.");
        }

        var query = $"DELETE FROM {_schema}.{tableName} WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}";

        var result = await ExecuteNonQuery(query, cancellationToken,
            new Dictionary<string, object>
            {
                { CORR_ID_PARAM, correlationId }
            }).ConfigureAwait(false);

        if (result is not 1)
        {
            throw new Exception("Failed to delete saga data.");
        }
    }

    private async Task<bool> TableExists(string tableName, CancellationToken cancellationToken)
    {
        var query = $"IF OBJECT_ID('{_schema}.{tableName}', 'U') IS NOT NULL SELECT 1 ELSE SELECT 0";

        var result = await ExecuteScalar(query, cancellationToken).ConfigureAwait(false);

        return result is 1;
    }

    private async Task<bool> SchemaExists(CancellationToken cancellationToken)
    {
        var query = $"IF SCHEMA_ID('{_schema}') IS NOT NULL SELECT 1 ELSE SELECT 0";

        var result = await ExecuteScalar(query, cancellationToken).ConfigureAwait(false);

        return result is 1;
    }

    private async Task CreateTable(string tableName, CancellationToken cancellationToken)
    {
        var query =
            $"CREATE TABLE {_schema}.{tableName} ({CORR_ID_HEADER} UNIQUEIDENTIFIER PRIMARY KEY, {SAGA_HEADER} NVARCHAR(MAX))";

        await ExecuteNonQuery(query, cancellationToken).ConfigureAwait(false);
    }

    private async Task CreateSchema(CancellationToken cancellationToken)
    {
        var query = $"CREATE SCHEMA {_schema}";

        await ExecuteNonQuery(query, cancellationToken);
    }

    private async Task<int> ExecuteNonQuery(string query, CancellationToken cancellationToken,
        IDictionary<string, object>? queryParams = null)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        if (queryParams is null)
            return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        foreach (var (key, value) in queryParams)
        {
            cmd.Parameters.Add(new SqlParameter(key, value));
        }

        return await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<object?> ExecuteScalar(string query, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        return await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
    }
}