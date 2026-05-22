using AppFactory.Framework.Shared.Config;

namespace AppFactory.Framework.DataAccess.CosmosDB.Settings;

public class CosmosDbSettings : ICosmosDbSettings
{
    private readonly IConfigSettings _config;

    public CosmosDbSettings(IConfigSettings config)
    {
        _config = config;
    }

    public string GetConnectionString()
    {
        return _config.GetValue("CosmosDb:ConnectionString");
    }

    public string GetDatabaseName()
    {
        return _config.GetValue("CosmosDb:DatabaseName");
    }

    public string GetEndpoint()
    {
        return _config.GetValue("CosmosDb:Endpoint");
    }

    public string GetAuthKey()
    {
        return _config.GetValue("CosmosDb:AuthKey");
    }
}
