using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.Core.DI;
using ASureBus.Services;
using ASureBus.Services.SqlServer;
using ASureBus.Services.StorageAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.DI;

[TestFixture]
public class SagaPersistenceSetupTests
{
    private Mock<IHostBuilder> _mockHostBuilder;
    private Mock<IServiceCollection> _mockServiceCollection;
    private HostBuilderContext _hostBuilderContext;
    private Mock<IConfiguration> _mockConfiguration;

    [SetUp]
    public void SetUp()
    {
        _mockHostBuilder = new Mock<IHostBuilder>();
        _mockServiceCollection = new Mock<IServiceCollection>();
        _mockConfiguration = new Mock<IConfiguration>();
        _hostBuilderContext = new HostBuilderContext(new Dictionary<object, object>())
        {
            Configuration = _mockConfiguration.Object
        };
    }
    
    [TearDown]
    public void TearDown()
    {
        _hostBuilderContext = null!;
    }

    [Test]
    public void UseDataStorageSagaPersistence_WithSettings_RegistersExpectedServices()
    {
        // Arrange
        _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()))
            .Returns(new Mock<IConfigurationSection>().Object);

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseDataStorageSagaPersistence<DataStorageSagaPersistenceSettings>();

        // Assert
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IAzureDataStorageService))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(ISagaPersistenceService))),
            Times.Once);
    }

    [Test]
    public void UseDataStorageSagaPersistence_WithConfig_RegistersExpectedServices()
    {
        // Arrange
        var config = new DataStorageSagaPersistenceConfig
        {
            ConnectionString = "TestConnectionString",
            Container = "TestContainer"
        };

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseDataStorageSagaPersistence(config);

        // Assert
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IAzureDataStorageService))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(ISagaPersistenceService))),
            Times.Once);
    }

    [Test]
    public void UseSqlServerSagaPersistence_WithSettings_RegistersExpectedServices()
    {
        // Arrange
        _mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>()))
            .Returns(new Mock<IConfigurationSection>().Object);

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseSqlServerSagaPersistence<SqlServerSagaPersistenceSettings>();

        // Assert
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ISqlServerService))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(ISagaPersistenceService))),
            Times.Once);
    }

    [Test]
    public void UseSqlServerSagaPersistence_WithConfig_RegistersExpectedServices()
    {
        // Arrange
        var config = new SqlServerSagaPersistenceConfig
        {
            ConnectionString = "TestConnectionString"
        };

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseSqlServerSagaPersistence(config);

        // Assert
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ISqlServerService))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(ISagaPersistenceService))),
            Times.Once);
    }
}

internal class DataStorageSagaPersistenceSettings : IConfigureDataStorageSagaPersistence
{
    public string? ConnectionString { get; set; }
    public string? Container { get; set; }
}

internal class SqlServerSagaPersistenceSettings : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
    public string? Schema { get; set; }
}