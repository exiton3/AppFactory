using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.Configuration;

public class DocumentKey
{
    public string Id { get; set; }
    public string PartitionKey { get; set; }

    public PartitionKey ToPartitionKey()
    {
        var builder = new PartitionKeyBuilder();
        builder.Add(PartitionKey);

        return builder.Build();
    }
}
