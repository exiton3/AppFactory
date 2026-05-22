using System.Net;
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.Settings;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

public class CosmosDbClient : ICosmosDbClient
{
    private readonly ILogger _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly string _databaseName;

    public CosmosDbClient(CosmosClient cosmosClient, ICosmosDbSettings settings, ILogger logger)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
        _databaseName = settings.GetDatabaseName();
    }

    public async Task<CosmosDbDocument> GetByIdAsync(DocumentKey documentKey, string containerName)
    {
        try
        {
            var container = _cosmosClient.GetContainer(_databaseName, containerName);
            var response = await container.ReadItemAsync<Dictionary<string, object>>(
                documentKey.Id,
                documentKey.ToPartitionKey());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new CosmosDbDocument(response.Resource);
            }

            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> UpsertItemAsync(CosmosDbDocument document, string containerName)
    {
        var container = _cosmosClient.GetContainer(_databaseName, containerName);
        var response = await container.UpsertItemAsync(document);
        
        return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<bool> CreateItemAsync(CosmosDbDocument document, string containerName)
    {
        var container = _cosmosClient.GetContainer(_databaseName, containerName);
        var response = await container.CreateItemAsync(document);
        
        return response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<bool> UpdateItemAsync(CosmosDbDocument document, string containerName)
    {
        if (!document.TryGetValue(CosmosDbConstants.Id, out var id) || 
            !document.TryGetValue(CosmosDbConstants.PartitionKey, out var partitionKey))
        {
            throw new ArgumentException("Document must contain id and partitionKey properties");
        }

        var container = _cosmosClient.GetContainer(_databaseName, containerName);
        var response = await container.ReplaceItemAsync(
            document,
            id.ToString(),
            new PartitionKey(partitionKey.ToString()));
        
        return response.StatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeleteItemAsync(DocumentKey documentKey, string containerName, CancellationToken cancellationToken = default)
    {
        var container = _cosmosClient.GetContainer(_databaseName, containerName);
        var response = await container.DeleteItemAsync<Dictionary<string, object>>(
            documentKey.Id,
            documentKey.ToPartitionKey(),
            cancellationToken: cancellationToken);
        
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public async Task BatchUpsertItemsAsync(List<CosmosDbDocument> documents, string containerName)
    {
        var container = _cosmosClient.GetContainer(_databaseName, containerName);
        
        // Group documents by partition key for batch operations
        var groupedDocuments = documents.GroupBy(d => d[CosmosDbConstants.PartitionKey].ToString());

        foreach (var group in groupedDocuments)
        {
            var partitionKey = new PartitionKey(group.Key);
            var batch = container.CreateTransactionalBatch(partitionKey);

            foreach (var document in group)
            {
                batch.UpsertItem(document);
            }

            using (_logger.LogPerformance($"Batch upsert for partition key: {group.Key}"))
            {
                var response = await batch.ExecuteAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(new Exception($"StatusCode: {response.StatusCode}"), $"Batch operation failed with status: {response.StatusCode}");
                }
            }
        }
    }

    public async Task<List<CosmosDbDocument>> QueryAsync(QueryDefinition queryDefinition, string containerName, string partitionKey = null)
    {
        var container = _cosmosClient.GetContainer(_databaseName, containerName);
        var documents = new List<CosmosDbDocument>();

        var queryRequestOptions = partitionKey != null 
            ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
            : null;

        using var feedIterator = container.GetItemQueryIterator<Dictionary<string, object>>(
            queryDefinition,
            requestOptions: queryRequestOptions);

        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync();
            
            foreach (var item in response)
            {
                documents.Add(new CosmosDbDocument(item));
            }
        }

        return documents;
    }

    public async Task<List<CosmosDbDocument>> QueryAsync(string query, string containerName, string partitionKey = null)
    {
        var queryDefinition = new QueryDefinition(query);
        return await QueryAsync(queryDefinition, containerName, partitionKey);
    }

    public void Dispose()
    {
        // CosmosClient is managed by the factory as a singleton
        // No need to dispose here
    }
}
