using System.Text.Json;
using System.Text.Json.Serialization;

namespace ASureBus.Utils;

internal static class Serializer
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        IncludeFields = true
    };

    internal static object? Deserialize(string read, Type type,
        JsonConverter converter)
    {
        return JsonSerializer.Deserialize(read, type, new JsonSerializerOptions
        {
            Converters = { converter }
        });
    }

    internal static object? Deserialize(string read, Type type,
        JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize(read, type, options);
    }

    internal static async Task<TReturn?> Deserialize<TReturn>(Stream utf8Json,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return await JsonSerializer
            .DeserializeAsync<TReturn>(utf8Json, options, cancellationToken)
            .ConfigureAwait(false);
    }
    
    internal static object? Deserialize(string read, Type type)
    {
        return JsonSerializer.Deserialize(read, type);
    }

    internal static string Serialize<TItem>(TItem item)
    {
        return JsonSerializer.Serialize(item, _jsonSerializerOptions);
    }

    internal static void Serialize(Utf8JsonWriter writer, object? value,
        Type inputType, JsonSerializerOptions? options = null)
    {
        JsonSerializer.Serialize(writer, value, inputType, options ?? _jsonSerializerOptions);
    }
}