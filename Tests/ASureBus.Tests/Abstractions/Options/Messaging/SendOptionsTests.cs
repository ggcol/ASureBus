using ASureBus.Abstractions.Options.Messaging;

namespace ASureBus.Tests.Abstractions.Options.Messaging;

[TestFixture]
public class SendOptionsTests
{
    [Test]
    public void SendOptions_ShouldInheritFromEmitOptions()
    {
        // Act
        var options = new SendOptions();

        // Assert
        Assert.That(options, Is.InstanceOf<EmitOptions>());
    }
}