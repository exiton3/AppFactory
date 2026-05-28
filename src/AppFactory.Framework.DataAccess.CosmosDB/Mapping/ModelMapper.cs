using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;


internal class ModelMapper<TModel> : IModelMapper<TModel> where TModel : class
{
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly CosmosDbModelConfig<TModel> _config;
    private readonly IIdKeyMapper<TModel> _idKeyMapper;
    public ModelMapper(CosmosDbModelConfig<TModel> config)
    {
        _config = config;
        _serializerOptions = GetJsonSerializerOptions(config);
        _idKeyMapper = new IdKeyMapper<TModel>(_config);
    }

    public CosmosDbDocument MapToDocument(TModel model)
    {
        var modelJson = JsonSerializer.Serialize(model, _serializerOptions);
        var document = new CosmosDbDocument(modelJson);
       
        var mappedId = _idKeyMapper.MapId(model);
        document[mappedId.Key] = mappedId.Value;

        MapPartitionKeys(model, document);

        if (_config.TimeToLiveInSeconds.HasValue)
        {
            document["ttl"] = _config.TimeToLiveInSeconds.Value;
        }

        return document;
    }

    private void MapPartitionKeys(TModel model, CosmosDbDocument document)
    {
        foreach (var partitionKeyConfig in _config.PartitionKeyParts)
        {
            if (partitionKeyConfig.IsPropertyNameSet)
            {
                document.Remove(partitionKeyConfig.OriginalPropertyName);
            }
            var value = partitionKeyConfig.Selector(model);

            if(partitionKeyConfig.IsPrefixSet)
            {
                value = $"{partitionKeyConfig.Prefix}{CosmosDbConstants.Separator}{value}";
            }
            var mappedPropertyName = partitionKeyConfig.IsPropertyNameSet ? partitionKeyConfig.PropertyName : partitionKeyConfig.OriginalPropertyName;
            
            document[mappedPropertyName] = value;
        }
    }

    public TModel MapFromDocument(CosmosDbDocument document)
    {
        var cleanDocument = new Dictionary<string, object>(document);
        
        RemoveCosmosDbSystemProperties(cleanDocument);

        MapBackId(document, cleanDocument);
       

        var json = JsonSerializer.Serialize(cleanDocument, _serializerOptions);
        var model = JsonSerializer.Deserialize<TModel>(json, _serializerOptions);

        MapBackPartitionKeys(document, model);

        return model;
    }

    private void MapBackPartitionKeys(CosmosDbDocument document, TModel? model)
    {
        foreach (var partitionKeyConfig in _config.PartitionKeyParts)
        {
            var key = partitionKeyConfig.IsPropertyNameSet ? partitionKeyConfig.PropertyName : partitionKeyConfig.OriginalPropertyName;

            var value = document[key].ToString();

            if(partitionKeyConfig.IsPrefixSet)
            {
                string prefix = $"{partitionKeyConfig.Prefix}{CosmosDbConstants.Separator}";
                value = value.StartsWith(prefix) ? value.Substring(prefix.Length) : value;
            }
            
            partitionKeyConfig.Setter(model, value);
        }
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

    private  JsonSerializerOptions GetJsonSerializerOptions(CosmosDbModelConfig<TModel> config)
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(IgnorePropertiesModifier)
            
        };
    }

    private  void IgnorePropertiesModifier(JsonTypeInfo obj)
    {
        
        var propertiesToIgnore = new HashSet<string>(_config.PartitionKeyParts.Select(x => x.OriginalPropertyName));

        if (obj.Type == typeof(TModel))
        {
            foreach (var property in obj.Properties)
            {
                if (propertiesToIgnore.Contains(property.Name))
                {
                    property.ShouldSerialize = (p,v) => false;
                }
            }
        }
    }
}

