using System.Linq.Expressions;

namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public interface IModelConfigOptions<TModel> where TModel : class
{
    /// <summary>
    /// Sets the Cosmos DB container name
    /// </summary>
    IModelConfigOptions<TModel> ContainerName(string containerName);

    /// <summary>
    /// Sets the ID prefix
    /// </summary>
    IModelConfigOptions<TModel> IdPrefix(string prefix);

    /// <summary>
    /// Sets the partition key selector and returns a fluent interface for configuring this partition key part
    /// Allows chaining .WithPropertyName() and .WithPrefix()
    /// </summary>
    IPartitionKeyConfigOptions<TModel> PartitionKey<TKey>(Expression<Func<TModel, TKey>> partitionKeySelector);

    /// <summary>
    /// Sets the ID selector
    /// </summary>
    IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> idSelector);

    /// <summary>
    /// Sets the Time-To-Live in seconds
    /// </summary>
    IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds);
}

