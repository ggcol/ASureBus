using ASureBus.Abstractions.Options.Messaging;

namespace ASureBus.Tests.Abstractions.Options.Messaging;

[TestFixture]
public class PublishOptionsTests
{
    [Test]
    public void PublishOptions_ShouldInheritFromEmitOptions()
    {
        // Act
        var options = new PublishOptions();

        // Assert
        Assert.That(options, Is.InstanceOf<EmitOptions>());
    }
}