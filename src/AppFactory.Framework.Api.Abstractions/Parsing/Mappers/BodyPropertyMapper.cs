using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Shared.Serialization;
using System.Text.Json;

namespace AppFactory.Framework.Api.Parsing.Mappers;

class BodyPropertyMapper : IPropertyMapper
{
    private readonly IJsonSerializer _jsonSerializer;

    public BodyPropertyMapper(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public bool CanMap(IPropertyMapInfo mapInfo)
    {
        return mapInfo.MapFrom == From.Body;
    }

    public object Map(InputRequest request, IPropertyMapInfo mapInfo)
    {
        if (mapInfo.ContentType == BodyContentType.Text)
        {
           return request.Body;
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            if (mapInfo.IsRequired)
            {
                throw new RequestParsingException("Request body is required but was empty.");
            }

            return null;
        }

        // Backward-compatible path: mapping entire body to the target type.
        if (string.IsNullOrWhiteSpace(mapInfo.FieldName))
        {
            return _jsonSerializer.Deserialize(request.Body, mapInfo.PropertyType);
        }

        try
        {
            using var document = JsonDocument.Parse(request.Body);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                JsonElement fieldValue;
                var found = root.TryGetProperty(mapInfo.FieldName, out fieldValue) ||
                            TryGetPropertyIgnoreCase(root, mapInfo.FieldName, out fieldValue);

                if (!found)
                {
                    if (mapInfo.IsRequired)
                    {
                        throw new RequestParsingException($"The body parameter '{mapInfo.FieldName}' was not found.");
                    }

                    return null;
                }

                if (fieldValue.ValueKind == JsonValueKind.Null)
                {
                    if (mapInfo.IsRequired)
                    {
                        throw new RequestParsingException($"The body parameter '{mapInfo.FieldName}' is required and cannot be null.");
                    }

                    return null;
                }

                var rawValue = fieldValue.GetRawText();
                return _jsonSerializer.Deserialize(rawValue, mapInfo.PropertyType);
            }
        }
        catch (JsonException)
        {
            // Fallback for non-object payloads or legacy flows that map whole-body content.
        }

        return _jsonSerializer.Deserialize(request.Body, mapInfo.PropertyType);
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement root, string propertyName, out JsonElement value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}