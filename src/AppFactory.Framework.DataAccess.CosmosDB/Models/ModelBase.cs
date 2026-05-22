namespace AppFactory.Framework.DataAccess.CosmosDB.Models;

public abstract class ModelBase
{
    public string Id { get; set; }
    public string PartitionKey { get; set; }
}
