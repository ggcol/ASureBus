using ASureBus.ConfigurationObjects.Exceptions;

namespace ASureBus.Tests.ASureBus.ConfigurationObjects.Exceptions;

[TestFixture]
public class ConfigurationNullExceptionTests
{
    [Test]
    public void Constructor_ShouldSetMessageCorrectly()
    {
        // Arrange
        const string configName = "TestConfig";

        // Act
        var exception = new ConfigurationNullException(configName);

        // Assert
        Assert.That(exception.Message,
            Is.EqualTo($"Configuration object '{configName}' cannot be null."));
    }

    [Test]
    public void Constructor_ShouldSetConfigNameCorrectly()
    {
        // Arrange
        const string configName = "AnotherConfig";

        // Act
        var exception = new ConfigurationNullException(configName);

        // Assert
        Assert.That(exception.Message,
            Is.EqualTo($"Configuration object '{configName}' cannot be null."));
    }
}