using AppFactory.Framework.DataAccess.CosmosDB.Settings;
using AppFactory.Framework.Logging;
using Microsoft.Azure.Cosmos;

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
                    
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        _cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions
                        {
                            SerializerOptions = new CosmosSerializationOptions
                            {
                                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                            }
                        });
                    }
                    else
                    {
                        var endpoint = _settings.GetEndpoint();
                        var authKey = _settings.GetAuthKey();
                        
                        _cosmosClient = new CosmosClient(endpoint, authKey, new CosmosClientOptions
                        {
                            SerializerOptions = new CosmosSerializationOptions
                            {
                                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                            }
                        });
                    }

                    _logger.LogInfo("CosmosClient initialized");
                }
            }
        }

        return _cosmosClient;
    }
}
