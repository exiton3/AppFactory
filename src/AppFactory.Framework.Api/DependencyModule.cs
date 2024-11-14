using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Api.Parsing.Mappers;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Domain;
using AppFactory.Framework.Domain.Commands;
using AppFactory.Framework.Domain.Services;
using AppFactory.Framework.Shared.Config;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using AppFactory.Framework.Shared.ServiceClient;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api;

public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();
        services.AddSingleton<IConfigSettings, ConfigSettings>();
        services.AddSingleton<IParseModelMapRegistry, ParseModelMapRegistry>();
        services.AddSingleton<IRequestParser, RequestParser>();
        services.AddSingleton<IServiceProvider>(x => x);
        services.AddTransient<IPropertyMapper, PathPropertyMapper>();
        services.AddTransient<IPropertyMapper, QueryPropertyMapper>();
        services.AddTransient<IPropertyMapper, BodyPropertyMapper>();
        services.AddTransient<IPropertyMapperRegistry, PropertyMapperRegistry>();
        services.AddSingleton<IEntityIdProvider, EntityIdProvider>();
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IWebServiceClient, WebServiceClient>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddLogging(x => x.LogLevel = LogLevel.Debug);
    }
}