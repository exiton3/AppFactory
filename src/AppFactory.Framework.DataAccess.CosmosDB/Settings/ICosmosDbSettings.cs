namespace AppFactory.Framework.DataAccess.CosmosDB.Settings;

public interface ICosmosDbSettings
{
    string GetConnectionString();
    string GetDatabaseName();
    string GetEndpoint();
    string GetAuthKey();
}
