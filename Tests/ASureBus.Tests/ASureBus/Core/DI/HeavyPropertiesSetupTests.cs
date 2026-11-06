using System.Reflection;
using ASureBus.Abstractions.Configurations;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.Core.DI;
using ASureBus.IO.Heavies;
using ASureBus.IO.SagaPersistence;
using ASureBus.IO.StorageAccount;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.DI;

[TestFixture]
public class HeavyPropertiesSetupTests
{
    private const string HEAVIES_ACCOUNT = "HeaviesConnectionString";
    private const string SAGAS_ACCOUNT = "SagasConnectionString";

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
        AsbConfiguration.HeavyProps = null;
        AsbConfiguration.DataStorageSagaPersistence = null;
        _hostBuilderContext = null!;
    }

    [Test]
    public void UseHeavyProps_WithSettings_RegistersExpectedServices()
    {
        // Arrange
        _mockConfiguration
            .Setup(c => c.GetSection(It.IsAny<string>()))
            .Returns(new Mock<IConfigurationSection>().Object);

        _mockHostBuilder
            .Setup(h => h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseHeavyProps<HeavyPropertiesSettings>();

        // Assert
        Assert.That(AsbConfiguration.HeavyProps, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(AsbConfiguration.HeavyProps.ConnectionString,
                Is.EqualTo("TestConnectionString"));
            Assert.That(AsbConfiguration.HeavyProps.Container,
                Is.EqualTo("TestContainer"));
        }
    }

    [Test]
    public void UseHeavyProps_WithHeavyPropertiesConfig_RegistersExpectedServices()
    {
        // Arrange
        var heavyPropsConfig = new HeavyPropertiesConfig
        {
            ConnectionString = "TestConnectionString",
            Container = "TestContainer"
        };
        
        _mockHostBuilder
            .Setup(h => h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseHeavyProps(heavyPropsConfig);

        // Assert
        Assert.That(AsbConfiguration.HeavyProps, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(AsbConfiguration.HeavyProps.ConnectionString,
                Is.EqualTo("TestConnectionString"));
            Assert.That(AsbConfiguration.HeavyProps.Container,
                Is.EqualTo("TestContainer"));
        }
    }

    [Test]
    public void UseHeavyProps_WithSettings_BuildsRealHost()
    {
        // Arrange
        var hostBuilder = new HostBuilder().UseHeavyProps<HeavyPropertiesSettings>();

        // Act
        using var host = hostBuilder.Build();

        // Assert
        Assert.That(host.Services.GetService<IHeavyIO>(), Is.InstanceOf<HeavyIO>());
    }

    [Test]
    public void UseHeavyProps_BeforeDataStorageSagaPersistence_EachFeatureUsesItsOwnAccount()
    {
        // Arrange
        var hostBuilder = new HostBuilder()
            .UseHeavyProps(MakeHeavyPropertiesConfig())
            .UseDataStorageSagaPersistence(MakeDataStorageSagaPersistenceConfig());

        // Act
        using var host = hostBuilder.Build();

        // Assert
        AssertEachFeatureUsesItsOwnAccount(host);
    }

    [Test]
    public void UseHeavyProps_AfterDataStorageSagaPersistence_EachFeatureUsesItsOwnAccount()
    {
        // Arrange
        var hostBuilder = new HostBuilder()
            .UseDataStorageSagaPersistence(MakeDataStorageSagaPersistenceConfig())
            .UseHeavyProps(MakeHeavyPropertiesConfig());

        // Act
        using var host = hostBuilder.Build();

        // Assert
        AssertEachFeatureUsesItsOwnAccount(host);
    }

    private static HeavyPropertiesConfig MakeHeavyPropertiesConfig()
    {
        return new HeavyPropertiesConfig
        {
            ConnectionString = HEAVIES_ACCOUNT,
            Container = "TestContainer"
        };
    }

    private static DataStorageSagaPersistenceConfig MakeDataStorageSagaPersistenceConfig()
    {
        return new DataStorageSagaPersistenceConfig
        {
            ConnectionString = SAGAS_ACCOUNT,
            Container = "TestContainer"
        };
    }

    private static void AssertEachFeatureUsesItsOwnAccount(IHost host)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetAccount(host.Services.GetRequiredService<IHeavyIO>()),
                Is.EqualTo(HEAVIES_ACCOUNT));
            Assert.That(GetAccount(host.Services.GetRequiredService<ISagaPersistenceService>()),
                Is.EqualTo(SAGAS_ACCOUNT));
        }
    }

    private static string GetAccount(object storageConsumer)
    {
        return GetPrivateField<string>(GetPrivateField<IAzureDataStorageService>(storageConsumer));
    }

    private static T GetPrivateField<T>(object target)
    {
        return (T)target.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Single(field => field.FieldType == typeof(T))
            .GetValue(target)!;
    }
}

internal class HeavyPropertiesSettings : IConfigureHeavyProperties
{
    public string? ConnectionString { get; set; } = "TestConnectionString";
    public string? Container { get; set; } = "TestContainer";
}