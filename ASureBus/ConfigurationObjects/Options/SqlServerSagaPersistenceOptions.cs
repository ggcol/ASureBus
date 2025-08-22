using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects.Options;

public class SqlServerSagaPersistenceOptions : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
    
    public string? Schema { get; set; } = Defaults.SqlServerSagaPersistence.SCHEMA;
}