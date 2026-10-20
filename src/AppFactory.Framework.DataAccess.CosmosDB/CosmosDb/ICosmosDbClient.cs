using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

public interface ICosmosDbClient : IDisposable
{
    Task<CosmosDbDocument> GetByIdAsync(DocumentKey documentKey, string containerName);

    /// <param name="ifMatchEtag">When set, the write only succeeds if the stored document still has this ETag;
    /// otherwise the response has <see cref="CosmosDbClientResponse.IsPreconditionFailed"/>.</param>
    Task<CosmosDbClientResponse> UpsertItemAsync(CosmosDbDocument document, string containerName, string? ifMatchEtag = null);
    Task<CosmosDbClientResponse> CreateItemAsync(CosmosDbDocument document, string containerName);

    /// <param name="ifMatchEtag">When set, the write only succeeds if the stored document still has this ETag;
    /// otherwise the response has <see cref="CosmosDbClientResponse.IsPreconditionFailed"/>.</param>
    Task<CosmosDbClientResponse> UpdateItemAsync(CosmosDbDocument document, string containerName, string? ifMatchEtag = null);

    /// <param name="ifMatchEtag">When set, the write only succeeds if the stored document still has this ETag;
    /// otherwise the response has <see cref="CosmosDbClientResponse.IsPreconditionFailed"/>.</param>
    Task<CosmosDbClientResponse> UpdateItemAsync(CosmosDbDocument document, string containerName, PartitionKey partitionKey, string? ifMatchEtag = null);
    Task<CosmosDbClientResponse> DeleteItemAsync(DocumentKey documentKey, string containerName, CancellationToken cancellationToken = default);
    Task BatchUpsertItemsAsync(List<CosmosDbDocument> documents, string containerName);
    Task<List<CosmosDbDocument>> QueryAsync(QueryDefinition queryDefinition, string containerName, string partitionKey = null);
    Task<List<CosmosDbDocument>> QueryAsync(string query, string containerName, string partitionKey = null);
    Task<List<CosmosDbDocument>> QueryAsync(QueryDefinition queryDefinition, string containerName, PartitionKey partitionKey);
    Task<List<CosmosDbDocument>> QueryAsync(string query, string containerName, PartitionKey partitionKey);
}
