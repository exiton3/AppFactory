using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

/// <summary>
/// Represents a document key with support for hierarchical partition keys (up to 3 levels)
/// </summary>
public class DocumentKey
{
    public string Id { get; set; }

    /// <summary>
    /// Hierarchical partition key values (supports 1-3 levels)
    /// Single partition key is represented as a list with one value
    /// </summary>
    public List<string> PartitionKeyValues { get; set; } = new List<string>();

    /// <summary>
    /// Converts the partition key values to Cosmos DB PartitionKey
    /// Supports both single and hierarchical partition keys
    /// </summary>
    public PartitionKey ToPartitionKey()
    {
        var builder = new PartitionKeyBuilder();

        if (PartitionKeyValues != null && PartitionKeyValues.Any())
        {
            foreach (var value in PartitionKeyValues)
            {
                builder.Add(value);
            }
        }

        return builder.Build();
    }
}

