using System.Linq.Expressions;
using AppFactory.Framework.Shared;

namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Configuration for mapping models to Cosmos DB documents with support for hierarchical partition keys
/// </summary>
public class CosmosDbModelConfig<TModel> : IModelConfigOptions<TModel> where TModel : class
{
    protected string IdPattern => $"{_idPrefix}{CosmosDbConstants.Separator}{{0}}";

    private string _containerName;
    private string _partitionKeyPath = "/partitionKey";
    private string _idPrefix;
    private Func<TModel, object?> _idSelector;
    private int? _ttlInSeconds;

    // Unified partition key configuration (handles both single and hierarchical)
    private readonly PartitionKeyConfig<TModel> _partitionKeyConfig = new PartitionKeyConfig<TModel>();

    private HashSet<string> _reservedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "id", // Cosmos DB requires an 'id' property for the document ID
        "partitionKey" // Default partition key path if not configured
    };

    private HashSet<string> ignoredPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public string Container => _containerName;

    public List<string> PartitionKeyPaths => _partitionKeyConfig.IsHierarchical 
        ? _partitionKeyConfig.GetPartitionKeyPaths() 
        : new List<string> { _partitionKeyPath };

    public int? TimeToLiveInSeconds => _ttlInSeconds;
    public bool IsHierarchicalPartitionKey => _partitionKeyConfig.IsHierarchical;
    public IEnumerable<PartitionKeyPart<TModel>> PartitionKeyParts => _partitionKeyConfig.Parts;
    public IEnumerable<string> PropertiesToIgnoreDuringSerialization => ignoredPropertyNames;

    internal string GetIdValue(TModel model)
    {
        return GetIdValue(_idSelector(model));
    }

    internal string GetIdValue(object key)
    {
        return string.IsNullOrEmpty(_idPrefix) ? key.ToString() : string.Format(IdPattern, key);
    }

    /// <summary>
    /// Strips the ID prefix from a document ID value
    /// </summary>
    internal string StripIdPrefix(string documentId)
    {
        if (string.IsNullOrEmpty(_idPrefix) || string.IsNullOrEmpty(documentId))
        {
            return documentId;
        }

        var prefix = $"{_idPrefix}{CosmosDbConstants.Separator}";
        return documentId.StartsWith(prefix) 
            ? documentId.Substring(prefix.Length) 
            : documentId;
    }

    /// <summary>
    /// Gets partition key values from the model (supports both single and hierarchical)
    /// </summary>
    internal List<string> GetPartitionKeyValues(TModel model)
    {
        return _partitionKeyConfig.GetValues(model);
    }

    /// <summary>
    /// Gets partition key values and property names for document mapping
    /// </summary>
    internal Dictionary<string, string> GetPartitionKeyProperties(TModel model)
    {
        return _partitionKeyConfig.GetPropertiesWithValues(model);
    }

    public DocumentKey GetDocumentKey(TModel model)
    {
        var partitionKeyValues = GetPartitionKeyValues(model);

        return new DocumentKey
        {
            Id = GetIdValue(_idSelector(model)),
            PartitionKeyValues = partitionKeyValues
        };
    }

    public DocumentKey GetDocumentKey(object id)
    {
        var partitionKeyValues = _partitionKeyConfig.Parts.Any()
            ? _partitionKeyConfig.Parts.Select(p => 
            {
                var value = id?.ToString() ?? string.Empty;
                return !string.IsNullOrEmpty(p.Prefix) 
                    ? $"{p.Prefix}{CosmosDbConstants.Separator}{value}" 
                    : value;
            }).ToList()
            : new List<string> { id?.ToString() ?? string.Empty };

        return new DocumentKey
        {
            Id = GetIdValue(id),
            PartitionKeyValues = partitionKeyValues
        };
    }


    public DocumentKey GetDocumentKey(object id, params object[] partitionKeyValues)
    {
        var values = new List<string>();
        for (int i = 0; i < partitionKeyValues.Length; i++)
        {
            var value = partitionKeyValues[i]?.ToString() ?? string.Empty;

            // Apply prefix if configured
            if (i < _partitionKeyConfig.Count && !string.IsNullOrEmpty(_partitionKeyConfig.Parts[i].Prefix))
            {
                value = $"{_partitionKeyConfig.Parts[i].Prefix}{CosmosDbConstants.Separator}{value}";
            }

            values.Add(value);
        }

        return new DocumentKey
        {
            Id = GetIdValue(id),
            PartitionKeyValues = values
        };
    }

    public IModelConfigOptions<TModel> ContainerName(string containerName)
    {
        _containerName = containerName;
        return this;
    }

    public IModelConfigOptions<TModel> IdPrefix(string prefix)
    {
        _idPrefix = prefix;
        return this;
    }

    public IPartitionKeyConfigOptions<TModel> PartitionKey<TKey>(Expression<Func<TModel, TKey>> propertyExpression)
    {
        var getter = PropertyExpressionHelper.InitializeGetter(propertyExpression);
        var setter = PropertyExpressionHelper.InitializeSetter(propertyExpression);
        var propertyName = PropertyExpressionHelper.GetPropertyName(propertyExpression).ToCamelCase();

        var part = new PartitionKeyPart<TModel>
        {
            Getter = o => getter(o),
            Setter = (o, v) => setter(o, (TKey)v),
            DestinationPropertyName =  propertyName
        };

        AddToIgnoreProperties(propertyName);

        _partitionKeyConfig.AddPart(part);

        return new PartitionKeyConfigBuilder<TModel>(this, part);
    }

    public IPartitionKeyConfigOptions<TModel> PartitionKey(string propertyName)
    {
        var part = new PartitionKeyPart<TModel>
        {
            DestinationPropertyName = propertyName
        };

        _partitionKeyConfig.AddPart(part);

        return new PartitionKeyConfigBuilder<TModel>(this, part);
    }

    public IModelConfigOptions<TModel> Id<TKey>(Expression<Func<TModel, TKey>> propertyExpression)
    {
        var getter = PropertyExpressionHelper.InitializeGetter(propertyExpression);
        var propertyName = PropertyExpressionHelper.GetPropertyName(propertyExpression).ToCamelCase();
        ignoredPropertyNames.Add(propertyName);
        _idSelector = o => getter(o);

        return this;
    }

    public IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds)
    {
        _ttlInSeconds = ttlInSeconds;
        return this;
    }

    public void AddToIgnoreProperties(string propertyName)
    {
        ignoredPropertyNames.Add(propertyName);
    }
}

