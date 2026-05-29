using Azure;
using Azure.Messaging.EventGrid;
using AppFactory.Framework.EventBus.Abstractions;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.EventBus.Azure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventGridPublisher(
        this IServiceCollection services,
        string endpoint,
        string accessKey)
    {
        services.AddSingleton(sp =>
        {
            var credential = new AzureKeyCredential(accessKey);
            return new EventGridPublisherClient(new Uri(endpoint), credential);
        });

        services.AddSingleton<IEventPublisher>(sp =>
        {
            var client = sp.GetRequiredService<EventGridPublisherClient>();
            var serializer = sp.GetRequiredService<IJsonSerializer>();
            var logger = sp.GetRequiredService<Logging.ILogger>();

            return new EventGridPublisher(client, serializer, logger);
        });

        return services;
    }

    public static IServiceCollection AddEventHandlers(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
