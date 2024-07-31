using AppFactory.Framework.Messaging.Publishers;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Messaging;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IMessagePublisher, MessagePublisher>();
        services.AddSingleton<IAmazonSqsClientFactory, AmazonSqsClientFactory>();

        return services;
    }
}