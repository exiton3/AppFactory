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

        // Add/Update id and partitionKey
        document[CosmosDbConstants.Id] = documentKey.Id;
        document[CosmosDbConstants.PartitionKey] = documentKey.PartitionKey;

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
        cleanDocument.Remove(CosmosDbConstants.PartitionKey);
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
