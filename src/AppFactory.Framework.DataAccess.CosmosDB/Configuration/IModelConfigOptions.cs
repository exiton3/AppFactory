namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public interface IModelConfigOptions<TModel> where TModel : class
{
    IModelConfigOptions<TModel> ContainerName(string containerName);
    IModelConfigOptions<TModel> PartitionKeyPath(string partitionKeyPath);
    IModelConfigOptions<TModel> PartitionKeyPrefix(string prefix);
    IModelConfigOptions<TModel> IdPrefix(string prefix);
    IModelConfigOptions<TModel> PartitionKey<TKey>(Func<TModel, TKey> partitionKeySelector);
    IModelConfigOptions<TModel> Id<TKey>(Func<TModel, TKey> idSelector);
    IModelConfigOptions<TModel> TimeToLive(int? ttlInSeconds);
}
