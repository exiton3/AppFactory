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

    /// <summary>
    /// Uses the model mapping built once by <see cref="IModelMapRegistry"/> and shared by all repository instances.
    /// </summary>
    protected RepositoryBase(ICosmosDbClientFactory cosmosDbFactory, ILogger logger, IModelMapRegistry modelMapRegistry)
        : this(cosmosDbFactory, logger, modelMapRegistry.Get<TModel>())
    {
    }

    /// <summary>
    /// Builds the model mapping on every repository instance (i.e. on every request for scoped repositories).
    /// </summary>
    [Obsolete("Use the constructor taking IModelMapRegistry. It builds the model mapping once instead of on every repository instance.")]
    protected RepositoryBase(ICosmosDbClientFactory cosmosDbFactory, ILogger logger, IModelConfig<TModel> modelConfig)
        : this(cosmosDbFactory, logger, CosmosDbModelMap<TModel>.Create(modelConfig))
    {
    }

    private RepositoryBase(ICosmosDbClientFactory cosmosDbFactory, ILogger logger, CosmosDbModelMap<TModel> modelMap)
    {
        Logger = logger;
        _cosmosDbClient = cosmosDbFactory.CreateClient();
        Logger.LogTrace($"Repository {GetType().Name} #{GetHashCode()} created");

        _config = modelMap.Config;
        _containerName = _config.Container;
        _mapper = modelMap.Mapper;
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
        var response = await _cosmosDbClient.CreateItemAsync(document, _containerName);

        RefreshConcurrencyToken(model, response);
        return response.IsSuccess;
    }

    /// <exception cref="ConcurrencyConflictException">The model has a concurrency token and the stored document was changed since it was loaded.</exception>
    public async Task<bool> Update(TModel model)
    {
        var document = _mapper.MapToDocument(model);
        var partitionKey = _config.GetDocumentKey(model).ToPartitionKey();
        var response = await _cosmosDbClient.UpdateItemAsync(document, _containerName, partitionKey, _config.GetConcurrencyToken(model));

        EnsureNoConcurrencyConflict(document, response);
        RefreshConcurrencyToken(model, response);
        return response.IsSuccess;
    }

    /// <exception cref="ConcurrencyConflictException">The model has a concurrency token and the stored document was changed since it was loaded.
    /// A model without a token (e.g. a new one) is written unconditionally.</exception>
    public async Task<bool> Upsert(TModel model)
    {
        var document = _mapper.MapToDocument(model);
        var response = await _cosmosDbClient.UpsertItemAsync(document, _containerName, _config.GetConcurrencyToken(model));

        EnsureNoConcurrencyConflict(document, response);
        RefreshConcurrencyToken(model, response);
        return response.IsSuccess;
    }

    private void EnsureNoConcurrencyConflict(CosmosDbDocument document, CosmosDbClientResponse response)
    {
        if (response.IsPreconditionFailed)
        {
            throw new ConcurrencyConflictException(typeof(TModel).Name, document[CosmosDbConstants.Id]?.ToString() ?? string.Empty);
        }
    }

    // Keeps the instance usable for a further update after a successful write
    private void RefreshConcurrencyToken(TModel model, CosmosDbClientResponse response)
    {
        if (_config.HasConcurrencyToken && response.IsSuccess && response.ETag != null)
        {
            _config.SetConcurrencyToken(model, response.ETag);
        }
    }

    /// <remarks>Concurrency tokens are not checked for batch writes.</remarks>
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

        var response = await _cosmosDbClient.DeleteItemAsync(documentKey, _containerName, cancellationToken);

        return response.IsSuccess;
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

    protected async Task<IEnumerable<TModel>> Query(QueryDefinition queryDefinition, PartitionKey partitionKey)
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

    protected async Task<IEnumerable<TModel>> Query(string query, PartitionKey partitionKey)
        => await Query(new QueryDefinition(query), partitionKey);

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
