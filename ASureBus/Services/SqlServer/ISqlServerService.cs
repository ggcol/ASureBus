namespace ASureBus.Services.SqlServer;

internal interface ISqlServerService
{
    internal Task Save(string serializedItem, string tableName,
        Guid correlationId, CancellationToken cancellationToken = default);

    internal Task<string?> Get(string tableName, Guid correlationId,
        CancellationToken cancellationToken = default);

    internal Task Delete(string tableName, Guid correlationId,
        CancellationToken cancellationToken = default);
}