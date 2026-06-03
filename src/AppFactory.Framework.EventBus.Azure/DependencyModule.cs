using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Shared.Config;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.EventBus.Azure;

public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();
        services.AddSingleton<IConfigSettings, ConfigSettings>();
        services.AddSingleton<IServiceProvider>(x => x);
    }
}
