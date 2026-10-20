using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AppFactory.Framework.DataAccess.CosmosDB.Mapping;


internal class ModelMapper<TModel> : IModelMapper<TModel> where TModel : class
{
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly CosmosDbModelConfig<TModel> _config;
    private readonly IIdKeyMapper<TModel> _idKeyMapper;
    private readonly HashSet<string> _ignoredRootProperties;
    private readonly JsonProperty? _sharedIdProperty;

    public ModelMapper(CosmosDbModelConfig<TModel> config)
    {
        _config = config;
        _serializerSettings = GetJsonSerializerSettings(config);
        _idKeyMapper = new IdKeyMapper<TModel>(_config);
        _ignoredRootProperties = new HashSet<string>(config.PropertiesToIgnoreDuringSerialization, StringComparer.OrdinalIgnoreCase);
        _sharedIdProperty = FindSharedIdProperty();
    }

    // The Cosmos 'id' belongs to the property configured with Id(...). When the model has another
    // property serialized as 'id' (e.g. Id(e => e.EmployeeNumber) while the model also has Id),
    // both share the document 'id' field.
    private JsonProperty? FindSharedIdProperty()
    {
        if (string.IsNullOrEmpty(_config.IdPropertyName)
            || _serializerSettings.ContractResolver!.ResolveContract(typeof(TModel)) is not JsonObjectContract contract)
        {
            return null;
        }

        return contract.Properties.FirstOrDefault(p =>
            !p.Ignored
            && string.Equals(p.PropertyName, CosmosDbConstants.Id, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(p.UnderlyingName, _config.IdPropertyName, StringComparison.Ordinal));
    }

    public CosmosDbDocument MapToDocument(TModel model)
    {
        EnsureSharedIdPropertyMatches(model);

        var modelJson = JsonConvert.SerializeObject(model, _serializerSettings);
        var document = new CosmosDbDocument(modelJson);

        RemoveIgnoredRootProperties(document);

        var mappedId = _idKeyMapper.MapId(model);
        document[mappedId.Key] = mappedId.Value;

        MapPartitionKeys(model, document);

        if (_config.TimeToLiveInSeconds.HasValue)
        {
            document["ttl"] = _config.TimeToLiveInSeconds.Value;
        }

        return document;
    }

    // A property sharing the 'id' field can only be stored when it holds the same value as the configured Id,
    // otherwise its value would be silently overwritten by the configured Id.
    private void EnsureSharedIdPropertyMatches(TModel model)
    {
        if (_sharedIdProperty == null)
        {
            return;
        }

        var sharedValue = _sharedIdProperty.ValueProvider!.GetValue(model);
        if (sharedValue == null)
        {
            return;
        }

        var idValue = _config.GetIdPropertyValue(model);
        if (!string.Equals(sharedValue.ToString(), idValue?.ToString(), StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{typeof(TModel).Name}.{_sharedIdProperty.UnderlyingName} ('{sharedValue}') differs from the configured Id " +
                $"{typeof(TModel).Name}.{_config.IdPropertyName} ('{idValue}'). Both are stored in the Cosmos 'id' field, " +
                $"so the value of {_sharedIdProperty.UnderlyingName} would be lost.");
        }
    }

    // Ignored properties (Id, partition key sources) are removed from the root of the document only.
    // Doing it in the contract resolver would also drop same-named properties of nested objects,
    // because contracts are resolved per type, not per position in the object graph.
    private void RemoveIgnoredRootProperties(CosmosDbDocument document)
    {
        if (_ignoredRootProperties.Count == 0)
        {
            return;
        }

        foreach (var key in document.Keys.Where(_ignoredRootProperties.Contains).ToList())
        {
            document.Remove(key);
        }
    }

    private void MapPartitionKeys(TModel model, CosmosDbDocument document)
    {
        foreach (var partitionKeyConfig in _config.PartitionKeyParts)
        {
            object value;
            if (partitionKeyConfig.IsResolverSet)
            {
                value = partitionKeyConfig.Resolver.GetValue();
            }
            else
            {
                value = partitionKeyConfig.Getter(model);
            }

            if(partitionKeyConfig.IsPrefixSet)
            {
                value = $"{partitionKeyConfig.Prefix}{CosmosDbConstants.Separator}{value}";
            }
            var mappedPropertyName = partitionKeyConfig.IsPropertyNameSet ? partitionKeyConfig.DestinationPropertyName : partitionKeyConfig.SourcePropertyName;
            
            document[mappedPropertyName] = value;
        }
    }

    public TModel MapFromDocument(CosmosDbDocument document)
    {
        var cleanDocument = new Dictionary<string, object>(document);

        var modelId = MapBackId(document, cleanDocument);

        var json = JsonConvert.SerializeObject(cleanDocument, _serializerSettings);
        var model = JsonConvert.DeserializeObject<TModel>(json, _serializerSettings);

        if (modelId != null)
        {
            _config.SetIdPropertyValue(model, modelId);
        }

        MapBackPartitionKeys(document, model);

        return model;
    }

    private void MapBackPartitionKeys(CosmosDbDocument document, TModel? model)
    {
        foreach (var partitionKeyConfig in _config.PartitionKeyParts)
        {
            var key = partitionKeyConfig.IsPropertyNameSet ? partitionKeyConfig.DestinationPropertyName : partitionKeyConfig.SourcePropertyName;
            
            var value = document[key];

            if(partitionKeyConfig.IsPrefixSet)
            {
                string prefix = $"{partitionKeyConfig.Prefix}{CosmosDbConstants.Separator}";
                value = value.ToString().StartsWith(prefix) ? value.ToString().Substring(prefix.Length) : value;
            }

            if (!partitionKeyConfig.IsResolverSet)
            {
                partitionKeyConfig.Setter(model, value);
            }
        }
    }


    // Returns the document id without prefix. It stays in the clean document as 'id' so a model property
    // sharing the 'id' field receives the same value.
    private string? MapBackId(CosmosDbDocument document, Dictionary<string, object> cleanDocument)
    {
        if (!document.ContainsKey(CosmosDbConstants.Id))
        {
            return null;
        }

        var documentId = document[CosmosDbConstants.Id]?.ToString();
        var modelId = _config.StripIdPrefix(documentId);

        cleanDocument[CosmosDbConstants.Id] = modelId;

        return modelId;
    }

    private static JsonSerializerSettings GetJsonSerializerSettings(CosmosDbModelConfig<TModel> config)
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new CosmosDbContractResolver(config.TypeDiscriminators, config.PropertyDiscriminators),
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Converters =
            {
                new Newtonsoft.Json.Converters.StringEnumConverter(new CamelCaseNamingStrategy())
            }
        };
    }
}
