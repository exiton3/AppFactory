using Amazon.Lambda.CloudWatchEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AppFactory.Framework.EventBus.Aws;
using EventDriven.Aws.UserService.Events;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace EventDriven.Aws.UserService.Functions;

public class UserCreatedEventFunction : LambdaEventHandlerBase<UserCreatedEvent>
{
    protected override IStartup GetStartup()
    {
        return new Startup();
    }

    public async Task FunctionHandler(CloudWatchEvent<UserCreatedEvent> @event, ILambdaContext context)
    {
        await base.FunctionHandler(@event, context);
    }
}
