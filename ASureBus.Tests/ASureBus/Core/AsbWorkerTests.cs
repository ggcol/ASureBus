using Moq;
using ASureBus.Core;
using ASureBus.Core.Caching;
using ASureBus.Core.Messaging;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.ServiceBus;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ASureBus.Tests.ASureBus.Core;

[TestFixture]
public class AsbWorkerTests
{
    [Test]
    public async Task StartAsync_StartsAllProcessors_ForHandlers()
    {
        // Arrange
        var mockProcessor = new Mock<ServiceBusProcessor>();
        var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
        var mockTypesLoader = new Mock<ITypesLoader>();

        var anHandler = new HandlerType()
        {
            MessageType = new MessageType()
            {
                IsCommand = true,
                Type = typeof(object)
            },
            Type = typeof(object)
        };

        var handlerSet = new[]
        {
            anHandler
        }.ToHashSet();

        mockTypesLoader.Setup(t => t.Handlers).Returns(handlerSet);
        mockTypesLoader.Setup(t => t.Sagas).Returns(Array.Empty<SagaType>().ToHashSet());

        mockAzureServiceBusService
            .Setup(a => a.GetProcessor(handlerSet.FirstOrDefault()!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockProcessor.Object);

        var worker = new AsbWorker(
            Mock.Of<IHostApplicationLifetime>(),
            Mock.Of<IServiceProvider>(),
            mockAzureServiceBusService.Object,
            Mock.Of<IMessageEmitter>(),
            mockTypesLoader.Object,
            Mock.Of<IAsbCache>(),
            Mock.Of<ISagaBehaviour>(),
            Mock.Of<ILogger<AsbWorker>>(),
            Mock.Of<ISagaIO>());

        // Act
        await worker.StartAsync(CancellationToken.None);

        // Assert
        mockProcessor.Verify(p => p.StartProcessingAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task StartAsync_StartsAllProcessors_ForSagas()
    {
        // Arrange
        var mockProcessor = new Mock<ServiceBusProcessor>();
        var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
        var mockTypesLoader = new Mock<ITypesLoader>();

        var sagaHandlerType = new SagaHandlerType()
        {
            IsInitMessageHandler = true,
            MessageType = new MessageType()
            {
                IsCommand = true,
                Type = typeof(object)
            }
        };

        var aSaga = new SagaType()
        {
            Type = typeof(object),
            Listeners = new[]
            {
                sagaHandlerType
            }.ToHashSet()
        };

        var sagaSet = new[]
        {
            aSaga
        }.ToHashSet();

        mockTypesLoader.Setup(t => t.Handlers).Returns(Array.Empty<HandlerType>().ToHashSet());
        mockTypesLoader.Setup(t => t.Sagas).Returns(sagaSet);

        mockAzureServiceBusService
            .Setup(a => a.GetProcessor(sagaHandlerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockProcessor.Object);

        var worker = new AsbWorker(
            Mock.Of<IHostApplicationLifetime>(),
            Mock.Of<IServiceProvider>(),
            mockAzureServiceBusService.Object,
            Mock.Of<IMessageEmitter>(),
            mockTypesLoader.Object,
            Mock.Of<IAsbCache>(),
            Mock.Of<ISagaBehaviour>(),
            Mock.Of<ILogger<AsbWorker>>(),
            Mock.Of<ISagaIO>());

        // Act
        await worker.StartAsync(CancellationToken.None);

        // Assert
        mockProcessor.Verify(p => p.StartProcessingAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task StopAsync_StopsAndDisposesAllProcessors_ForHandlers()
    {
        // Arrange
        var mockProcessor = new Mock<ServiceBusProcessor>();
        var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
        var mockTypesLoader = new Mock<ITypesLoader>();

        var anHandler = new HandlerType()
        {
            MessageType = new MessageType()
            {
                IsCommand = true,
                Type = typeof(object)
            },
            Type = typeof(object)
        };

        var handlerSet = new[]
        {
            anHandler
        }.ToHashSet();

        mockTypesLoader.Setup(t => t.Handlers).Returns(handlerSet);
        mockTypesLoader.Setup(t => t.Sagas).Returns(Array.Empty<SagaType>().ToHashSet());

        mockAzureServiceBusService
            .Setup(a => a.GetProcessor(handlerSet.FirstOrDefault()!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockProcessor.Object);

        var worker = new AsbWorker(
            Mock.Of<IHostApplicationLifetime>(),
            Mock.Of<IServiceProvider>(),
            mockAzureServiceBusService.Object,
            Mock.Of<IMessageEmitter>(),
            mockTypesLoader.Object,
            Mock.Of<IAsbCache>(),
            Mock.Of<ISagaBehaviour>(),
            Mock.Of<ILogger<AsbWorker>>(),
            Mock.Of<ISagaIO>());

        // Act
        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        // Assert
        mockProcessor.Verify(p => p.StopProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StopAsync_StopsAndDisposesAllProcessors_ForSagas()
    {
        // Arrange
        var mockProcessor = new Mock<ServiceBusProcessor>();
        var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
        var mockTypesLoader = new Mock<ITypesLoader>();

        var sagaHandlerType = new SagaHandlerType()
        {
            IsInitMessageHandler = true,
            MessageType = new MessageType()
            {
                IsCommand = true,
                Type = typeof(object)
            }
        };

        var aSaga = new SagaType()
        {
            Type = typeof(object),
            Listeners = new[]
            {
                sagaHandlerType
            }.ToHashSet()
        };

        var sagaSet = new[]
        {
            aSaga
        }.ToHashSet();

        mockTypesLoader.Setup(t => t.Handlers).Returns(Array.Empty<HandlerType>().ToHashSet());
        mockTypesLoader.Setup(t => t.Sagas).Returns(sagaSet);

        mockAzureServiceBusService
            .Setup(a => a.GetProcessor(sagaHandlerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockProcessor.Object);

        var worker = new AsbWorker(
            Mock.Of<IHostApplicationLifetime>(),
            Mock.Of<IServiceProvider>(),
            mockAzureServiceBusService.Object,
            Mock.Of<IMessageEmitter>(),
            mockTypesLoader.Object,
            Mock.Of<IAsbCache>(),
            Mock.Of<ISagaBehaviour>(),
            Mock.Of<ILogger<AsbWorker>>(),
            Mock.Of<ISagaIO>());

        // Act
        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        // Assert
        mockProcessor.Verify(p => p.StopProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}