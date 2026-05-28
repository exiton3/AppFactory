using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Logging.Abstractions;
using AppFactory.Framework.Shared.Config;
using AppFactory.Framework.Shared.Serialization;
using AppFactory.Framework.Shared.ServiceClient;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api.Aws;

/// <summary>
/// Dependency registration module for AWS Lambda API integration
/// </summary>
public class DependencyModule : IDependencyRegistrationModule
{
    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IJsonSerializer, DefaultJsonSerializer>();
        services.AddSingleton<IConfigSettings, ConfigSettings>();
        services.AddScoped<IWebServiceClient, WebServiceClient>();
        services.AddSingleton<IServiceProvider>(x => x);
        services.AddSingleton<IRequestParser, RequestParser>();
    }
}
