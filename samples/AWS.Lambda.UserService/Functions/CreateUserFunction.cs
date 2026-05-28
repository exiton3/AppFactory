using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using AppFactory.Framework.Api.Aws;
using AWS.Lambda.UserService.Application.Commands;
using AWS.Lambda.UserService.Application.DTOs;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace AWS.Lambda.UserService.Functions;

public class CreateUserFunction : LambdaFunctionHandlerBase<CreateUserCommand, UserDto>
{
    protected override IStartup GetStartup()
    {
        return new Startup();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(
        APIGatewayProxyRequest request,
        ILambdaContext context)
    {
        return await Handle(request, context);
    }
}
