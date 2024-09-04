using System.Net;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.Models;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess.AmazonDbServices;

public class DynamoDbClient : IDynamoDbClient
{
    private readonly ILogger _logger;
    private readonly AmazonDynamoDBClient _client;
    private readonly string _tableName;

    public DynamoDbClient(IAWSSettings settings, ILogger logger)
    {
        _logger = logger;
        _tableName = settings.GetTableName();
        _client = new AmazonDynamoDBClient(settings.GetAWSRegion());
    }

    public async Task<DynamoDbItem> GetByPrimaryKey(PrimaryKey primaryKey)
    {
        var request = new GetItemRequest
        {
            TableName = _tableName,
            Key = primaryKey.ToAttributeValues()
        };

        var response = await _client.GetItemAsync(request);

        if (response.Item.Count == 0)
        {
            return default;
        }

        return new DynamoDbItem(response.Item);
    }

    public async Task<bool> PutItemAsync(DynamoDbItem item)
    {
        var response = await _client.PutItemAsync(new PutItemRequest { TableName = _tableName, Item = item });

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeleteItemAsync(PrimaryKey key, CancellationToken cancellationToken = default)
    {
        var delItemRq = new DeleteItemRequest
        {
            TableName = _tableName,
            Key = key.ToAttributeValues()
        };
        var result = await _client.DeleteItemAsync(delItemRq, cancellationToken);

        return result.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task BatchWriteItemAsync(List<DynamoDbItem> items)
    {
        var requests = new List<WriteRequest>();

        foreach (var item in items)
        {
            var putRq = new PutRequest(item);

            requests.Add(new WriteRequest(putRq));
        }

        int pageSize = 25;
        int itemsCount = requests.Count;
        int pageNumber = 0;

        do
        {
            var batchItems = requests.Skip(pageSize * pageNumber).Take(pageSize).ToList();

            var batchRq = new Dictionary<string, List<WriteRequest>> { { _tableName, batchItems } };
            using (_logger.LogPerformance($"Batch number {pageNumber + 1}"))
            {
                var response = await _client.BatchWriteItemAsync(batchRq);
            }

            pageNumber++;

            itemsCount -= batchItems.Count;

        } while (itemsCount > 0);
    }

    public async Task<List<DynamoDbItem>> QueryAsync(QueryRequest queryRequest)
    {
        Dictionary<string, AttributeValue> lastKeyEvaluated = null;

        var items = new List<DynamoDbItem>();

        int i = 0;
        do
        {
            var request = queryRequest;

            request.ExclusiveStartKey = lastKeyEvaluated;
            QueryResponse? queryResponse;

            using (_logger.LogPerformance($"Get Query number {++i}"))
            {
                queryResponse = await _client.QueryAsync(request);
                _logger.LogInfo($"Number of Items retrieved {queryResponse.Count} and size of {queryResponse.ContentLength}");
            }

            foreach (var item in queryResponse.Items)
            {
                var model = new DynamoDbItem(item);

                items.Add(model);
            }

            lastKeyEvaluated = queryResponse.LastEvaluatedKey;

        } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0);

        return items;
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}