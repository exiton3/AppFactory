using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

/// <summary>
/// Writes the discriminator into each object of a polymorphic property and uses it to pick the concrete
/// type when reading.
/// </summary>
/// <remarks>
/// Attached to properties (JsonProperty.Converter / ItemConverter), never to a type contract. Deserializing
/// into the concrete type therefore does not invoke this converter again, which matters when a concrete base
/// type (e.g. Person registered as "person") is one of the registered types. Properties of the concrete type
/// get their own converters, so nested hierarchies work at any depth.
/// Stateless, so one instance is shared by all requests.
/// </remarks>
internal sealed class DiscriminatorConverter : JsonConverter
{
    private readonly DiscriminatorConfig _config;
    private readonly string _propertyName;

    public DiscriminatorConverter(DiscriminatorConfig config)
    {
        _config = config;
        _propertyName = config.PropertyName!;
    }

    public override bool CanConvert(Type objectType) => true;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var type = value.GetType();
        if (!_config.ValuesByType.TryGetValue(type, out var discriminator))
        {
            throw new JsonSerializationException(
                $"{_config.Description}: {type.Name} at '{writer.Path}' is not registered. " +
                $"Add WithValue<{type.Name}>(\"...\") to the configuration.");
        }

        var json = JObject.FromObject(value, serializer);
        json.AddFirst(new JProperty(_propertyName, discriminator));
        json.WriteTo(writer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        var path = reader.Path;
        var json = JObject.Load(reader);
        var targetType = ResolveType(json, objectType, path);

        json.Remove(_propertyName);

        return serializer.Deserialize(json.CreateReader(), targetType);
    }

    private Type ResolveType(JObject json, Type objectType, string path)
    {
        var token = json.GetValue(_propertyName, StringComparison.Ordinal);
        Type? targetType;

        if (token == null || token.Type == JTokenType.Null)
        {
            // Objects stored before the discriminator was configured
            targetType = _config.DefaultType ?? (objectType.IsAbstract || objectType.IsInterface ? null : objectType);
            if (targetType == null)
            {
                throw new JsonSerializationException(
                    $"{_config.Description}: object at '{path}' has no '{_propertyName}' and {objectType.Name} is abstract. " +
                    "Add DefaultTo<T>() to the configuration to read objects stored without a discriminator.");
            }
        }
        else if (!_config.TryGetType(token.ToString(), out targetType))
        {
            throw new JsonSerializationException(
                $"{_config.Description}: unknown {_propertyName} '{token}' at '{path}'. " +
                $"Registered values: {string.Join(", ", _config.ValuesByType.Values)}.");
        }

        if (!objectType.IsAssignableFrom(targetType))
        {
            throw new JsonSerializationException(
                $"{_config.Description}: {_propertyName} '{token}' at '{path}' maps to {targetType.Name}, " +
                $"which is not assignable to {objectType.Name}.");
        }

        return targetType;
    }
}
