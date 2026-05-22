using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

public interface ICosmosDbClientFactory
{
    ICosmosDbClient CreateClient();
    CosmosClient GetCosmosClient();
}
