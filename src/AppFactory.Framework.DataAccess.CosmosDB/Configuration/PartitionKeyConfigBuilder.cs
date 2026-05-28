using System.Linq.Expressions;

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

    public PartitionKeyConfigBuilder(
        CosmosDbModelConfig<TModel> parentConfig, 
        PartitionKeyPart<TModel> currentPart)
    {
        _parentConfig = parentConfig;
        _currentPart = currentPart;
    }

    public IPartitionKeyConfigOptions<TModel> WithName(string propertyName)
    {
        _currentPart.DestinationPropertyName = propertyName;
        _parentConfig.AddToIgnoreProperties(propertyName);
        return this;
    }

    public IPartitionKeyConfigOptions<TModel> WithPrefix(string prefix)
    {
        _currentPart.Prefix = prefix;

        return this;
    }

    public IPartitionKeyConfigOptions<TModel> UseResolver<TResolver>() where TResolver : IPartitionKeyValueResolver, new()
    {
        _currentPart.Resolver = new TResolver();

        return this;
    }

    // Implement IModelConfigOptions<TModel> by delegating to parent config
    public IModelConfigOptions<TModel> ContainerName(string containerName)
    {
        return _parentConfig.ContainerName(containerName);
    }

    public IModelConfigOptions<TModel> IdPrefix(string prefix)
    {
        return _parentConfig.IdPrefix(prefix);
    }

    public IPartitionKeyConfigOptions<TModel> PartitionKey<TKey>(Expression<Func<TModel, TKey>> propertyExpression)
    {
        return _parentConfig.PartitionKey(propertyExpression);
    }

    public IPartitionKeyConfigOptions<TModel> PartitionKey(string propertyName)
    {
       return _parentConfig.PartitionKey(propertyName);
    }

    public IModelConfigOptions<TModel> Id<TKey>(Expression<Func<TModel, TKey>> propertyExpression)
    {
        return _parentConfig.Id(propertyExpression);
    }


    public IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds)
    {
        return _parentConfig.TimeToLive(ttlInSeconds);
    }
}
