using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.ConfigurationObjects.Config;
using ASureBus.Core;
using ASureBus.Services.StorageAccount;
using Moq;

namespace ASureBus.Tests.ASureBus.Accessories.Heavy;

[TestFixture]
public class HeavyIoTests
{
    private Mock<IAzureDataStorageService> _storageMock;

    [SetUp]
    public void SetUp()
    {
        _storageMock = new Mock<IAzureDataStorageService>();

        AsbConfiguration.HeavyProps = new HeavyPropertiesConfig
        {
            ConnectionString = "UseDevelopmentStorage=true",
            Container = "test-container"
        };

        HeavyIo.ConfigureStorage(_storageMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        AsbConfiguration.HeavyProps = null;
        HeavyIo.ConfigureStorage(null);
    }

    [Test]
    public async Task Unload_ReturnsEmptyListWhenNoHeavyProperties()
    {
        // Arrange
        var message = new Mock<IAmAMessage>().Object;

        // Act
        var result = await HeavyIo.Unload(message, Guid.NewGuid());

        // Assert
        Assert.That(result, Is.Empty);
    }

    private class AFakeMessage : IAmAMessage
    {
        public Heavy<string> AStringProp { get; set; }
    }

    [Test]
    public async Task Unload_SavesHeavyPropertiesAndReturnsReferences()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new AFakeMessage
        {
            AStringProp = new Heavy<string>("AString")
        };

        var expectedHeavyRef = new HeavyReference
        {
            PropertyName = nameof(AFakeMessage.AStringProp),
            Ref = message.AStringProp.Ref
        };

        _storageMock
            .Setup(s => s.Save(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await HeavyIo.Unload(message, messageId);

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result[0].PropertyName, Is.EqualTo(expectedHeavyRef.PropertyName));
            Assert.That(result[0].Ref, Is.EqualTo(expectedHeavyRef.Ref));
        });
    }

    [Test]
    public async Task Unload_SetsHeavyPropertyToNullAfterSaving()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new AFakeMessage
        {
            AStringProp = new Heavy<string>("AString")
        };

        _storageMock
            .Setup(s => s.Save(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), false,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await HeavyIo.Unload(message, messageId);

        // Assert
        var heavyProp = message.GetType().GetProperty(nameof(AFakeMessage.AStringProp));
        var value = heavyProp.GetValue(message);

        Assert.That(value, Is.Null);
    }

    [Test]
    public async Task Load_SetsHeavyPropertiesFromStorage()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var heavyRef = new HeavyReference
        {
            PropertyName = nameof(AFakeMessage.AStringProp),
            Ref = Guid.NewGuid()
        };

        var heavies = new List<HeavyReference> { heavyRef };
        var message = new AFakeMessage();
        var expectedHeavy = new Heavy<string>("AString");

        _storageMock
            .Setup(s => s.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Type>(), null,
                CancellationToken.None))
            .ReturnsAsync(expectedHeavy);

        // Act
        await HeavyIo.Load(message, heavies, messageId);

        // Assert
        var heavyProp = message.GetType().GetProperty(nameof(AFakeMessage.AStringProp));
        var value = heavyProp.GetValue(message) as Heavy<string>;

        Assert.Multiple(() =>
        {
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Value, Is.EqualTo(expectedHeavy.Value));
        });
    }

    [Test]
    public async Task Delete_CallsStorageDelete()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var heavyRef = new HeavyReference
        {
            PropertyName = "AStringProp",
            Ref = Guid.NewGuid()
        };

        _storageMock
            .Setup(s =>
                s.Delete(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await HeavyIo.Delete(messageId, heavyRef);

        // Assert
        _storageMock.Verify(
            s => s.Delete(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}