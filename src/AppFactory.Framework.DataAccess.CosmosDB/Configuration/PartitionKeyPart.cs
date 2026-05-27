namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Represents a single level in a hierarchical partition key configuration
/// </summary>
/// <typeparam name="TModel">The model type</typeparam>
public class PartitionKeyPart<TModel> where TModel : class
{
    /// <summary>
    /// Function to extract the partition key value from the model
    /// </summary>
    public Func<TModel, object> Selector { get; set; }
    
    /// <summary>
    /// The property name to use in the Cosmos DB document
    /// </summary>
    public string PropertyName { get; set; }
    
    /// <summary>
    /// Optional prefix to apply to the partition key value
    /// </summary>
    public string Prefix { get; set; }
}
