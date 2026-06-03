using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Messaging.Azure.FunctionHandlers;

/// <summary>
/// Dependency module for Azure Function message handlers
/// </summary>
public class DependencyModule
{
    public void RegisterServices(IServiceCollection services)
    {
        // Base handlers are registered per-function, not globally
        // Individual projects should register their specific handlers and processors
    }
}
