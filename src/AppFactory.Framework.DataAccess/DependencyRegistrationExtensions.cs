using AppFactory.Framework.DataAccess.AmazonDbServices;
using AppFactory.Framework.DataAccess.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.DataAccess;

public static class DependencyRegistrationExtensions
{
    public static void RegisterPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDynamoDBClientFactory, DynamoDbClientFactory>();
        services.AddScoped<IAWSSettings, AwsSettings>();
    }
}