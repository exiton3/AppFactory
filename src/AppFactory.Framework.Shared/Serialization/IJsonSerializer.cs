using System.Text.Json;

namespace AppFactory.Framework.Shared.Serialization;

public interface IJsonSerializer
{
    TValue? Deserialize<TValue>(string json, JsonSerializerOptions? options = null);
    string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null);
    object Deserialize(string json, Type objectType);
}