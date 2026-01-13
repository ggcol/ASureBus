using ASureBus.Abstractions.Behaviours;

namespace ASureBus.Tests.Abstractions.Behaviours;

[TestFixture]
public class ObservableExpirableTests
{
    [Test]
    public void Constructor_WithExpirationOnly_HasNoStartTime()
    {
        // Arrange & Act
        var observable = new TestObservableExpirable(TimeSpan.FromSeconds(1));

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(observable.HasExpiration, Is.True);
            Assert.That(observable.StartTime, Is.Null);
        }
    }

    [Test]
    public void Constructor_WithExpirationAndStartTime_PreservesStartTime()
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddMinutes(-5);

        // Act
        var observable = new TestObservableExpirable(TimeSpan.FromSeconds(1), startTime);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(observable.HasExpiration, Is.True);
            Assert.That(observable.StartTime, Is.EqualTo(startTime));
        }
    }

    [Test]
    public void Constructor_WithNullExpiration_HasNoExpiration()
    {
        // Arrange & Act
        var observable = new TestObservableExpirable(null);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(observable.HasExpiration, Is.False);
            Assert.That(observable.StartTime, Is.Null);
        }
    }

    [Test]
    public void StartTime_PreservedAcrossInstances_MaintainsSameValue()
    {
        // Arrange
        var originalStartTime = DateTimeOffset.UtcNow.AddMinutes(-10);
        var observable1 = new TestObservableExpirable(TimeSpan.FromSeconds(1), originalStartTime);

        // Act
        var observable2 = new TestObservableExpirable(TimeSpan.FromSeconds(1), observable1.StartTime!.Value);

        // Assert
        Assert.That(observable2.StartTime, Is.EqualTo(originalStartTime));
        Assert.That(observable2.StartTime, Is.EqualTo(observable1.StartTime));
    }

    [Test]
    public void ExpiredEvent_TriggersAfterExpiration_WhenTimerSet()
    {
        // Arrange
        var eventTriggered = false;
        var observable = new TestObservableExpirable(TimeSpan.FromMilliseconds(50));
        observable.OnExpired(() => eventTriggered = true);

        // Act
        Thread.Sleep(100);

        // Assert
        Assert.That(eventTriggered, Is.True);
    }

    [Test]
    public void ExpiredEvent_WithStartTime_StillTriggersAfterExpiration()
    {
        // Arrange
        var eventTriggered = false;
        var startTime = DateTimeOffset.UtcNow.AddSeconds(-30);
        var observable = new TestObservableExpirable(TimeSpan.FromMilliseconds(50), startTime);
        observable.OnExpired(() => eventTriggered = true);

        // Act
        Thread.Sleep(100);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventTriggered, Is.True);
            Assert.That(observable.StartTime, Is.EqualTo(startTime));
        }
    }

    [Test]
    public void StartTime_CalculatesElapsedTime_Correctly()
    {
        // Arrange
        var startTime = DateTimeOffset.UtcNow.AddSeconds(-10);
        var observable = new TestObservableExpirable(TimeSpan.FromMinutes(1), startTime);

        // Act
        var elapsedTime = DateTimeOffset.UtcNow - observable.StartTime!.Value;

        // Assert
        // Should be approximately 10 seconds (with tolerance for execution time)
        Assert.That(elapsedTime.TotalSeconds, Is.GreaterThanOrEqualTo(10));
        Assert.That(elapsedTime.TotalSeconds, Is.LessThan(11));
    }
}

internal class TestObservableExpirable : ObservableExpirable
{
    public TestObservableExpirable(TimeSpan? expiresAfter) : base(expiresAfter)
    {
    }

    public TestObservableExpirable(TimeSpan? expiresAfter, DateTimeOffset startTime)
        : base(expiresAfter, startTime)
    {
    }

    public void OnExpired(Action callback)
    {
        Expired += (_, _) => callback();
    }
}