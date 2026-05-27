using System.Text.Json;
using System.Text.Json.Serialization;
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;

internal class ModelMapper<TModel> : IModelMapper<TModel> where TModel : class
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly CosmosDbModelConfig<TModel> _config;

    public ModelMapper(CosmosDbModelConfig<TModel> config)
    {
        _config = config;
        _serializerOptions = GetJsonSerializerOptions();
    }

    public CosmosDbDocument MapToDocument(TModel model)
    {
        var modelJson = JsonSerializer.Serialize(model, _serializerOptions);
        var document = new CosmosDbDocument(modelJson);

        var documentKey = _config.GetDocumentKey(model);

        // Add/Update id
        document[CosmosDbConstants.Id] = documentKey.Id;

        // Add partition key properties (works for both single and hierarchical)
        var partitionKeyProperties = _config.GetPartitionKeyProperties(model);
        foreach (var kvp in partitionKeyProperties)
        {
            document[kvp.Key] = kvp.Value;
        }

        // Add TTL if configured
        if (_config.TimeToLiveInSeconds.HasValue)
        {
            document["ttl"] = _config.TimeToLiveInSeconds.Value;
        }

        return document;
    }

    public TModel MapFromDocument(CosmosDbDocument document)
    {
        // Remove CosmosDB specific properties before deserializing to model
        var cleanDocument = new Dictionary<string, object>(document);
        cleanDocument.Remove(CosmosDbConstants.Id);

        // Remove partition key properties (both single and hierarchical)
        var partitionKeyPaths = _config.PartitionKeyPaths;
        foreach (var path in partitionKeyPaths)
        {
            var propertyName = path.TrimStart('/');
            cleanDocument.Remove(propertyName);
        }

        // Remove Cosmos DB system properties
        cleanDocument.Remove("_rid");
        cleanDocument.Remove("_self");
        cleanDocument.Remove("_etag");
        cleanDocument.Remove("_attachments");
        cleanDocument.Remove("_ts");
        cleanDocument.Remove("ttl");

        var json = JsonSerializer.Serialize(cleanDocument, _serializerOptions);
        var model = JsonSerializer.Deserialize<TModel>(json, _serializerOptions);

        return model;
    }

    private static JsonSerializerOptions GetJsonSerializerOptions()
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
}

