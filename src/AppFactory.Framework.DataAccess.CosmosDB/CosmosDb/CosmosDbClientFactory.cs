using AppFactory.Framework.DataAccess.CosmosDB.Settings;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;

public class CosmosDbClientFactory : ICosmosDbClientFactory
{
    private readonly ICosmosDbSettings _settings;
    private readonly ILogger _logger;
    private static CosmosClient _cosmosClient;
    private static readonly object _lock = new object();

    public CosmosDbClientFactory(ICosmosDbSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public ICosmosDbClient CreateClient()
    {
        return new CosmosDbClient(GetCosmosClient(), _settings, _logger);
    }

    public CosmosClient GetCosmosClient()
    {
        if (_cosmosClient == null)
        {
            lock (_lock)
            {
                if (_cosmosClient == null)
                {
                    var connectionString = _settings.GetConnectionString();

                    // Configure to use Newtonsoft.Json serializer to handle Dictionary<string, object> properly
                    var clientOptions = new CosmosClientOptions
                    {
                        Serializer = new CosmosNewtonsoftJsonSerializer(new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver(),
                            NullValueHandling = NullValueHandling.Ignore,
                            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                            Formatting = Formatting.None
                        })
                    };

                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        _cosmosClient = new CosmosClient(connectionString, clientOptions);
                    }
                    else
                    {
                        var endpoint = _settings.GetEndpoint();
                        var authKey = _settings.GetAuthKey();

                        _cosmosClient = new CosmosClient(endpoint, authKey, clientOptions);
                    }

                    _logger.LogInfo("CosmosClient initialized with Newtonsoft.Json serializer");
                }
            }
        }

        return _cosmosClient;
    }
}
