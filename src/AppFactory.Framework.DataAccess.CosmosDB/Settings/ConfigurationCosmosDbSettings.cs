using Microsoft.Extensions.Configuration;

namespace AppFactory.Framework.DataAccess.CosmosDB.Settings;

internal sealed class ConfigurationCosmosDbSettings(IConfiguration configuration) : ICosmosDbSettings
{
    public string GetConnectionString() => configuration["CosmosDb:ConnectionString"] ?? string.Empty;

    public string GetDatabaseName() => configuration["CosmosDb:DatabaseName"] 
        ?? throw new InvalidOperationException(
            "CosmosDb:DatabaseName is required but not configured. " +
            "Please add 'CosmosDb:DatabaseName' to your configuration.");

    public string GetEndpoint()
    {
        var endpoint = configuration["CosmosDb:Endpoint"];
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            return endpoint;
        }

        return GetConnectionStringValue("AccountEndpoint");
    }

    public string GetAuthKey()
    {
        var authKey = configuration["CosmosDb:AuthKey"];
        if (!string.IsNullOrWhiteSpace(authKey))
        {
            return authKey;
        }

        return GetConnectionStringValue("AccountKey");
    }

    private string GetConnectionStringValue(string key)
    {
        var connectionString = GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return string.Empty;
        }

        // Try standard format: AccountEndpoint=...;AccountKey=...
        foreach (var segment in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2 && parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }
        }

        // Fallback: if connection string looks like a full AccountKey (no parsing), return it directly
        // This handles cases where the entire connection string IS the account key for auth
        if (key.Equals("AccountKey", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("AccountEndpoint") &&
            !connectionString.Contains("="))
        {
            return connectionString;
        }

        return string.Empty;
    }
}