using System.Text.Json;
using System.Text.Json.Serialization;
using ASureBus.Abstractions;
using ASureBus.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace ASureBus.Core.Sagas;

internal class SagaConverter(Type sagaType, Type sagaDataType, IServiceProvider services)
    : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return sagaType.IsAssignableFrom(typeToConvert);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        
        //TODO hardcoded string
        var sagaDataJson = jsonDocument.RootElement.TryGetProperty("SagaData", out var retrievedSagaDataElement) 
            ? retrievedSagaDataElement.GetRawText()
            : throw new JsonException("SagaData property not found");
        var sagaData = Serializer.Deserialize(sagaDataJson, sagaDataType, options);

        var correlationId = jsonDocument.RootElement.TryGetProperty(nameof(ISaga.CorrelationId),
            out var retrievedCorrelationIdElement)
            ? retrievedCorrelationIdElement.GetGuid()
            : throw new JsonException("CorrelationId property not found");

        var saga = ActivatorUtilities.CreateInstance(services, sagaType);
        
        //TODO hardcoded string
        var sagaDataProperty = sagaType.GetProperty("SagaData");
        sagaDataProperty.SetValue(saga, sagaData);
        
        var correlationIdProperty = sagaType.GetProperty(nameof(ISaga.CorrelationId));
        correlationIdProperty.SetValue(saga, correlationId);

        return saga;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        //TODO hardcoded string
        writer.WritePropertyName("SagaData");
        var sagaData = value.GetType().GetProperty("SagaData").GetValue(value);
        Serializer.Serialize(writer, sagaData, sagaDataType, options);
        
        writer.WritePropertyName(nameof(ISaga.CorrelationId));
        writer.WriteStringValue(value.GetType().GetProperty(nameof(ISaga.CorrelationId)).GetValue(value).ToString());
        
        writer.WriteEndObject();
    }
}