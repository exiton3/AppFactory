using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Config;
using AppFactory.Framework.Shared.Serialization;
using AppFactory.Framework.Shared.ServiceClient;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Messaging;

public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();
        services.AddSingleton<IConfigSettings, ConfigSettings>();
        services.AddScoped<IWebServiceClient, WebServiceClient>();
        services.AddSingleton<IServiceProvider>(x => x);
        services.AddLogging();
    }
}