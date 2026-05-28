using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Api.Parsing;
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Application.Queries;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.Api.AspNetCore.Extensions;

/// <summary>
/// Extension methods for mapping CQRS endpoints to ASP.NET Core minimal API
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Map a CQRS endpoint (command or query)
    /// </summary>
    public static RouteHandlerBuilder MapCqrsEndpoint<TRequest, TResponse>(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        string method = "POST")
        where TRequest : class, new()
        where TResponse : class
    {
        var handler = async (HttpContext context) =>
        {
            var requestParser = context.RequestServices.GetRequiredService<IRequestParser>();
            var processor = context.RequestServices.GetRequiredService<IFunctionProcessor<TRequest, TResponse>>();
            var logger = context.RequestServices.GetService<ILogger>();

            var requestContext = new AspNetCoreRequestContext(context);
            var responseBuilder = new AspNetCoreResponseBuilder(context);

            try
            {
                // Parse request
                var inputRequest = new Parsing.InputRequest
                {
                    Path = requestContext.PathParameters,
                    Query = requestContext.QueryParameters,
                    Body = requestContext.Body
                };

                var parsedRequest = requestParser.ParseRequest<TRequest>(inputRequest);

                // Process request
                logger?.LogTrace($"Processing {typeof(TRequest).Name}");
                var result = await processor.Process(parsedRequest, context.RequestAborted);

                // Build response
                MapResult(result, responseBuilder, context);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error processing request: {Message}", ex.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
            }
        };

        return method.ToUpperInvariant() switch
        {
            "GET" => endpoints.MapGet(pattern, handler),
            "POST" => endpoints.MapPost(pattern, handler),
            "PUT" => endpoints.MapPut(pattern, handler),
            "DELETE" => endpoints.MapDelete(pattern, handler),
            "PATCH" => endpoints.MapPatch(pattern, handler),
            _ => throw new ArgumentException($"Unsupported HTTP method: {method}", nameof(method))
        };
    }

    /// <summary>
    /// Map a command endpoint (POST by default)
    /// </summary>
    public static RouteHandlerBuilder MapCommand<TCommand, TResponse>(
        this IEndpointRouteBuilder endpoints,
        string pattern)
        where TCommand : class, ICommand, new()
        where TResponse : class
    {
        return endpoints.MapCqrsEndpoint<TCommand, TResponse>(pattern, "POST");
    }

    /// <summary>
    /// Map a query endpoint (GET by default)
    /// </summary>
    public static RouteHandlerBuilder MapQuery<TQuery, TResponse>(
        this IEndpointRouteBuilder endpoints,
        string pattern)
        where TQuery : class, IQueryRequest, new()
        where TResponse : class
    {
        return endpoints.MapCqrsEndpoint<TQuery, TResponse>(pattern, "GET");
    }

    private static void MapResult<TResponse>(
        Domain.ServiceResult.Result<TResponse> result,
        AspNetCoreResponseBuilder responseBuilder,
        HttpContext context)
        where TResponse : class
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
