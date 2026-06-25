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

    public ModelMapper(CosmosDbModelConfig<TModel> config)
    {
        _config = config;
        _serializerSettings = GetJsonSerializerSettings(config);
        _idKeyMapper = new IdKeyMapper<TModel>(_config);
    }

    public CosmosDbDocument MapToDocument(TModel model)
    {
        var modelJson = JsonConvert.SerializeObject(model, _serializerSettings);
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

        MapBackId(document, cleanDocument);

        var json = JsonConvert.SerializeObject(cleanDocument, _serializerSettings);
        var model = JsonConvert.DeserializeObject<TModel>(json, _serializerSettings);

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


    private void MapBackId(CosmosDbDocument document, Dictionary<string, object> cleanDocument)
    {
        if (document.ContainsKey(CosmosDbConstants.Id))
        {
            var documentId = document[CosmosDbConstants.Id]?.ToString();
            var modelId = _config.StripIdPrefix(documentId);

            cleanDocument[CosmosDbConstants.Id] = modelId;
        }
    }

    private JsonSerializerSettings GetJsonSerializerSettings(CosmosDbModelConfig<TModel> config)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Converters =
            {
                new Newtonsoft.Json.Converters.StringEnumConverter(new CamelCaseNamingStrategy())
            }
        };

        // Configure property ignore list using custom contract resolver
        if (config.PropertiesToIgnoreDuringSerialization.Any())
        {
            var resolver = new IgnorePropertiesResolver(config.PropertiesToIgnoreDuringSerialization);
            settings.ContractResolver = resolver;
        }

        return settings;
    }

    // Custom contract resolver to ignore specific properties
    private class IgnorePropertiesResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly IEnumerable<string> _propertiesToIgnore;

        public IgnorePropertiesResolver(IEnumerable<string> propertiesToIgnore)
        {
            _propertiesToIgnore = propertiesToIgnore;
        }

        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (_propertiesToIgnore.Contains(property.PropertyName, StringComparer.OrdinalIgnoreCase))
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }
}

