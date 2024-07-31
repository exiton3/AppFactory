using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Messaging.Publishers;
using AppFactory.Framework.Shared.Config;
using AppFactory.Framework.Shared.Serialization;
using AppFactory.Framework.Shared.ServiceClient;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Messaging;

public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();
        services.AddSingleton<IConfigSettings, ConfigSettings>();
        //  services.AddSingleton<IEntityIdProvider, EntityIdProvider>();
        // services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IWebServiceClient, WebServiceClient>();
        services.AddSingleton<IServiceProvider>(x => x);

    }
}