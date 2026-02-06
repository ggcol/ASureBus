using ASureBus.Core.Exceptions;

namespace ASureBus.Tests.ASureBus.Core.Exceptions;

[TestFixture]
public class AsbExceptionTests
{
    [Test]
    public void AsbException_ShouldInitializeProperties()
    {
        // Arrange
        var originalException = new Exception("Original exception");
        var correlationId = Guid.NewGuid();

        // Act
        var asbException = new AsbException
        {
            OriginalException = originalException, 
            CorrelationId = correlationId
        };

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(asbException.OriginalException, Is.EqualTo(originalException));
            Assert.That(asbException.CorrelationId, Is.EqualTo(correlationId));
        });
    }

    [Test]
    public void AsbException_ShouldInheritFromException()
    {
        // Act
        var asbException = new AsbException
        {
            OriginalException = new Exception(),
            CorrelationId = Guid.NewGuid()
        };

        // Assert
        Assert.That(asbException, Is.InstanceOf<Exception>());
    }
}