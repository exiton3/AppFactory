using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.DynamoDb;

namespace AppFactory.Framework.DataAccess.Mapping;

internal class ModelMapper<TModel> : IModelMapper<TModel> where TModel : class
{
    private readonly JsonSerializerOptions _defaultOptions;
    private readonly DynamoDbModelConfig<TModel> _config;
    public ModelMapper(DynamoDbModelConfig<TModel> config)
    {
        _config = config;
        _defaultOptions = GetJsonDefaultOptions();
    }
    public DynamoDbItem MapToAttributes(TModel model)
    {
        var modelJson = JsonSerializer.Serialize(model, _defaultOptions);
        var modelDoc = Document.FromJson(modelJson);

        var primaryKey = _config.GetPrimaryKey(model);

        var attributeMap = GetMergedAttributeValues(primaryKey, modelDoc);

        return new DynamoDbItem(attributeMap);


    }

    public TModel MapModelFromAttributes(DynamoDbItem item)
    {
        item.Remove(DynamoDbConstants.PK);
        item.Remove(DynamoDbConstants.SK);

        var itemAsDocument = Document.FromAttributeMap(item);

        var model = JsonSerializer.Deserialize<TModel>(itemAsDocument.ToJson(), _defaultOptions);

        return model;
    }

    private static JsonSerializerOptions GetJsonDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    private static Dictionary<string, AttributeValue> GetMergedAttributeValues(PrimaryKey primaryKey, Document modelDoc)
    {
        var keyAttributes = primaryKey.ToAttributeValues();

        var attributeMap = modelDoc.ToAttributeMap();

        attributeMap = keyAttributes.Union(attributeMap).ToDictionary(k => k.Key, v => v.Value);

        return attributeMap;
    }
}