namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Builder for fluently configuring a specific partition key part
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
internal class PartitionKeyConfigBuilder<TModel> : IPartitionKeyConfigOptions<TModel> 
    where TModel : class
{
    private readonly CosmosDbModelConfig<TModel> _parentConfig;
    private readonly PartitionKeyPart<TModel> _currentPart;
    private bool _isAdded;

    public PartitionKeyConfigBuilder(
        CosmosDbModelConfig<TModel> parentConfig, 
        PartitionKeyPart<TModel> currentPart)
    {
        _parentConfig = parentConfig;
        _currentPart = currentPart;
        _isAdded = false;
    }

    public IPartitionKeyConfigOptions<TModel> WithPropertyName(string propertyName)
    {
        _currentPart.PropertyName = propertyName;
        return this;
    }

    public IPartitionKeyConfigOptions<TModel> WithPrefix(string prefix)
    {
        _currentPart.Prefix = prefix;
        return this;
    }

    // Ensure the part is added when moving to next configuration method
    private void EnsurePartAdded()
    {
        if (!_isAdded)
        {
            _parentConfig.AddPartitionKeyPart(_currentPart);
            _isAdded = true;
        }
    }

    // Implement IModelConfigOptions<TModel> by delegating to parent config
    public IModelConfigOptions<TModel> ContainerName(string containerName)
    {
        EnsurePartAdded();
        return _parentConfig.ContainerName(containerName);
    }

    public IModelConfigOptions<TModel> PartitionKeyPrefix(string prefix)
    {
        EnsurePartAdded();
        return _parentConfig.PartitionKeyPrefix(prefix);
    }

    public IModelConfigOptions<TModel> IdPrefix(string prefix)
    {
        EnsurePartAdded();
        return _parentConfig.IdPrefix(prefix);
    }

    public IPartitionKeyConfigOptions<TModel> PartitionKey<TKey>(Func<TModel, TKey> partitionKeySelector)
    {
        EnsurePartAdded();
        return _parentConfig.PartitionKey(partitionKeySelector);
    }

    public IModelConfigOptions<TModel> AddPartitionKey<TKey>(
        Func<TModel, TKey> partitionKeySelector, 
        string propertyName = null, 
        string prefix = null)
    {
        EnsurePartAdded();
        return _parentConfig.AddPartitionKey(partitionKeySelector, propertyName, prefix);
    }

    public IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> idSelector)
    {
        EnsurePartAdded();
        return _parentConfig.Id(idSelector);
    }

    public IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds)
    {
        EnsurePartAdded();
        return _parentConfig.TimeToLive(ttlInSeconds);
    }
}
