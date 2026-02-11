using ASureBus.Abstractions;
using ASureBus.IntegrationTests.TestConfigurations;

namespace ASureBus.IntegrationTests._04_HeavyProperties;

public class HeavyPropertiesTests : WithAsbHostCheckServiceAndHeavies
{
    private const double PROCESSING_TIME_IN_MS = 500;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        RunHost();
    }

    [SetUp]
    public void Setup()
    {
        CheckService.Reset();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await StopHost().ConfigureAwait(false);
        CleanHeaviesContainer();
    }

    [Test]
    public async Task HeavyProperties_WithBasicType_SuccessfullyOffloadProp()
    {
        // Arrange
        const string anything = "This is a heavy property value";
        var message = new HeavyPropertiesTestsBasicTypeCommand(new Heavy<string>(anything), anything);

        // Act
        await Context.Send(message).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(PROCESSING_TIME_IN_MS)).ConfigureAwait(false);

        // Assert
        Assert.That(CheckService.Acknowledged, Is.True);
    }

    [Test]
    public async Task HeavyProperties_WithCustomType_SuccessfullyOffloadProp()
    {
        // Arrange
        var anything = new ACustomType("value1", 42, true);
        var message = new HeavyPropertiesTestsCustomTypeCommand(new Heavy<ACustomType>(anything), anything);

        // Act
        await Context.Send(message).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(PROCESSING_TIME_IN_MS)).ConfigureAwait(false);

        // Assert
        Assert.That(CheckService.Acknowledged, Is.True);
    }

    [Ignore("TODO works locally but fails in CI, need to investigate")]
    [Test]
    public async Task HeavyProperties_WithAnonymousType_SuccessfullyOffloadProp()
    {
        // Arrange
        var anything = new { Prop1 = "value1", Prop2 = 42, Prop3 = true };
        var message = new HeavyPropertiesTestsAnonymousTypeCommand(new Heavy<object>(anything), anything);

        // Act
        await Context.Send(message).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(PROCESSING_TIME_IN_MS)).ConfigureAwait(false);

        // Assert
        Assert.That(CheckService.Acknowledged, Is.True);
    }
}

public record HeavyPropertiesTestsBasicTypeCommand(Heavy<string> OffloadedProperty, string OnBoardProperty)
    : IAmACommand;

public class ACustomType(string prop1, int prop2, bool prop3)
{
    public string Prop1 { get; init; } = prop1;
    public int Prop2 { get; init; } = prop2;
    public bool Prop3 { get; init; } = prop3;

    public override bool Equals(object? obj)
    {
        if (obj is not ACustomType other) return false;

        return GetHashCode() == other.GetHashCode();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Prop1, Prop2, Prop3);
    }
}

public record HeavyPropertiesTestsCustomTypeCommand(Heavy<ACustomType> OffloadedProperty, ACustomType OnBoardProperty)
    : IAmACommand;

public record HeavyPropertiesTestsAnonymousTypeCommand(Heavy<object> OffloadedProperty, object OnBoardProperty)
    : IAmACommand;

public class HeavyPropertiesTestsBasicTypeCommandHandler(CheckService checkService)
    : IHandleMessage<HeavyPropertiesTestsBasicTypeCommand>
{
    public Task Handle(HeavyPropertiesTestsBasicTypeCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        if (message.OnBoardProperty.Equals(message.OffloadedProperty.Value))
        {
            checkService.Acknowledge();
        }

        checkService.IncrementProcessedMessageCount();
        return Task.CompletedTask;
    }
}

public class HeavyPropertiesTestsCustomTypeCommandHandler(CheckService checkService)
    : IHandleMessage<HeavyPropertiesTestsCustomTypeCommand>
{
    public Task Handle(HeavyPropertiesTestsCustomTypeCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        if (message.OnBoardProperty.Equals(message.OffloadedProperty.Value))
        {
            checkService.Acknowledge();
        }

        checkService.IncrementProcessedMessageCount();
        return Task.CompletedTask;
    }
}

public class HeavyPropertiesTestsAnonymousTypeCommandHandler(CheckService checkService)
    : IHandleMessage<HeavyPropertiesTestsAnonymousTypeCommand>
{
    public Task Handle(HeavyPropertiesTestsAnonymousTypeCommand message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        var offloadType = message.OffloadedProperty.Value?.GetType();
        var offloadProps = offloadType?.GetProperties();

        var onBoardType = message.OnBoardProperty.GetType();
        var onBoardProps = onBoardType.GetProperties();

        var samePropsCount = offloadProps?.Length == onBoardProps.Length;

        var samePropsNames = offloadProps?
            .Select(p => p.Name)
            .OrderBy(n => n)
            .SequenceEqual(onBoardProps.Select(p => p.Name).OrderBy(n => n));

        if (samePropsCount && samePropsNames.HasValue && samePropsNames.Value)
        {
            checkService.Acknowledge();
        }

        checkService.IncrementProcessedMessageCount();
        return Task.CompletedTask;
    }
}