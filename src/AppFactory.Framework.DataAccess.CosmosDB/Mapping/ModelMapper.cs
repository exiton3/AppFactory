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
        var cleanDocument = new Dictionary<string, object>(document);
        RemoveCosmosDbSystemProperties(cleanDocument);

        MapBackId(document, cleanDocument);
       

        var json = JsonSerializer.Serialize(cleanDocument, _serializerOptions);
        var model = JsonSerializer.Deserialize<TModel>(json, _serializerOptions);

        foreach (var partitionKeyConfig in _config.PartitionKeyParts)
        {
            var value = document[partitionKeyConfig.PropertyName].ToString();

            if(partitionKeyConfig.IsPrefixSet)
            {
                string prefix = $"{partitionKeyConfig.Prefix}{CosmosDbConstants.Separator}";
                value = value.StartsWith(prefix) ? value.Substring(prefix.Length) : value;
            }
            
            partitionKeyConfig.Setter(model, value);
        }

        return model;
    }

    private static void RemoveCosmosDbSystemProperties(Dictionary<string, object> cleanDocument)
    {
        cleanDocument.Remove("_rid");
        cleanDocument.Remove("_self");
        cleanDocument.Remove("_etag");
        cleanDocument.Remove("_attachments");
        cleanDocument.Remove("_ts");
        cleanDocument.Remove("ttl");
    }

    private void MapBackId(CosmosDbDocument document, Dictionary<string, object> cleanDocument)
    {
        if (document.ContainsKey(CosmosDbConstants.Id))
        {
            var documentId = document[CosmosDbConstants.Id]?.ToString();
            var modelId = _config.StripIdPrefix(documentId);
            cleanDocument[CosmosDbConstants.Id] = modelId;
        }
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

