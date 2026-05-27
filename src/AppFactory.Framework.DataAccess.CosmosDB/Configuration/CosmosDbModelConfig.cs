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
    private string _pendingPartitionKeyPrefix; // Store prefix until PartitionKey() is called
    private Func<TModel, object> _idSelector;
    private int? _ttlInSeconds;

    // Unified partition key configuration (handles both single and hierarchical)
    private readonly PartitionKeyConfig<TModel> _partitionKeyConfig = new PartitionKeyConfig<TModel>();

    public string Container => _containerName;

    /// <summary>
    /// Gets the partition key paths - auto-derived from property names in partition key parts
    /// </summary>
    public List<string> PartitionKeyPaths => _partitionKeyConfig.IsHierarchical 
        ? _partitionKeyConfig.GetPartitionKeyPaths() 
        : new List<string> { _partitionKeyPath };

    public int? TimeToLiveInSeconds => _ttlInSeconds;
    public bool IsHierarchicalPartitionKey => _partitionKeyConfig.IsHierarchical;

    internal string GetIdValue(TModel model)
    {
        return GetIdValue(_idSelector(model));
    }

    internal string GetIdValue(object key)
    {
        return string.IsNullOrEmpty(_idPrefix) ? key.ToString() : string.Format(IdPattern, key);
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

    /// <summary>
    /// Internal method to add a partition key part (used by PartitionKeyConfigBuilder)
    /// </summary>
    internal void AddPartitionKeyPart(PartitionKeyPart<TModel> part)
    {
        _partitionKeyConfig.AddPart(part);
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

    public DocumentKey GetDocumentKey(object id, object partitionKey)
    {
        var partitionKeyValue = partitionKey?.ToString() ?? string.Empty;

        // Apply prefix from first partition key part if configured
        if (_partitionKeyConfig.Parts.Any() && !string.IsNullOrEmpty(_partitionKeyConfig.Parts[0].Prefix))
        {
            partitionKeyValue = $"{_partitionKeyConfig.Parts[0].Prefix}{CosmosDbConstants.Separator}{partitionKeyValue}";
        }

        return new DocumentKey
        {
            Id = GetIdValue(id),
            PartitionKeyValues = new List<string> { partitionKeyValue }
        };
    }

    /// <summary>
    /// Gets document key with hierarchical partition key values
    /// </summary>
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

    public IModelConfigOptions<TModel> PartitionKeyPath(string partitionKeyPath)
    {
        _partitionKeyPath = partitionKeyPath;
        return this;
    }

    public IModelConfigOptions<TModel> AddPartitionKeyPath(params string[] paths)
    {
        // This method is now deprecated but kept for backward compatibility
        // Partition key paths are auto-derived from property names
        // This method does nothing but returns this for method chaining
        return this;
    }

    public IModelConfigOptions<TModel> PartitionKeyPrefix(string prefix)
    {
        // If PartitionKey() has already been called, apply the prefix to the first part
        if (_partitionKeyConfig.Parts.Any())
        {
            _partitionKeyConfig.Parts[0].Prefix = prefix;
        }
        else
        {
            // Otherwise, store the prefix to be applied when PartitionKey() is called
            _pendingPartitionKeyPrefix = prefix;
        }
        return this;
    }

    public IModelConfigOptions<TModel> IdPrefix(string prefix)
    {
        _idPrefix = prefix;
        return this;
    }

    public IPartitionKeyConfigOptions<TModel> PartitionKey<TKey>(Func<TModel, TKey> partitionKeySelector)
    {
        // Create a new partition key part with default property name
        var part = new PartitionKeyPart<TModel>
        {
            Selector = o => partitionKeySelector(o),
            PropertyName = _partitionKeyPath.TrimStart('/'), // Default property name
            Prefix = _pendingPartitionKeyPrefix // Apply pending prefix if any
        };

        // Clear pending prefix
        _pendingPartitionKeyPrefix = null;

        // Return a builder that allows fluent configuration of this part
        return new PartitionKeyConfigBuilder<TModel>(this, part);
    }

    public IModelConfigOptions<TModel> AddPartitionKey<TKey>(Func<TModel, TKey> partitionKeySelector, string propertyName = null, string prefix = null)
    {
        // Ensure property name is provided
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("PropertyName must be specified when using AddPartitionKey", nameof(propertyName));
        }

        var part = new PartitionKeyPart<TModel>
        {
            Selector = o => partitionKeySelector(o),
            PropertyName = propertyName,
            Prefix = prefix
        };

        _partitionKeyConfig.AddPart(part);
        return this;
    }

    public IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> idSelector)
    {
        _idSelector = o => idSelector(o);

        // If partition key is not set, use the same as id (single partition key)
        if (!_partitionKeyConfig.Parts.Any())
        {
            var part = new PartitionKeyPart<TModel>
            {
                Selector = o => idSelector(o),
                PropertyName = _partitionKeyPath.TrimStart('/'),
                Prefix = _pendingPartitionKeyPrefix // Apply pending prefix if any
            };
            _partitionKeyConfig.AddPart(part);
            _pendingPartitionKeyPrefix = null;
        }

        return this;
    }

    public IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds)
    {
        _ttlInSeconds = ttlInSeconds;
        return this;
    }
}

