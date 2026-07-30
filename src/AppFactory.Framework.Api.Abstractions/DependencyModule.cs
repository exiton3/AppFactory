using AppFactory.Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api.Abstractions;

public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddRequestParsing();
    }
}