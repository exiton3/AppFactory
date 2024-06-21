using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppFactory.Framework.Infrastructure.Serialization;

public class DefaultJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _defaultOptions;

    public DefaultJsonSerializer()
    {
        _defaultOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            NumberHandling = JsonNumberHandling.AllowReadingFromString ,
            
        };
    }
    public TValue? Deserialize<TValue>(string json, JsonSerializerOptions? options = null)
    {
      
        return JsonSerializer.Deserialize<TValue>(json, options ?? _defaultOptions);
    }

    public object Deserialize(string json, Type objectType)
    {
        return JsonSerializer.Deserialize(json, objectType, _defaultOptions);
    }
    public string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? _defaultOptions);
    }
}