using AppFactory.Framework.DataAccess.CosmosDB.Configuration;
using AppFactory.Framework.DataAccess.CosmosDB.CosmosDb;
using AppFactory.Framework.DataAccess.CosmosDB.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DataAccess.CosmosDB;

public static class DependencyRegistrationExtensions
{
    public static void RegisterCosmosDbPersistence(this IServiceCollection services)
    {
        services.AddSingleton<ICosmosDbClientFactory, CosmosDbClientFactory>();
        services.AddScoped<ICosmosDbSettings, CosmosDbSettings>();
    }

    public static IServiceCollection RegisterModelConfig<TModelConfig, TModel>(this IServiceCollection services) 
        where TModelConfig : IModelConfig<TModel> 
        where TModel : class
    {
        services.AddSingleton(typeof(IModelConfig<TModel>), typeof(TModelConfig));
        return services;
    }
}
