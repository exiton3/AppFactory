using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppFactory.Framework.Shared.Serialization;

/// <summary>
/// Converts System.Text.Json's default <see cref="JsonElement"/> representation of <c>object</c>-typed
/// properties into plain .NET types so they survive round-tripping through Newtonsoft.Json
/// (used by the Cosmos DB persistence layer).
/// </summary>
public class ObjectJsonConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadValue(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value.GetType() == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }

    private static object? ReadValue(ref Utf8JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out var l) ? l : reader.GetDouble(),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Null => null,
            JsonTokenType.StartArray => ReadArray(ref reader),
            JsonTokenType.StartObject => ReadObject(ref reader),
            _ => null,
        };
    }

    private static List<object?> ReadArray(ref Utf8JsonReader reader)
    {
        var list = new List<object?>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(ReadValue(ref reader));
        }
        return list;
    }

    private static Dictionary<string, object?> ReadObject(ref Utf8JsonReader reader)
    {
        var dict = new Dictionary<string, object?>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            var key = reader.GetString()!;
            reader.Read();
            dict[key] = ReadValue(ref reader);
        }
        return dict;
    }
}
