namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public class CosmosDbModelConfig<TModel> : IModelConfigOptions<TModel> where TModel : class
{
    protected string IdPattern => $"{_idPrefix}{CosmosDbConstants.Separator}{{0}}";
    protected string PartitionKeyPattern => $"{_partitionKeyPrefix}{CosmosDbConstants.Separator}{{0}}";

    private string _containerName;
    private string _partitionKeyPath = "/partitionKey";
    private string _idPrefix;
    private string _partitionKeyPrefix;
    private Func<TModel, object> _idSelector;
    private Func<TModel, object> _partitionKeySelector;
    private int? _ttlInSeconds;

    public string Container => _containerName;
    public string PartitionPath => _partitionKeyPath;
    public int? TimeToLiveInSeconds => _ttlInSeconds;

    internal string GetIdValue(TModel model)
    {
        return GetIdValue(_idSelector(model));
    }

    internal string GetPartitionKeyValue(TModel model)
    {
        return GetPartitionKeyValue(_partitionKeySelector(model));
    }

    internal string GetIdValue(object key)
    {
        return string.IsNullOrEmpty(_idPrefix) ? key.ToString() : string.Format(IdPattern, key);
    }

    internal string GetPartitionKeyValue(object key)
    {
        return string.IsNullOrEmpty(_partitionKeyPrefix) ? key.ToString() : string.Format(PartitionKeyPattern, key);
    }

    public DocumentKey GetDocumentKey(TModel model)
    {
        return GetDocumentKey(_idSelector(model), _partitionKeySelector(model));
    }

    public DocumentKey GetDocumentKey(object id)
    {
        return new DocumentKey
        {
            Id = GetIdValue(id),
            PartitionKey = GetPartitionKeyValue(id)
        };
    }

    public DocumentKey GetDocumentKey(object id, object partitionKey)
    {
        return new DocumentKey
        {
            Id = GetIdValue(id),
            PartitionKey = GetPartitionKeyValue(partitionKey)
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

    public IModelConfigOptions<TModel> PartitionKeyPrefix(string prefix)
    {
        _partitionKeyPrefix = prefix;
        return this;
    }

    public IModelConfigOptions<TModel> IdPrefix(string prefix)
    {
        _idPrefix = prefix;
        return this;
    }

    public IModelConfigOptions<TModel> PartitionKey<TKey>(Func<TModel, TKey> partitionKeySelector)
    {
        _partitionKeySelector = o => partitionKeySelector(o);
        return this;
    }

    public IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> idSelector)
    {
        _idSelector = o => idSelector(o);

        // If partition key selector is not set, use the same as id
        if (_partitionKeySelector == null)
        {
            _partitionKeySelector = o => idSelector(o);
        }

        return this;
    }

    public IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds)
    {
        _ttlInSeconds = ttlInSeconds;
        return this;
    }
}
