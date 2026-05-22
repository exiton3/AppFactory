using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

public interface ICosmosDbClient : IDisposable
{
    Task<CosmosDbDocument> GetByIdAsync(DocumentKey documentKey, string containerName);
    Task<bool> UpsertItemAsync(CosmosDbDocument document, string containerName);
    Task<bool> CreateItemAsync(CosmosDbDocument document, string containerName);
    Task<bool> UpdateItemAsync(CosmosDbDocument document, string containerName);
    Task<bool> DeleteItemAsync(DocumentKey documentKey, string containerName, CancellationToken cancellationToken = default);
    Task BatchUpsertItemsAsync(List<CosmosDbDocument> documents, string containerName);
    Task<List<CosmosDbDocument>> QueryAsync(QueryDefinition queryDefinition, string containerName, string partitionKey = null);
    Task<List<CosmosDbDocument>> QueryAsync(string query, string containerName, string partitionKey = null);
}
