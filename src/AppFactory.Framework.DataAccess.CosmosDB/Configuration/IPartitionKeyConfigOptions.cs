namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Fluent configuration interface for a specific partition key part
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public interface IPartitionKeyConfigOptions<TModel> : IModelConfigOptions<TModel> 
    where TModel : class
{
    /// <summary>
    /// Sets the property name for this partition key in the Cosmos DB document
    /// </summary>
    /// <param name="propertyName">The property name (without leading slash)</param>
    /// <returns>The partition key config options for method chaining</returns>
    IPartitionKeyConfigOptions<TModel> WithPropertyName(string propertyName);
    
    /// <summary>
    /// Sets the prefix to be applied to the partition key value
    /// </summary>
    /// <param name="prefix">The prefix (e.g., "TENANT", "USER")</param>
    /// <returns>The partition key config options for method chaining</returns>
    IPartitionKeyConfigOptions<TModel> WithPrefix(string prefix);
}
