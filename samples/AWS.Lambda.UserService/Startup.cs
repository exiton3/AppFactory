using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.DataAccess.DynamoDB;
using AppFactory.Framework.Logging.Serilog;
using Microsoft.Extensions.DependencyInjection;
using AWS.Lambda.UserService.Application.Processors;
using AWS.Lambda.UserService.Application.DTOs;
using AWS.Lambda.UserService.Application.Commands;

namespace AWS.Lambda.UserService;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSerilogLogging();

        services.AddCqrs(typeof(CreateUserCommandHandler).Assembly);

        services.AddScoped<IFunctionProcessor<CreateUserCommand, UserDto>, CreateUserProcessor>();

        services.RegisterDynamoDbDataAccess(typeof(Startup).Assembly);
    }
}
