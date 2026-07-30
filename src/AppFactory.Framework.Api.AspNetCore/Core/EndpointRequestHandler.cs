using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public class EndpointRequestHandler<TRequest, TResponse> : IEndpointRequestHandler<TRequest, TResponse>
    where TRequest : class, new()
    where TResponse : class
{
    private readonly IRequestParser _requestParser;
    private readonly IFunctionProcessor<TRequest, TResponse> _processor;
    private readonly ILogger? _logger;

    public EndpointRequestHandler(
        IRequestParser requestParser,
        IFunctionProcessor<TRequest, TResponse> processor,
        ILogger? logger = null)
    {
        _requestParser = requestParser;
        _processor = processor;
        _logger = logger;
    }

    public async Task HandleAsync(HttpContext context)
    {
        var requestContext = new AspNetCoreRequestContext(context);
        var responseBuilder = new AspNetCoreResponseBuilder(context);

        try
        {
            var inputRequest = new InputRequest
            {
                Path = requestContext.PathParameters,
                Query = requestContext.QueryParameters,
                Body = requestContext.Body
            };

            var parsedRequest = _requestParser.ParseRequest<TRequest>(inputRequest);

            _logger?.LogTrace($"Processing {typeof(TRequest).Name}");
            var result = await _processor.Process(parsedRequest, context.RequestAborted);

            MapResult(result, responseBuilder, context);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing request: {Message}", ex.Message);
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Internal server error", exception = ex });
        }
    }

    private static void MapResult(
        Domain.ServiceResult.Result<TResponse> result,
        AspNetCoreResponseBuilder responseBuilder,
        HttpContext context)
    {
        var jsonSerializer = context.RequestServices.GetRequiredService<Shared.Serialization.IJsonSerializer>();

        switch (result.ResultType)
        {
            case Domain.ServiceResult.ResultType.Ok:
                responseBuilder
                    .StatusCode(Abstractions.HttpStatusCode.OK)
                    .Body(jsonSerializer.Serialize(result.Data));
                break;

            case Domain.ServiceResult.ResultType.Accepted:
                responseBuilder
                    .StatusCode(Abstractions.HttpStatusCode.Accepted)
                    .Body(jsonSerializer.Serialize(result.Data));
                break;

            case Domain.ServiceResult.ResultType.Invalid:
                responseBuilder
                    .StatusCode(Abstractions.HttpStatusCode.BadRequest)
                    .ErrorType("ValidationException")
                    .Errors(result.Errors)
                    .Body(new Responses.ProblemResponse
                    {
                        Problem = "Validation failed",
                        Errors = result.Errors.ToList()
                    });
                break;

            case Domain.ServiceResult.ResultType.NotFound:
                responseBuilder
                    .StatusCode(Abstractions.HttpStatusCode.NotFound)
                    .ErrorType("NotFoundException")
                    .Body(new { message = string.Join(", ", result.Errors.Select(e => e.Message)) });
                break;

            case Domain.ServiceResult.ResultType.Unauthorized:
                responseBuilder
                    .StatusCode(Abstractions.HttpStatusCode.Unauthorized)
                    .ErrorType("UnauthorizedException")
                    .Body(new { message = "Unauthorized" });
                break;

            case Domain.ServiceResult.ResultType.External:
                responseBuilder
                    .StatusCode(Abstractions.HttpStatusCode.ServiceUnavailable)
                    .ErrorType("ExternalSystemError")
                    .Errors(result.Errors)
                    .Body(new Responses.ProblemResponse
                    {
                        Problem = "External system error",
                        Errors = result.Errors.ToList()
                    });
                break;

            case Domain.ServiceResult.ResultType.Unexpected:
                responseBuilder
                    .StatusCode(Abstractions.HttpStatusCode.InternalServerError)
                    .ErrorType("InternalServerError")
                    .Errors(result.Errors)
                    .Body(new Responses.ProblemResponse
                    {
                        Problem = "Unexpected error",
                        Errors = result.Errors.ToList()
                    });
                break;
        }

        responseBuilder.Build();
    }
}
