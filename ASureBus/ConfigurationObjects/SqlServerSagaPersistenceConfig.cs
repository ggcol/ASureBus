using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    private string? _schema;
    public required string ConnectionString { get; set; }

    public string? Schema
    {
        get => _schema ?? Defaults.SqlServerSagaPersistence.SCHEMA; 
        set => _schema = value;
    }
}

public class SqlServerSagaPersistenceOptions : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
    
    public string? Schema { get; set; } = Defaults.SqlServerSagaPersistence.SCHEMA;
}