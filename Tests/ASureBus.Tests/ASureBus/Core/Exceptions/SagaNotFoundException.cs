using ASureBus.Core.Exceptions;

namespace ASureBus.Tests.ASureBus.Core.Exceptions;

[TestFixture]
public class SagaNotFoundExceptionTests
{
    [Test]
    public void SagaNotFoundException_ShouldInitializeProperties()
    {
        // Arrange
        var sagaType = typeof(FakeSaga);
        var correlationId = Guid.NewGuid();
        var expectedMessage =
            $"Saga of type {sagaType.Name} with correlation id {correlationId} not found";

        // Act
        var exception = new SagaNotFoundException(sagaType, correlationId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo(expectedMessage));
            Assert.That(exception.InnerException, Is.Null);
        });
    }

    private class FakeSaga;
}