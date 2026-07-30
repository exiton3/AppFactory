using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Api.Parsing.Mappers;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api.Abstractions;

public static class DependencyModuleExtensions
{
    public static IServiceCollection AddRequestParsing(this IServiceCollection services)
    {

        services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();

        // Add parsing services
        services.AddSingleton<IPropertyMapperRegistry, PropertyMapperRegistry>();
        services.AddTransient<IPropertyMapper, PathPropertyMapper>();
        services.AddTransient<IPropertyMapper, QueryPropertyMapper>();
        services.AddTransient<IPropertyMapper, BodyPropertyMapper>();
        services.AddSingleton<IParseModelMapRegistry>(sp =>
        {
            var maps = sp.GetServices<IParseModelMap>();
            return new ParseModelMapRegistry(maps);
        });
        services.AddSingleton<IRequestParser, RequestParser>();

        return services;
    }
}