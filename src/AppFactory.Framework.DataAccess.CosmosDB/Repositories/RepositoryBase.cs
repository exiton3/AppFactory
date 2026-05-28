using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Mapping;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;

namespace AppFactory.Framework.DataAccess.CosmosDB.Repositories;

public abstract class RepositoryBase<TModel> : IDisposable, IRepository<TModel> where TModel : class
{
    protected readonly ILogger Logger;
    private readonly CosmosDbModelConfig<TModel> _config;
    private readonly ICosmosDbClient _cosmosDbClient;
    private readonly IModelMapper<TModel> _mapper;
    private readonly string _containerName;

    protected RepositoryBase(ICosmosDbClientFactory cosmosDbFactory, ILogger logger, IModelConfig<TModel> modelConfig)
    {
        Logger = logger;
        _cosmosDbClient = cosmosDbFactory.CreateClient();
        Logger.LogTrace($"Repository {GetType().Name} #{GetHashCode()} created");

        _config = new CosmosDbModelConfig<TModel>();
        modelConfig.Configure(_config);
        _containerName = _config.Container;
        _mapper = new ModelMapper<TModel>(_config);
    }

    protected async Task<TModel> GetByDocumentKey(DocumentKey documentKey)
    {
        var document = await _cosmosDbClient.GetByIdAsync(documentKey, _containerName);
        return document == null ? default : _mapper.MapFromDocument(document);
    }

    public async Task<TModel> GetById<TKey>(TKey key)
    {
        return await GetByDocumentKey(_config.GetDocumentKey(key));
    }

    public async Task<bool> Add(TModel model)
    {
        var document = _mapper.MapToDocument(model);

        return await _cosmosDbClient.CreateItemAsync(document, _containerName);
    }

    public async Task<bool> Update(TModel model)
    {
        var document = _mapper.MapToDocument(model);

        return await _cosmosDbClient.UpdateItemAsync(document, _containerName);
    }

    public async Task<bool> Upsert(TModel model)
    {
        var document = _mapper.MapToDocument(model);

        return await _cosmosDbClient.UpsertItemAsync(document, _containerName);
    }

    public async Task BatchAddItems(IEnumerable<TModel> models)
    {
        var documents = new List<CosmosDbDocument>();

        using (Logger.LogPerformance("Serialize models to documents"))
        {
            foreach (var model in models)
            {
                var document = _mapper.MapToDocument(model);
                documents.Add(document);
            }
        }

        using (Logger.LogPerformance("Batch write documents"))
        {
            await _cosmosDbClient.BatchUpsertItemsAsync(documents, _containerName);
        }
    }

    public async Task<bool> Delete<TKey>(TKey key, CancellationToken cancellationToken = default)
    {
        var documentKey = _config.GetDocumentKey(key);

        return await _cosmosDbClient.DeleteItemAsync(documentKey, _containerName, cancellationToken);
    }

    protected async Task<IEnumerable<TModel>> Query(string query, string partitionKey = null)
    {
        var documents = await _cosmosDbClient.QueryAsync(query, _containerName, partitionKey);
        var models = new List<TModel>();

        foreach (var document in documents)
        {
            var model = _mapper.MapFromDocument(document);
            models.Add(model);
        }

        return models;
    }

    protected async Task<IEnumerable<TModel>> Query(QueryDefinition queryDefinition, string partitionKey = null)
    {
        var documents = await _cosmosDbClient.QueryAsync(queryDefinition, _containerName, partitionKey);
        var models = new List<TModel>();

        foreach (var document in documents)
        {
            var model = _mapper.MapFromDocument(document);
            models.Add(model);
        }

        return models;
    }

    protected async Task<TModel> QuerySingle(string query, string partitionKey = null)
    {
        var results = await Query(query, partitionKey);
        return results.FirstOrDefault();
    }

    protected async Task<TModel> QuerySingle(QueryDefinition queryDefinition, string partitionKey = null)
    {
        var results = await Query(queryDefinition, partitionKey);
        return results.FirstOrDefault();
    }

    protected QueryDefinition CreateQuery(string query)
    {
        return new QueryDefinition(query);
    }

    public void Dispose()
    {
        _cosmosDbClient?.Dispose();
    }
}
