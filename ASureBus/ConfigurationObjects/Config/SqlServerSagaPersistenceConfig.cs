using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects.Config;

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