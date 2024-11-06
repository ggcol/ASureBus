﻿using ASureBus.Abstractions;
using ASureBus.Abstractions.Configurations;
using ASureBus.Abstractions.Configurations.ConfigObjects;
using ASureBus.Core;
using ASureBus.Core.Caching;
using ASureBus.Core.DI;
using ASureBus.Core.Messaging;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling;
using ASureBus.Services.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ASureBus.Tests.Core.DI;

[TestFixture]
public class MinimalSetupTests
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
    public void UseAsb_WithSettings_RegistersExpectedServices()
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
        _mockHostBuilder.Object.UseAsb<ServiceBusSettings>();

        // Assert
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IAsbCache))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITypesLoader))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IAzureServiceBusService))), Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ISagaBehaviour))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IMessagingContext))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IMessageEmitter))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IHostedService) &&
                d.ImplementationType == typeof(AsbWorker))), Times.Once);
    }

    [Test]
    public void UseAsb_WithServiceBusConfig_RegistersExpectedServices()
    {
        // Arrange
        var serviceBusConfig = new ServiceBusConfig
        {
            ServiceBusConnectionString = "TestConnectionString"
        };

        _mockHostBuilder.Setup(h =>
                h.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
            .Callback((Action<HostBuilderContext, IServiceCollection> configureServices) =>
            {
                configureServices(_hostBuilderContext, _mockServiceCollection.Object);
            });

        // Act
        _mockHostBuilder.Object.UseAsb(serviceBusConfig);

        // Assert
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IAsbCache))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ITypesLoader))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IAzureServiceBusService))), Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(ISagaBehaviour))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IMessagingContext))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d => d.ServiceType == typeof(IMessageEmitter))),
            Times.Once);
        _mockServiceCollection.Verify(
            s => s.Add(It.Is<ServiceDescriptor>(d =>
                d.ServiceType == typeof(IHostedService) &&
                d.ImplementationType == typeof(AsbWorker))), Times.Once);
    }
}

internal class ServiceBusSettings : IConfigureAzureServiceBus
{
    public string? ServiceBusConnectionString { get; set; }
}