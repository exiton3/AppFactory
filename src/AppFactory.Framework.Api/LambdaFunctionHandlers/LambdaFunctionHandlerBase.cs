using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api.LambdaFunctionHandlers;

public abstract class LambdaFunctionHandlerBase<TRequest, TResponse> where TRequest : class, new() where TResponse : class
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private IRequestParser _requestParser;
    private ILambdaProcessor<TRequest, TResponse> _processor;
    private ILogger _log;
    private IStartup _startup;
    protected LambdaFunctionHandlerBase(IStartup startup = null)
    {
        _startup = startup;
        InitializeServices();
    }

    private void InitializeServices()
    {
        var services = new ServiceCollection();

        ConfigureServicesInt(services);
        ServiceProvider = services.BuildServiceProvider();
        JsonSerializer = ServiceProvider.GetRequiredService<IJsonSerializer>();
        _requestParser = ServiceProvider.GetService<IRequestParser>();
        _log = ServiceProvider.GetRequiredService<ILogger>();
        _log.LogInfo($"New instance of Lambda {GetHashCode()}");
    }

    private void ConfigureServicesInt(IServiceCollection services)
    {
        new DependencyModule().RegisterServices(services);

        _startup ??= GetStartup();
        _startup.ConfigureServices(services);
    }

    protected abstract IStartup GetStartup();

    public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context)
    {
        _log.AddTraceId(context.AwsRequestId);

        var errors = new List<Error>();

        TRequest parsedRequest;

        try
        {
            var inputRequest = MapToInputRequest(request);

            parsedRequest = _requestParser.ParseRequest<TRequest>(inputRequest);

        }
        catch (RequestParsingException e)
        {
            errors.Add(new Error("100", e.Message));


            return APIGatewayProxyReponseFactory.BadRequest(errors, "Input request parsing error");
        }
        catch (Exception e)
        {
            errors.Add(new Error("100", $"Could not parse request {e.Message} -- {e.StackTrace}"));

            return APIGatewayProxyReponseFactory.BadRequest(errors, "Input request parsing error");
        }

        try
        {
            using var scope = ServiceProvider.CreateScope();
            _processor = scope.ServiceProvider.GetRequiredService<ILambdaProcessor<TRequest, TResponse>>();

           
            _log.LogTrace($"Processor #{_processor.GetHashCode()} {_processor.GetType().Name} started");
            using (_log.LogPerformance($"Processor #{_processor.GetHashCode()} {_processor.GetType().Name}"))
            {
                var result = await _processor.Process(parsedRequest);


                var response = MapFromResult(result);

                return response;
            }
        }
        catch (Exception e)
        {
            errors.Add(new Error("100", $"{e.Message} -- {e.StackTrace} "));
            return APIGatewayProxyReponseFactory.Unexpected(errors);
        }
       
    }

    private APIGatewayProxyResponse MapFromResult<T>(Result<T> result)
    {
        switch (result.ResultType)
        {
            case ResultType.Ok:
               var data = JsonSerializer.Serialize(result.Data);
               return APIGatewayProxyReponseFactory.OK(data);
            case ResultType.Unexpected:
                return APIGatewayProxyReponseFactory.Unexpected(result.Errors);
            case ResultType.NotFound:
                return APIGatewayProxyReponseFactory.NotFound(result.Errors);
            case ResultType.Unauthorized:
                return APIGatewayProxyReponseFactory.NotFound(result.Errors);
            case ResultType.Invalid:
                return APIGatewayProxyReponseFactory.BadRequest(result.Errors);
            case ResultType.External:
                return APIGatewayProxyReponseFactory.ExternalError(result.Errors);
            case ResultType.Accepted:
                var acceptedData = JsonSerializer.Serialize(result.Data);
                return APIGatewayProxyReponseFactory.Accepted(acceptedData);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private InputRequest MapToInputRequest(APIGatewayProxyRequest request)
    {
        return new InputRequest
        {
            Path = request.PathParameters,
            Query = request.QueryStringParameters ?? new Dictionary<string, string>(),
            Body = request.Body
        };
    }
}