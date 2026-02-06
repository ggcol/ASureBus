using ASureBus.ConfigurationObjects;
using ASureBus.ConfigurationObjects.Config;
using Azure.Messaging.ServiceBus;

namespace ASureBus.Tests.ASureBus.ConfigurationObjects;

[TestFixture]
public class InternalServiceBusConfigTests
{
    [Test]
    public void Constructor_ShouldInitializeProperties_FromConfig()
    {
        // Arrange
        var config = new ServiceBusConfig
        {
            ConnectionString = "TestConnectionString",
            TransportType = "AmqpWebSockets",
            MaxRetries = 5,
            DelayInSeconds = 2,
            MaxDelayInSeconds = 30,
            TryTimeoutInSeconds = 45,
            ServiceBusRetryMode = "Exponential",
        };

        // Act
        var internalConfig = new InternalServiceBusConfig(config);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(internalConfig.ConnectionString,
                Is.EqualTo(config.ConnectionString));
            Assert.That(internalConfig.ClientOptions.TransportType,
                Is.EqualTo(ServiceBusTransportType.AmqpWebSockets));
            Assert.That(internalConfig.ClientOptions.RetryOptions.Mode,
                Is.EqualTo(ServiceBusRetryMode.Exponential));
            Assert.That(internalConfig.ClientOptions.RetryOptions.MaxRetries, Is.EqualTo(5));
            Assert.That(internalConfig.ClientOptions.RetryOptions.Delay,
                Is.EqualTo(TimeSpan.FromSeconds(2)));
            Assert.That(internalConfig.ClientOptions.RetryOptions.MaxDelay,
                Is.EqualTo(TimeSpan.FromSeconds(30)));
            Assert.That(internalConfig.ClientOptions.RetryOptions.TryTimeout,
                Is.EqualTo(TimeSpan.FromSeconds(45)));
        });
    }

    [Test]
    public void Constructor_ShouldInitializeProperties_FromDefaults()
    {
        // Arrange
        var config = new ServiceBusConfig
        {
            ConnectionString = "TestConnectionString"
            // Other properties are not set, so defaults should be used
        };

        // Act
        var internalConfig = new InternalServiceBusConfig(config);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(internalConfig.ConnectionString,
                Is.EqualTo(config.ConnectionString));
            Assert.That(internalConfig.ClientOptions.TransportType,
                Is.EqualTo(Defaults.ServiceBus.CLIENT_OPTIONS.TransportType));
            Assert.That(internalConfig.ClientOptions.RetryOptions.Mode,
                Is.EqualTo(Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.Mode));
            Assert.That(internalConfig.ClientOptions.RetryOptions.MaxRetries,
                Is.EqualTo(Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.MaxRetries));
            Assert.That(internalConfig.ClientOptions.RetryOptions.Delay,
                Is.EqualTo(Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.Delay));
            Assert.That(internalConfig.ClientOptions.RetryOptions.MaxDelay,
                Is.EqualTo(Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.MaxDelay));
            Assert.That(internalConfig.ClientOptions.RetryOptions.TryTimeout,
                Is.EqualTo(Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.TryTimeout));
        });
    }
}