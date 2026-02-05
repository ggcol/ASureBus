using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services.StorageAccount;
using Moq;

namespace ASureBus.Tests.ASureBus.Services.StorageAccount;

[TestFixture]
public class SagaDataStoragePersistenceServiceTests
{
    private Mock<IAzureDataStorageService> _mockStorage;
    private Mock<IServiceProvider> _mockServiceProvider;
    private SagaDataStoragePersistenceService _service;
    private SagaType _sagaType;
    private Guid _correlationId;

    [SetUp]
    public void SetUp()
    {
        _mockStorage = new Mock<IAzureDataStorageService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _service = new SagaDataStoragePersistenceService(_mockStorage.Object, _mockServiceProvider.Object);
        _sagaType = new SagaType
        {
            Type = typeof(object),
            SagaDataType = typeof(object)
        };
        _correlationId = Guid.NewGuid();
    }

    [Test]
    public async Task Get_WhenCalled_ReturnsExpectedResult()
    {
        // Arrange
        var expectedResult = new object();
        _mockStorage.Setup(s => s.Get(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Type>(),
                It.IsAny<SagaConverter>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.Get(_sagaType, _correlationId);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task Save_WhenCalled_CallsStorageSave()
    {
        // Arrange
        var item = new object();

        // Act
        await _service.Save(item, _sagaType, _correlationId);

        // Assert
        _mockStorage.Verify(s => s.Save(
            item,
            It.IsAny<string>(),
            It.IsAny<string>(),
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Delete_WhenCalled_CallsStorageDelete()
    {
        // Act
        await _service.Delete(_sagaType, _correlationId);

        // Assert
        _mockStorage.Verify(s => s.Delete(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}