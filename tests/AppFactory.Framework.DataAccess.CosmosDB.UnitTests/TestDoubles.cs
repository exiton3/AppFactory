using System.Net;
using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.UnitTests
{
    /// <summary>
    /// In-memory stand-in for Cosmos DB: documents go through JSON like real storage, every write assigns
    /// a new _etag, and If-Match conditions fail with 412 like the real service.
    /// </summary>
    internal sealed class InMemoryCosmosDbClient : ICosmosDbClient
    {
        private readonly Dictionary<string, CosmosDbDocument> _documents = new Dictionary<string, CosmosDbDocument>();
        private int _version;

        public List<CosmosDbDocument> Created { get; } = new List<CosmosDbDocument>();
        public string? LastContainer { get; private set; }
        public List<string?> IfMatchEtags { get; } = new List<string?>();

        /// <summary>The document exactly as stored, including _etag.</summary>
        public CosmosDbDocument Stored(string documentId) => Clone(_documents[documentId]);

        public Task<CosmosDbDocument> GetByIdAsync(DocumentKey documentKey, string containerName)
        {
            LastContainer = containerName;
            return Task.FromResult(_documents.TryGetValue(documentKey.Id, out var document) ? Clone(document) : null!);
        }

        public Task<CosmosDbClientResponse> CreateItemAsync(CosmosDbDocument document, string containerName)
        {
            LastContainer = containerName;
            Created.Add(document);

            var id = Id(document);
            if (_documents.ContainsKey(id))
            {
                return Task.FromResult(new CosmosDbClientResponse(HttpStatusCode.Conflict));
            }

            return Task.FromResult(Store(document, HttpStatusCode.Created));
        }

        public Task<CosmosDbClientResponse> UpdateItemAsync(CosmosDbDocument document, string containerName, string? ifMatchEtag = null)
            => UpdateItemAsync(document, containerName, PartitionKey.None, ifMatchEtag);

        public Task<CosmosDbClientResponse> UpdateItemAsync(CosmosDbDocument document, string containerName, PartitionKey partitionKey, string? ifMatchEtag = null)
        {
            LastContainer = containerName;
            IfMatchEtags.Add(ifMatchEtag);

            if (!_documents.TryGetValue(Id(document), out var stored))
            {
                return Task.FromResult(new CosmosDbClientResponse(HttpStatusCode.NotFound));
            }

            if (ifMatchEtag != null && ifMatchEtag != stored[CosmosDbConstants.ETag].ToString())
            {
                return Task.FromResult(new CosmosDbClientResponse(HttpStatusCode.PreconditionFailed));
            }

            return Task.FromResult(Store(document, HttpStatusCode.OK));
        }

        public Task<CosmosDbClientResponse> UpsertItemAsync(CosmosDbDocument document, string containerName, string? ifMatchEtag = null)
        {
            LastContainer = containerName;
            IfMatchEtags.Add(ifMatchEtag);

            var exists = _documents.TryGetValue(Id(document), out var stored);
            if (ifMatchEtag != null && (!exists || ifMatchEtag != stored![CosmosDbConstants.ETag].ToString()))
            {
                return Task.FromResult(new CosmosDbClientResponse(HttpStatusCode.PreconditionFailed));
            }

            return Task.FromResult(Store(document, exists ? HttpStatusCode.OK : HttpStatusCode.Created));
        }

        public Task<CosmosDbClientResponse> DeleteItemAsync(DocumentKey documentKey, string containerName, CancellationToken cancellationToken = default)
        {
            LastContainer = containerName;
            return Task.FromResult(new CosmosDbClientResponse(_documents.Remove(documentKey.Id) ? HttpStatusCode.NoContent : HttpStatusCode.NotFound));
        }

        public Task<List<CosmosDbDocument>> QueryAsync(string query, string containerName, string? partitionKey = null)
        {
            LastContainer = containerName;
            return Task.FromResult(_documents.Values.Select(Clone).ToList());
        }

        public Task<List<CosmosDbDocument>> QueryAsync(QueryDefinition queryDefinition, string containerName, string? partitionKey = null)
            => QueryAsync(queryDefinition.QueryText, containerName, partitionKey);

        public Task<List<CosmosDbDocument>> QueryAsync(QueryDefinition queryDefinition, string containerName, PartitionKey partitionKey)
            => QueryAsync(queryDefinition.QueryText, containerName);

        public Task<List<CosmosDbDocument>> QueryAsync(string query, string containerName, PartitionKey partitionKey)
            => QueryAsync(query, containerName);

        public Task BatchUpsertItemsAsync(List<CosmosDbDocument> documents, string containerName) => throw new NotSupportedException();

        public void Dispose() { }

        private CosmosDbClientResponse Store(CosmosDbDocument document, HttpStatusCode statusCode)
        {
            var eTag = $"\"etag-{++_version}\"";
            var stored = Clone(document);
            stored[CosmosDbConstants.ETag] = eTag;
            _documents[Id(document)] = stored;

            return new CosmosDbClientResponse(statusCode, eTag);
        }

        private static string Id(CosmosDbDocument document) => document[CosmosDbConstants.Id].ToString()!;

        private static CosmosDbDocument Clone(CosmosDbDocument document) => new CosmosDbDocument(document.ToJson());
    }

    internal sealed class InMemoryCosmosDbClientFactory : ICosmosDbClientFactory
    {
        public InMemoryCosmosDbClient Client { get; } = new InMemoryCosmosDbClient();

        public ICosmosDbClient CreateClient() => Client;

        public CosmosClient GetCosmosClient() => throw new NotSupportedException();
    }

    internal sealed class NullLogger : ILogger
    {
        public void AddTraceId(string traceId) { }
        public void LogInfo(string message) { }
        public void LogTrace(string message) { }
        public ITimeLogger LogPerformance(string message) => throw new NotSupportedException();
        public void LogError(Exception exception, string messageTemplate) { }
        public void LogInfo(string messageTemplate, params object[] propertyValues) { }
        public void LogDebug(string context, object value, string message, params object[] propertyValues) { }
        public void LogDebug(string context, object value, string message) { }
        public void SetContext(string context) { }
        public void LogDebug(string messageTemplate, params object[] values) { }
        public void LogTrace(string message, params object[] propertyValues) { }
        public void LogError(Exception exception, string messageTemplate, params object[] values) { }
        public void LogError(string messageTemplate, params object[] values) { }
    }
}
