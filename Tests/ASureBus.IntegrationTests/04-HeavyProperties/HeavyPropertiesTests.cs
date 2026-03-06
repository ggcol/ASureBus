using ASureBus.Abstractions;
using ASureBus.IntegrationTests.TestConfigurations;

namespace ASureBus.IntegrationTests._04_HeavyProperties;

public class HeavyPropertiesTests : WithAsbHostCheckServiceAndHeavies
{
    private const double PROCESSING_TIME_IN_MS = 2000;

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