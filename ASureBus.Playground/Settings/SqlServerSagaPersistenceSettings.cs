using ASureBus.Abstractions.Configurations;

namespace ASureBus.Playground.Settings;

public class SqlServerSagaPersistenceSettings : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
    public string? Schema { get; set; }
}