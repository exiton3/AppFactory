using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.AmazonDbServices;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.Models;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess;

public abstract class Repository2<TModel> : IDisposable, IRepository2<TModel> where TModel : class
{
    protected readonly ILogger Logger;
    protected string TableName;
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IDynamoDBClientFactory _dynamoDbFactory;
    private readonly JsonSerializerOptions _defaultOptions;
    private readonly DynamoDbModelConfig<TModel> _config;

    protected Repository2(IDynamoDBClientFactory dynamoDbFactory, IAWSSettings awsSettings,ILogger logger)
    {
        Logger = logger;
        _dynamoDbFactory = dynamoDbFactory;
        _dynamoDb = dynamoDbFactory.Create();
        TableName = awsSettings.GetTableName();
        Logger.LogTrace($"Repository {GetType().Name} #{GetHashCode()} created");

        _defaultOptions = GetJsonDefaultOptions();
         var builder = new DynamoDBModelConfigBuilder<TModel>();
         Configure(builder);
         _config = builder.Build();
    }

    private static JsonSerializerOptions GetJsonDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }


    protected abstract void Configure(DynamoDBModelConfigBuilder<TModel> builder);
    

    protected IAmazonDynamoDB DynamoDb => _dynamoDb ?? _dynamoDbFactory.Create();

    protected async Task<TModel?> GetByPrimaryKey(PrimaryKey primaryKey)
    {
        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = primaryKey.ToAttributeValues()
        };

        var response = await DynamoDb.GetItemAsync(request);

        if (response.Item.Count == 0)
        {
            return default;
        }

        return MapModelFromAttributes(response.Item);

    }

    public async Task<TModel?> GetById<TKey>(TKey key)
    {
       return await GetByPrimaryKey(_config.GetPrimaryKey(key));
    }

    protected async Task<bool> Insert(TModel model) 
    {
        var items = MapToAttributes(model);

        //var request = new TransactWriteItemsRequest();
        //request.TransactItems.Add(new TransactWriteItem{ Put = new Put {}});
        
           
        //await DynamoDb.TransactWriteItemsAsync(request);

        var response = await DynamoDb.PutItemAsync(new PutItemRequest { TableName = TableName, Item = items });

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> Add(TModel model) 
    {
        var items = MapToAttributes(model);

        var response = await DynamoDb.PutItemAsync(new PutItemRequest { TableName = TableName, Item = items });

        return response.HttpStatusCode == HttpStatusCode.OK;
    }


    public async Task<bool> Update(TModel model)
    {
        var items = MapToAttributes(model);

        var response = await DynamoDb.PutItemAsync(new PutItemRequest { TableName = TableName, Item = items });

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task BatchAddItems(IEnumerable<TModel> models)
    {
        var requests = new List<WriteRequest>();
        using (Logger.LogPerformance("Serialize requests"))
        {
            foreach (var item in models)
            {
                var attributeMap = MapToAttributes(item);

                var putRq = new PutRequest(attributeMap);

                requests.Add(new WriteRequest(putRq));
            }
        }

        int pageSize = 25;
        int itemsCount = requests.Count;
        int pageNumber = 0;

        do
        {
            var batchItems = requests.Skip(pageSize * pageNumber).Take(pageSize).ToList();

            var batchRq = new Dictionary<string, List<WriteRequest>> { { TableName, batchItems } };
            using (Logger.LogPerformance($"Batch number {pageNumber + 1}"))
            {
                var response = await DynamoDb.BatchWriteItemAsync(batchRq);
            }

            pageNumber++;

            itemsCount -= batchItems.Count;

        } while (itemsCount > 0);
    }

    public async Task<bool> Delete(PrimaryKey key, CancellationToken cancellationToken = default)
    {
        var delItemRq = new DeleteItemRequest
        {
            TableName = TableName,
            Key = key.ToAttributeValues()
        };
        var result = await DynamoDb.DeleteItemAsync(delItemRq, cancellationToken);

        return result.HttpStatusCode == HttpStatusCode.OK;
    }

    protected async Task<IEnumerable<TModel>> Query(Func<QueryRequest> queryRequestFactory, CancellationToken cancellationToken)
    {
        Dictionary<string, AttributeValue> lastKeyEvaluated = null;
        var models = new List<TModel>();
        int i = 0;
        do
        {
            var request = queryRequestFactory();

            request.ExclusiveStartKey = lastKeyEvaluated;
            QueryResponse? queryResponse;
           
            using (Logger.LogPerformance($"Get Query number {++i}"))
            {
                queryResponse = await DynamoDb.QueryAsync(request, cancellationToken);
                Logger.LogInfo($"Number of Items retrieved {queryResponse.Count} and size of {queryResponse.ContentLength}");
            }

            foreach (var item in queryResponse.Items)
            {
                var model = MapModelFromAttributes(item);

                models.Add(model);
            }

            lastKeyEvaluated = queryResponse.LastEvaluatedKey;

        } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0) ;

        return models;
    }

    private  TModel? MapModelFromAttributes(Dictionary<string, AttributeValue> item)
    {
        item.Remove(DynamoDBConstants.PK);
        item.Remove(DynamoDBConstants.SK);

        var itemAsDocument = Document.FromAttributeMap(item);

        var model = JsonSerializer.Deserialize<TModel>(itemAsDocument.ToJson(), _defaultOptions);

        return model;
    }

    private  Dictionary<string, AttributeValue> MapToAttributes(TModel item)
    {
        var modelJson = JsonSerializer.Serialize(item, _defaultOptions);
        var modelDoc = Document.FromJson(modelJson);

        var primaryKey = _config.GetPrimaryKey(item);

        var attributeMap = GetMergedAttributeValues(primaryKey, modelDoc);

        return attributeMap;
    }

    private static Dictionary<string, AttributeValue> GetMergedAttributeValues(PrimaryKey primaryKey, Document modelDoc)
    {
        var keyAttributes = primaryKey.ToAttributeValues();

        var attributeMap = modelDoc.ToAttributeMap();

        attributeMap = keyAttributes.Union(attributeMap).ToDictionary(k => k.Key, v => v.Value);
        return attributeMap;
    }

    public void Dispose()
    {
        Logger.LogTrace($"Disposed repository #{GetHashCode()}");
        DynamoDb.Dispose();
    }
}