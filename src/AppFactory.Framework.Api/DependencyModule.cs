using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Api.Parsing.Mappers;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Domain;
using AppFactory.Framework.Domain.Commands;
using AppFactory.Framework.Domain.Services;
using AppFactory.Framework.Infrastructure.Config;
using AppFactory.Framework.Infrastructure.Serialization;
using AppFactory.Framework.Infrastructure.ServiceClient;
using AppFactory.Framework.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api;

public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddLogging();
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
    }
}