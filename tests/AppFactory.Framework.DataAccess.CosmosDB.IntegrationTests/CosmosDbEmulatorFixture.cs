using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Settings;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Testcontainers.CosmosDb;
using Xunit;

namespace AppFactory.Framework.DataAccess.CosmosDB.IntegrationTests;

/// <summary>
/// Starts one Cosmos DB emulator container for all integration tests (requires Docker).
/// </summary>
public sealed class CosmosDbEmulatorFixture : IAsyncLifetime
{
    public const string DatabaseName = "appfactory-integration";

    // The vNext emulator starts in seconds. Set COSMOS_EMULATOR_IMAGE to run against another emulator image.
    private readonly CosmosDbContainer _container = new CosmosDbBuilder(
            Environment.GetEnvironmentVariable("COSMOS_EMULATOR_IMAGE")
            ?? "mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview")
        .Build();

    public CosmosClient CosmosClient { get; private set; } = null!;

    public Database Database { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        // Same serializer as CosmosDbClientFactory; gateway mode through the container's HttpClient (emulator certificate)
        CosmosClient = new CosmosClient(_container.GetConnectionString(), new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            LimitToEndpoint = true,
            HttpClientFactory = () => _container.HttpClient,
            Serializer = new CosmosNewtonsoftJsonSerializer(new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Formatting = Formatting.None
            })
        });

        Database = (await CosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName)).Database;
    }

    public async ValueTask DisposeAsync()
    {
        CosmosClient?.Dispose();
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a container with a unique name so tests don't see each other's documents.
    /// </summary>
    public async Task<Container> CreateContainerAsync(string partitionKeyPath)
    {
        var response = await Database.CreateContainerAsync($"c-{Guid.NewGuid():N}", partitionKeyPath);
        return response.Container;
    }

    /// <summary>
    /// A repository wired like production (ModelMapRegistry, CosmosDbClient) against the emulator.
    /// </summary>
    public TRepository CreateRepository<TRepository, TModel>(IModelConfig<TModel> modelConfig)
        where TModel : class
        where TRepository : class
    {
        var services = new ServiceCollection();
        services.RegisterCosmosDbPersistence();

        // Last registration wins: point the client factory at the emulator
        services
            .AddSingleton<ICosmosDbClientFactory>(new EmulatorClientFactory(CosmosClient))
            .AddSingleton<ILogger, NullLogger>()
            .AddSingleton(modelConfig)
            .AddTransient<TRepository>();

        return services.BuildServiceProvider().GetRequiredService<TRepository>();
    }

    private sealed class EmulatorClientFactory(CosmosClient cosmosClient) : ICosmosDbClientFactory
    {
        public ICosmosDbClient CreateClient() => new CosmosDbClient(cosmosClient, new EmulatorSettings(), new NullLogger());

        public CosmosClient GetCosmosClient() => cosmosClient;
    }

    private sealed class EmulatorSettings : ICosmosDbSettings
    {
        public string GetConnectionString() => string.Empty;
        public string GetDatabaseName() => DatabaseName;
        public string GetEndpoint() => string.Empty;
        public string GetAuthKey() => string.Empty;
    }
}

[CollectionDefinition(Name)]
public sealed class CosmosDbEmulatorCollection : ICollectionFixture<CosmosDbEmulatorFixture>
{
    public const string Name = "Cosmos DB emulator";
}

internal sealed class NullLogger : ILogger
{
    public void AddTraceId(string traceId) { }
    public void LogInfo(string message) { }
    public void LogTrace(string message) { }
    public ITimeLogger LogPerformance(string message) => new NullTimeLogger();
    public void LogError(Exception exception, string messageTemplate) { }
    public void LogInfo(string messageTemplate, params object[] propertyValues) { }
    public void LogDebug(string context, object value, string message, params object[] propertyValues) { }
    public void LogDebug(string context, object value, string message) { }
    public void SetContext(string context) { }
    public void LogDebug(string messageTemplate, params object[] values) { }
    public void LogTrace(string message, params object[] propertyValues) { }
    public void LogError(Exception exception, string messageTemplate, params object[] values) { }
    public void LogError(string messageTemplate, params object[] values) { }

    private sealed class NullTimeLogger : ITimeLogger
    {
        public void Dispose() { }
    }
}
