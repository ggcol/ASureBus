using System.Buffers;
using System.Text;
using System.Text.Json;
using ASureBus.Abstractions;
using ASureBus.Core.Sagas;
using Moq;

namespace ASureBus.Tests.ASureBus.Core.Sagas;

[TestFixture]
public class SagaConverterTests
{
    private class ASagaData : SagaData
    {
        public string? Data { get; set; }
    }

    private class ASaga : Saga<ASagaData>
    {
    }

    private SagaConverter _sagaConverter;
    private JsonSerializerOptions _options;
    private Mock<IServiceProvider> _mockServiceProvider;

    [SetUp]
    public void SetUp()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _sagaConverter = new SagaConverter(typeof(ASaga), typeof(ASagaData), _mockServiceProvider.Object);
        _options = new JsonSerializerOptions
        {
            Converters = { _sagaConverter }
        };
    }

    [Test]
    public void CanConvert_ReturnsTrueForAssignableType()
    {
        // Act
        var result = _sagaConverter.CanConvert(typeof(ASaga));

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CanConvert_ReturnsFalseForNonAssignableType()
    {
        // Act
        var result = _sagaConverter.CanConvert(typeof(string));

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Read_DeserializesJsonToObject()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var json = "{\"SagaData\":{\"Data\":\"TestData\"}, \"CorrelationId\":\"" + correlationId + "\"}";
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

        // Act
        var result = _sagaConverter.Read(ref reader, typeof(ASaga), _options);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ASaga>());
            Assert.That(((ASaga)result).SagaData?.Data, Is.EqualTo("TestData"));
        });
    }

    [Test]
    public void Write_SerializesObjectToJson()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var saga = new ASaga
        {
            CorrelationId = correlationId,
            SagaData = new ASagaData { Data = "TestData" }
        };
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new Utf8JsonWriter(buffer);

        // Act
        _sagaConverter.Write(writer, saga, _options);
        writer.Flush();
        var json = Encoding.UTF8.GetString(buffer.WrittenMemory.ToArray());

        // Assert
        var expected = "{\"SagaData\":{\"Data\":\"TestData\"},\"CorrelationId\":\"" + correlationId + "\"}";
        Assert.That(json, Is.EqualTo(expected));
    }
}