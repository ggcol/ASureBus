using ASureBus.Utils;
using Microsoft.Extensions.Configuration;

namespace ASureBus.Tests.ASureBus.Utils;

[TestFixture]
public class ConfigProviderTests
{
    private IConfiguration _configuration;

    [SetUp]
    public void SetUp()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "TestSettings:Setting1", "Value1" },
            { "TestSettings:Setting2", "Value2" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }
    
    [TearDown]
    public void TearDown()
    {
        _configuration = null!;
    }

    [Test]
    public void LoadSettings_WhenCalled_ReturnsConfiguredSettings()
    {
        // Act
        var result = ConfigProvider.LoadSettings<TestSettings>(_configuration);

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(result.Setting1, Is.EqualTo("Value1"));
            Assert.That(result.Setting2, Is.EqualTo("Value2"));
        });
    }

    private class TestSettings
    {
        public string Setting1 { get; set; }
        public string Setting2 { get; set; }
    }
}