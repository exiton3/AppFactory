namespace AppFactory.Framework.Infrastructure.Config;

public class ConfigSettings : IConfigSettings
{
    public string GetValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
        
        var val = Environment.GetEnvironmentVariable(key);

        return val ?? string.Empty;
    }
}