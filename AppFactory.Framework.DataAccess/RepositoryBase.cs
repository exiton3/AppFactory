using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.AmazonDbServices;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.Mappers;
using AppFactory.Framework.DataAccess.Models;
using AppFactory.Framework.Domain.Entities;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess;

public abstract class RepositoryBase<TEntity,TModel> : IRepositoryBase<TEntity> where TEntity : Entity where TModel : ModelBase
{
    protected readonly ILogger Logger;
    protected string TableName;
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IDynamoDBClientFactory _dynamoDbFactory;
    protected readonly IModelMapper<TEntity, TModel> Mapper;
    protected RepositoryBase(IDynamoDBClientFactory dynamoDbFactory, IAWSSettings awsSettings,ILogger logger, IModelMapper<TEntity, TModel> mapper)
    {
        Logger = logger;
        Mapper = mapper;
        _dynamoDbFactory = dynamoDbFactory;
        _dynamoDb = dynamoDbFactory.Create();
        TableName = awsSettings.GetTableName();
        Logger.LogTrace($"Repository {GetType().Name} #{GetHashCode()} created");
    }


    public async Task Add(TEntity entity, CancellationToken cancellationToken = default)
    {
        var model = Mapper.MapToModel(entity);

        await Insert(model);

        Logger.LogInfo($@"The {entity.GetType().Name} with id #{entity.Id} added to repository");
    }

    public async Task<bool> Update(TEntity entity, CancellationToken cancellation = default)
    {
        var model = Mapper.MapToModel(entity);

        var result = await Update(model);

        Logger.LogInfo($"{entity.GetType().Name} #{entity.Id} updated in repository");

        return result;
    }

    public async Task<TEntity> GetById(string id)
    {
        var model = await GetByPrimaryKey<TModel>(GetPrimaryKey(id));

        if (model is null)
        {
            Logger.LogInfo($"{typeof(TEntity).Name} with Id {id} not found");

            return null;
        }

        return Mapper.MapFromModel(model);
    }

    public  Task Add(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var models = entities.Select(x => Mapper.MapToModel(x)).ToList();

       return BatchAddItemsAsync(models);
    }


    public async Task<bool> Delete(string id)
    {
        return await Delete(GetPrimaryKey(id));
    }
    protected abstract PrimaryKey GetPrimaryKey(string id);
    

    protected IAmazonDynamoDB DynamoDb => _dynamoDb ?? _dynamoDbFactory.Create();

    protected async Task<TModel?> GetByPrimaryKey<TModel>(PrimaryKey primaryKey) where TModel : ModelBase
    {
        //  Query.GetItem.From(TableName).PrimaryKey(primaryKey);

        var request = new GetItemRequest
        {
            TableName = TableName,
            Key = primaryKey.ToAttributeValues()
        };

        var response = await DynamoDb.GetItemAsync(request);

        if (response.Item.Count == 0)
        {
            return null;
        }

        return MapModelFromAttributes<TModel>(response.Item);

    }

    protected async Task<bool> Insert<TModel>(TModel model) where TModel : ModelBase
    {
        var items = MapToAttributes(model);

        //var request = new TransactWriteItemsRequest();
        //request.TransactItems.Add(new TransactWriteItem{ Put = new Put {}});
        
           
        //await DynamoDb.TransactWriteItemsAsync(request);

        var response = await DynamoDb.PutItemAsync(new PutItemRequest { TableName = TableName, Item = items });

        return response.HttpStatusCode == HttpStatusCode.OK;
    }


    protected async Task<bool> Update<TModel>(TModel model) where TModel : ModelBase
    {
        var items = MapToAttributes(model);

        var response = await DynamoDb.PutItemAsync(new PutItemRequest { TableName = TableName, Item = items });

        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    protected async Task BatchAddItemsAsync<TModel>(IEnumerable<TModel> models) where TModel : ModelBase
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

    protected async Task<bool> Delete(PrimaryKey key, CancellationToken cancellationToken = default)
    {
        var delItemRq = new DeleteItemRequest
        {
            TableName = TableName,
            Key = key.ToAttributeValues()
        };
        var result = await DynamoDb.DeleteItemAsync(delItemRq, cancellationToken);

        return result.HttpStatusCode == HttpStatusCode.OK;
    }

    protected async Task<IEnumerable<TModel>> Query<TModel>(Func<QueryRequest> queryRequestFactory, CancellationToken cancellationToken) where TModel : ModelBase
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
                var model = MapModelFromAttributes<TModel>(item);

                models.Add(model);
            }

            lastKeyEvaluated = queryResponse.LastEvaluatedKey;

        } while (lastKeyEvaluated != null && lastKeyEvaluated.Count != 0) ;

        return models;
    }

    private static TModel? MapModelFromAttributes<TModel>(Dictionary<string, AttributeValue> item) where TModel : ModelBase
    {
        var itemAsDocument = Document.FromAttributeMap(item);

        var model = JsonSerializer.Deserialize<TModel>(itemAsDocument.ToJson());

        return model;
    }

    private static Dictionary<string, AttributeValue> MapToAttributes<TModel>(TModel item) where TModel : ModelBase
    {
        var modelJson = JsonSerializer.Serialize(item);
        var modelDoc = Document.FromJson(modelJson);

        var attributeMap = modelDoc.ToAttributeMap();

        return attributeMap;
    }

    public void Dispose()
    {
        Logger.LogTrace($"Disposed repository #{GetHashCode()}");
        DynamoDb.Dispose();
    }
}