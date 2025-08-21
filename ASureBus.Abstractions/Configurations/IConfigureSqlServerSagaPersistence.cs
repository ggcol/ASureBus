namespace ASureBus.Abstractions.Configurations;

public interface IConfigureSqlServerSagaPersistence
{
    public string ConnectionString { get; set; }
    public string? Schema { get; set; }
}