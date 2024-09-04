using Amazon.DynamoDBv2.Model;
using AppFactory.Framework.DataAccess.AmazonDbServices;
using AppFactory.Framework.DataAccess.Configuration;
using AppFactory.Framework.DataAccess.Mapping;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.DataAccess.Repositories;

public abstract class RepositoryBase<TModel> : IDisposable, IRepository<TModel> where TModel : class
{
    protected readonly ILogger Logger;
    private readonly DynamoDbModelConfig<TModel> _config;
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly IModelMapper<TModel> _mapper;

    protected RepositoryBase(IDynamoDBClientFactory dynamoDbFactory, ILogger logger, IModelConfig<TModel> modelConfig)
    {
        Logger = logger;
        _dynamoDbClient = dynamoDbFactory.CreateClient();
        Logger.LogTrace($"Repository {GetType().Name} #{GetHashCode()} created");
        _config = new DynamoDbModelConfig<TModel>();
        modelConfig.Configure(_config);
        _mapper = new ModelMapper<TModel>(_config);
    }

    protected async Task<TModel> GetByPrimaryKey(PrimaryKey primaryKey)
    {
        var item = await _dynamoDbClient.GetByPrimaryKey(primaryKey);

        return item == default ? default : _mapper.MapModelFromAttributes(item);
    }

    public async Task<TModel> GetById<TKey>(TKey key)
    {
        return await GetByPrimaryKey(_config.GetPrimaryKey(key));
    }

    protected async Task<bool> Insert(TModel model)
    {
        var items = _mapper.MapToAttributes(model);

        var response = await _dynamoDbClient.PutItemAsync(items);

        return response;
    }

    public async Task<bool> Add(TModel model)
    {
        var items = _mapper.MapToAttributes(model);

        var response = await _dynamoDbClient.PutItemAsync(items);

        return response;
    }

    public async Task<bool> Update(TModel model)
    {
        var items = _mapper.MapToAttributes(model);

        var response = await _dynamoDbClient.PutItemAsync(items);

        return response;
    }

    public async Task BatchAddItems(IEnumerable<TModel> models)
    {
        var dynamoDbItems = new List<DynamoDbItem>();

        using (Logger.LogPerformance("Serialize requests"))
        {
            foreach (var model in models)
            {
                var item = _mapper.MapToAttributes(model);
                dynamoDbItems.Add(item);
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
            var model = _mapper.MapModelFromAttributes(item);

            models.Add(model);
        }

        return models;
    }

    public void Dispose()
    {
        Logger.LogTrace($"Disposed repository #{GetHashCode()}");
        _dynamoDbClient.Dispose();
    }
}