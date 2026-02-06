using ASureBus.Abstractions.Options.Messaging;

namespace ASureBus.Tests.Abstractions.Options.Messaging;

[TestFixture]
public class EmitOptionsTests
{
    [Test]
    public void IsScheduled_ShouldReturnTrue_WhenScheduledTimeIsNotNull()
    {
        // Arrange
        var options = new TestEmitOptions
        {
            ScheduledTime = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        // Act
        var isScheduled = options.IsScheduled;

        // Assert
        Assert.That(isScheduled, Is.True);
    }

    [Test]
    public void IsScheduled_ShouldReturnFalse_WhenScheduledTimeIsNull()
    {
        // Arrange
        var options = new TestEmitOptions();

        // Act
        var isScheduled = options.IsScheduled;

        // Assert
        Assert.That(isScheduled, Is.False);
    }

    [Test]
    public void Delay_ShouldSetScheduledTime()
    {
        // Arrange
        var options = new TestEmitOptions();
        var delay = TimeSpan.FromMinutes(10);
        var expectedScheduledTime = DateTimeOffset.UtcNow.Add(delay);

        // Act
        options.Delay = delay;

        // Assert
        Assert.That(options.ScheduledTime,
            Is.EqualTo(expectedScheduledTime).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void Delay_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var options = new TestEmitOptions();
        var delay = TimeSpan.FromMinutes(10);
        options.Delay = delay;

        // Act
        var result = options.Delay;

        // Assert
        Assert.That(result, Is.EqualTo(delay).Within(TimeSpan.FromSeconds(1)));
    }

    private class TestEmitOptions : EmitOptions
    {
    }
}