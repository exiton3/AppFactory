using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.EventBus.Aws;
using AppFactory.Framework.Logging.Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace EventDriven.Aws.UserService;

public class Startup : IStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSerilogLogging();

        services.AddCqrs(typeof(Startup).Assembly);

        var eventBusName = Environment.GetEnvironmentVariable("EVENT_BUS_NAME") ?? "default";
        services.AddEventBridgePublisher(eventBusName);

        services.AddEventHandlers(typeof(Startup).Assembly);
    }
}
