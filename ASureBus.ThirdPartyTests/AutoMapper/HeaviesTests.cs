using ASureBus.Abstractions;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace ASureBus.ThirdPartyTests.AutoMapper;

public class Tests
{
    private readonly IMapper _mapper = AutoMapperConfig.Initialize();

    [Test]
    public void MappingHeavies_1to1_ShouldBeEqual()
    {
        //Arrange
        var veryHeavyString = new Heavy<string>("This is a very heavy string");

        var source = new SourceMessageWithHeavy
        {
            HeavyProperty = veryHeavyString
        };

        //Act
        var destination = _mapper.Map<DestinationMessageWithHeavy>(source);

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(destination.HeavyProperty.Value, Is.EqualTo(source.HeavyProperty.Value));
            Assert.That(destination.HeavyProperty.Ref, Is.EqualTo(source.HeavyProperty.Ref));
        });
    }
}

public class SourceMessageWithHeavy
{
    public required Heavy<string> HeavyProperty { get; set; }
}

public class DestinationMessageWithHeavy
{
    public required Heavy<string> HeavyProperty { get; set; }
}

public static class AutoMapperConfig
{
    public static IMapper Initialize()
    {
        var coreAssembly = typeof(AutoMapperConfig).Assembly;

        var logger = new Mock<ILogger>();

        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory
            .Setup(l => l.CreateLogger(It.IsAny<string>()))
            .Returns(logger.Object);

        var license = File.ReadAllText(Path.Combine(".", "AutoMapper", "License.txt"));
        
        return new MapperConfiguration(config =>
        {
            config.AddMaps(coreAssembly);
            config.LicenseKey = license;
        }, loggerFactory.Object).CreateMapper();
    }
}

public class ProfileWithHeavy : Profile
{
    public ProfileWithHeavy()
    {
        CreateMap<SourceMessageWithHeavy, DestinationMessageWithHeavy>()
            .ForMember(dest => dest.HeavyProperty, opt => opt.MapFrom(src => src.HeavyProperty));
    }
}