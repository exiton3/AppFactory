using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.AmazonDbServices;
using AppFactory.Framework.DataAccess.Models;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess;

public abstract class RepositoryBase<TModel> : IDisposable, IRepository<TModel> where TModel : class
{
    protected readonly ILogger Logger;
    private readonly JsonSerializerOptions _defaultOptions;
    private readonly DynamoDbModelConfig<TModel> _config;
    private readonly IDynamoDbClient _dynamoDbClient;

    protected RepositoryBase(IDynamoDBClientFactory dynamoDbFactory,ILogger logger, IModelConfig<TModel> modelConfig)
    {
        Logger = logger;
        _dynamoDbClient = dynamoDbFactory.CreateClient();
        Logger.LogTrace($"Repository {GetType().Name} #{GetHashCode()} created");

        _defaultOptions = GetJsonDefaultOptions();
         _config = new DynamoDbModelConfig<TModel>();
         modelConfig.Configure(_config);
         
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


    protected async Task<TModel?> GetByPrimaryKey(PrimaryKey primaryKey)
    {
        var item = await _dynamoDbClient.GetByPrimaryKey(primaryKey);

        return item == default ? default : MapModelFromAttributes(item);
    }

    public async Task<TModel?> GetById<TKey>(TKey key)
    {
       return await GetByPrimaryKey(_config.GetPrimaryKey(key));
    }

    protected async Task<bool> Insert(TModel model) 
    {
        var items = MapToAttributes(model);

        var response = await _dynamoDbClient.PutItemAsync( new DynamoDbItem(items));

        return response;
    }

    public async Task<bool> Add(TModel model)
    {
        var items = MapToAttributes(model);

        var response = await _dynamoDbClient.PutItemAsync(new DynamoDbItem(items));

        return response;
    }


    public async Task<bool> Update(TModel model)
    {
        var items = MapToAttributes(model);

        var response = await _dynamoDbClient.PutItemAsync(new DynamoDbItem(items));

        return response;
    }

    public async Task BatchAddItems(IEnumerable<TModel> models)
    {
        var dynamoDbItems = new List<DynamoDbItem>();

        using (Logger.LogPerformance("Serialize requests"))
        {
            foreach (var model in models)
            {
                var attributeMap = MapToAttributes(model);
                var dynamoDbItem = new DynamoDbItem(attributeMap);
                dynamoDbItems.Add(dynamoDbItem);
            }
        }

        using (Logger.LogPerformance("Batches Write requests"))
        {
            await _dynamoDbClient.BatchWriteItemAsync(dynamoDbItems);
        }
    }

    public async Task<bool> Delete(PrimaryKey key, CancellationToken cancellationToken = default)
    {
        return await _dynamoDbClient.DeleteItemAsync(key, cancellationToken);
    }

    protected async Task<IEnumerable<TModel>> Query(Func<QueryRequest> queryRequestFactory, CancellationToken cancellationToken)
    {
        var items = await _dynamoDbClient.QueryAsync(queryRequestFactory());

        var models = new List<TModel>();

        foreach (var item in items)
        {
            var model = MapModelFromAttributes(item);

            models.Add(model);
        }

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
        _dynamoDbClient.Dispose();
    }
}