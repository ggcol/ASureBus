using System.Text;
using System.Text.Json;
using ASureBus.Utils;

namespace ASureBus.Tests.ASureBus.Utils;

[TestFixture]
public class SerializerTests
{
    private class TestObject
    {
        public int Id { get; init; }
        public string? Name { get; init; }
    }

    [Test]
    public void Serialize_ShouldReturnJsonString()
    {
        //Arrange
        var obj = new TestObject { Id = 1, Name = "Test" };

        //Act
        var json = Serializer.Serialize(obj);

        //Assert
        Assert.That(json, Is.EqualTo("{\"Id\":1,\"Name\":\"Test\"}"));
    }

    [Test]
    public void Deserialize_ShouldReturnObject()
    {
        //Arrange
        var json = "{\"Id\":1,\"Name\":\"Test\"}";

        //Act
        var obj = (TestObject)Serializer.Deserialize(json, typeof(TestObject));

        //Assert
        Assert.That(obj, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(obj.Id, Is.EqualTo(1));
            Assert.That(obj.Name, Is.EqualTo("Test"));
        });
    }

    [Test]
    public async Task DeserializeAsync_ShouldReturnObject()
    {
        //Arrange
        var json = "{\"Id\":1,\"Name\":\"Test\"}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        //Act
        var obj = await Serializer.Deserialize<TestObject>(stream);

        //Assert
        Assert.That(obj, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(obj.Id, Is.EqualTo(1));
            Assert.That(obj.Name, Is.EqualTo("Test"));
        });
    }

    [Test]
    public void Deserialize_WithOptions_ShouldReturnObject()
    {
        //Arrange
        var json = "{\"Id\":1,\"Name\":\"Test\"}";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        //Act
        var obj = (TestObject)Serializer.Deserialize(json, typeof(TestObject), options);

        //Assert
        Assert.That(obj, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(obj.Id, Is.EqualTo(1));
            Assert.That(obj.Name, Is.EqualTo("Test"));
        });
    }

    [Test]
    public void Serialize_WithWriter_ShouldWriteJson()
    {
        //Arrange
        var obj = new TestObject { Id = 1, Name = "Test" };
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        //Act
        Serializer.Serialize(writer, obj, typeof(TestObject));
        writer.Flush();
        var json = Encoding.UTF8.GetString(stream.ToArray());

        //Assert
        Assert.That(json, Is.EqualTo("{\"Id\":1,\"Name\":\"Test\"}"));
    }
}