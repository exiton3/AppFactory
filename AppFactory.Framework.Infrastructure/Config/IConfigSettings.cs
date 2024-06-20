namespace AppFactory.Framework.Infrastructure.Config;

public interface IConfigSettings
{
    /// <summary>
    /// Get the value of environment variable by key
    /// </summary>
    /// <param name="key">environment variable key</param>
    /// <returns>value</returns>
    string GetValue(string key);
}