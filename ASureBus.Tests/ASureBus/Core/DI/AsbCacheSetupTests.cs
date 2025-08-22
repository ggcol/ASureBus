using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.Core.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.DI;

[TestFixture]
public class AsbCacheSetupTests
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
    public void ConfigureAsbCache_WithSettings_RegistersExpectedServices()
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
        _mockHostBuilder.Object.ConfigureAsbCache<AsbCacheSettings>();

        // Assert
        // Add assertions to verify the expected services are registered
    }

    [Test]
    public void ConfigureAsbCache_WithAsbCacheConfig_RegistersExpectedServices()
    {
        // Arrange
        var asbCacheConfig = new AsbCacheConfig
        {
            Expiration = TimeSpan.FromMinutes(5),
            TopicConfigPrefix = "TestPrefix",
            ServiceBusSenderCachePrefix = "TestSenderPrefix"
        };

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.ConfigureAsbCache(asbCacheConfig);

        // Assert
        // Add assertions to verify the expected services are registered
    }
}

internal class AsbCacheSettings : IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}