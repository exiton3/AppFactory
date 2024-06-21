using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Logging;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddLogging(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILogger, SerilogLogger>();

        return serviceCollection;
    }
}