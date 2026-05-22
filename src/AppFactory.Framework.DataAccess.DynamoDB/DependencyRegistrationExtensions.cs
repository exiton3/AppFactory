using AppFactory.Framework.DataAccess.DynamoDB.Configuration;
using AppFactory.Framework.DataAccess.DynamoDB.DynamoDb;
using AppFactory.Framework.DataAccess.DynamoDB.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DataAccess.DynamoDB;

public static class DependencyRegistrationExtensions
{
    public static void RegisterPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDynamoDBClientFactory, DynamoDbClientFactory>();
        services.AddScoped<IAWSSettings, AwsSettings>();
    }

    public static IServiceCollection RegisterModelConfig<TModelConfig,TModel>(this IServiceCollection services) where TModelConfig : IModelConfig<TModel> where TModel:class
    {
        services.AddSingleton(typeof(IModelConfig<TModel>), typeof(TModelConfig));

        return services;
    }
}