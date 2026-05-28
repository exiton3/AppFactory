using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.DependencyInjection;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api.Core;

/// <summary>
/// Platform-agnostic core handler for CQRS-based HTTP functions
/// Works across AWS Lambda, Azure Functions, and ASP.NET Core
/// </summary>
public class FunctionHandlerCore<TRequest, TResponse> 
    where TRequest : class, new() 
    where TResponse : class
{
    protected ServiceProvider ServiceProvider;
    protected IJsonSerializer JsonSerializer;
    private IRequestParser _requestParser;
    private IFunctionProcessor<TRequest, TResponse> _processor;
    private ILogger _log;
    private readonly IStartup _startup;

    public FunctionHandlerCore(IStartup startup)
    {
        _startup = startup ?? throw new ArgumentNullException(nameof(startup));
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
    }

    private void ConfigureServicesInt(IServiceCollection services)
    {
        new DependencyModule().RegisterServices(services);
        _startup.ConfigureServices(services);
    }

    /// <summary>
    /// Handle HTTP request using platform-agnostic abstractions
    /// </summary>
    public async Task HandleRequest(
        IHttpRequestContext requestContext, 
        IHttpResponseBuilder responseBuilder)
    {
        _log.AddTraceId(requestContext.RequestId);
        _log.LogInfo($"Processing request {requestContext.RequestId}");

        var errors = new List<Error>();
        TRequest parsedRequest;

        // Parse request
        try
        {
            var inputRequest = MapToInputRequest(requestContext);
            parsedRequest = _requestParser.ParseRequest<TRequest>(inputRequest);
        }
        catch (RequestParsingException e)
        {
            errors.Add(new Error("PARSING_ERROR", e.Message));
            BuildErrorResponse(responseBuilder, errors, "Input request parsing error", 
                HttpStatusCode.BadRequest, "ValidationException");
            return;
        }
        catch (Exception e)
        {
            errors.Add(new Error("PARSING_ERROR", $"Could not parse request: {e.Message}"));
            _log.LogError(e, $"Request parsing failed: {e.Message}");
            BuildErrorResponse(responseBuilder, errors, "Input request parsing error", 
                HttpStatusCode.BadRequest, "ValidationException");
            return;
        }

        // Process request
        try
        {
            using var scope = ServiceProvider.CreateScope();
            _processor = scope.ServiceProvider.GetRequiredService<IFunctionProcessor<TRequest, TResponse>>();

            _log.LogTrace($"Processor #{_processor.GetHashCode()} {_processor.GetType().Name} started");
            
            using (_log.LogPerformance($"Processor #{_processor.GetHashCode()} {_processor.GetType().Name}"))
            {
                var result = await _processor.Process(parsedRequest);
                MapFromResult(result, responseBuilder);
            }
        }
        catch (Exception e)
        {
            errors.Add(new Error("INTERNAL_ERROR", e.Message));
            _log.LogError(e, $"Request processing failed: {e.Message}");
            BuildErrorResponse(responseBuilder, errors, "Unexpected error", 
                HttpStatusCode.InternalServerError, "InternalServerError");
        }
    }

    private void MapFromResult(Result<TResponse> result, IHttpResponseBuilder responseBuilder)
    {
        switch (result.ResultType)
        {
            case ResultType.Ok:
                var data = JsonSerializer.Serialize(result.Data);
                responseBuilder
                    .StatusCode(HttpStatusCode.OK)
                    .ContentType("application/json")
                    .Body(data);
                break;

            case ResultType.Accepted:
                var acceptedData = JsonSerializer.Serialize(result.Data);
                responseBuilder
                    .StatusCode(HttpStatusCode.Accepted)
                    .ContentType("application/json")
                    .Body(acceptedData);
                break;

            case ResultType.Invalid:
                BuildErrorResponse(responseBuilder, result.Errors, 
                    $"{result.Errors.Count} validation error{(result.Errors.Count > 1 ? "s" : string.Empty)} detected:",
                    HttpStatusCode.BadRequest, "ValidationException");
                break;

            case ResultType.NotFound:
                BuildErrorResponse(responseBuilder, result.Errors, "Resource not found",
                    HttpStatusCode.NotFound, "NotFoundException");
                break;

            case ResultType.Unauthorized:
                BuildErrorResponse(responseBuilder, result.Errors, "Unauthorized",
                    HttpStatusCode.Unauthorized, "UnauthorizedException");
                break;

            case ResultType.External:
                BuildErrorResponse(responseBuilder, result.Errors, "External system error",
                    HttpStatusCode.ServiceUnavailable, "ExternalSystemError");
                break;

            case ResultType.Unexpected:
                BuildErrorResponse(responseBuilder, result.Errors, "Unexpected error",
                    HttpStatusCode.InternalServerError, "InternalServerError");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result.ResultType), result.ResultType, 
                    "Unknown result type");
        }
    }

    private void BuildErrorResponse(
        IHttpResponseBuilder responseBuilder,
        IEnumerable<Error> errors,
        string title,
        int statusCode,
        string errorType)
    {
        var problemResponse = new ProblemResponse
        {
            Problem = title,
            Errors = errors.ToList()
        };

        responseBuilder
            .StatusCode(statusCode)
            .ContentType("application/json")
            .ProblemTitle(title)
            .ErrorType(errorType)
            .Errors(errors)
            .Body(JsonSerializer.Serialize(problemResponse));
    }

    private InputRequest MapToInputRequest(IHttpRequestContext requestContext)
    {
        return new InputRequest
        {
            Path = requestContext.PathParameters,
            Query = requestContext.QueryParameters ?? new Dictionary<string, string>(),
            Body = requestContext.Body
        };
    }
}
