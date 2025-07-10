using System.Reflection;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Sagas;

public class SagaIoTests
{
    private ISagaIO _sagaIo;
    
    [SetUp]
    public void Setup()
    {
        var services = new Mock<IServiceProvider>();
        _sagaIo = new SagaIO(services.Object);
    }
    
    [Test]
    public async Task Load_ReturnsNull_WhenSagasAreNotInUse()
    {
        // Act
        var result = await _sagaIo.Load(Guid.NewGuid(), It.IsAny<SagaType>());

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task Load_ReturnsSaga_WhenSagasAreInUse()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        var persistenceService = new Mock<ISagaPersistenceService>();
        
        persistenceService
            .Setup(ps 
                => ps.Get(It.IsAny<SagaType>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        typeof(SagaIO)
            .GetField("_persistenceService", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(_sagaIo, persistenceService.Object);
        typeof(SagaIO)
            .GetField("_isInUse", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(_sagaIo, true);

        // Act
        var result = await _sagaIo.Load(Guid.NewGuid(), It.IsAny<SagaType>());

        // Assert
        Assert.IsNotNull(result);
    }

    [Test]
    public void Unload_DoesNothing_WhenSagasAreNotInUse()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () =>
            await _sagaIo.Unload(new object(), Guid.NewGuid(), It.IsAny<SagaType>()));
    }

    [Test]
    public async Task Unload_SavesSaga_WhenSagasAreInUse()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        var persistenceService = new Mock<ISagaPersistenceService>();

        typeof(SagaIO)
            .GetField("_persistenceService", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(_sagaIo, persistenceService.Object);
        typeof(SagaIO)
            .GetField("_isInUse", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(_sagaIo, true);

        // Act
        await _sagaIo.Unload(new object(), Guid.NewGuid(), It.IsAny<SagaType>());

        // Assert
        persistenceService.Verify(
            ps => ps.Save(It.IsAny<object>(), It.IsAny<SagaType>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Delete_DoesNothing_WhenSagasAreNotInUse()
    {
        // Act & Assert
        Assert.DoesNotThrowAsync(async () =>
            await _sagaIo.Delete(Guid.NewGuid(), It.IsAny<SagaType>()));
    }

    [Test]
    public async Task Delete_RemovesSaga_WhenSagasAreInUse()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        var persistenceService = new Mock<ISagaPersistenceService>();

        typeof(SagaIO)
            .GetField("_persistenceService", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(_sagaIo, persistenceService.Object);
        typeof(SagaIO)
            .GetField("_isInUse", BindingFlags.NonPublic | BindingFlags.Instance)?
            .SetValue(_sagaIo, true);

        // Act
        await _sagaIo.Delete(Guid.NewGuid(), It.IsAny<SagaType>());

        // Assert
        persistenceService.Verify(
            ps => ps.Delete(It.IsAny<SagaType>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}