using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Domain;
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Domain.Services;
using AppFactory.Framework.Shared.Config;
using AppFactory.Framework.Shared.ServiceClient;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api;

public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddRequestParsing();
        services.AddSingleton<IConfigSettings, ConfigSettings>();
        services.AddSingleton<IServiceProvider>(x => x);
       
        services.AddSingleton<IEntityIdProvider, EntityIdProvider>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IWebServiceClient, WebServiceClient>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
    }
}

