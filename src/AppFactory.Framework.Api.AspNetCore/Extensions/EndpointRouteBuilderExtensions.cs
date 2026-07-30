using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using AppFactory.Framework.Api.AspNetCore.Core;
using AppFactory.Framework.Api.AspNetCore.Endpoints;
using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Application.Commands;
using AppFactory.Framework.Application.Queries;

namespace AppFactory.Framework.Api.AspNetCore.Extensions;

/// <summary>
/// Extension methods for mapping AppFactory endpoints to ASP.NET Core minimal API
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Discover and map endpoint configuration classes from the provided assemblies.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpointConfigs(
        this IEndpointRouteBuilder endpoints,
        params System.Reflection.Assembly[] assemblies)
    {
        if (assemblies is null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
        }

        var endpointTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IEndpointConfig).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
            .OrderBy(t => t.FullName)
            .ToList();

        foreach (var endpointType in endpointTypes)
        {
            var endpointConfig = (IEndpointConfig)ActivatorUtilities.CreateInstance(endpoints.ServiceProvider, endpointType);
            endpointConfig.Map(endpoints);
        }

        return endpoints;
    }

    /// <summary>
    /// Map an endpoint route and bind it to the AppFactory request handler pipeline.
    /// </summary>
    public static RouteHandlerBuilder MapEndpointRoute<TRequest, TResponse>(
        this IEndpointRouteBuilder routeBuilder,
        string pattern,
        string method = "POST")
        where TRequest : class, new()
        where TResponse : class
    {
        return MapByHttpMethod(routeBuilder, pattern, method, HandleEndpointRequest<TRequest, TResponse>);
    }

    /// <summary>
    /// Backward-compatible alias for MapEndpointRoute.
    /// </summary>
    public static RouteHandlerBuilder MapEndpoint<TRequest, TResponse>(
        this IEndpointRouteBuilder routeBuilder,
        string pattern,
        string method = "POST")
        where TRequest : class, new()
        where TResponse : class
    {
        return routeBuilder.MapEndpointRoute<TRequest, TResponse>(pattern, method);
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
        return endpoints.MapEndpointRoute<TCommand, TResponse>(pattern, "POST");
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
        return endpoints.MapEndpointRoute<TQuery, TResponse>(pattern, "GET");
    }

    private static Task HandleEndpointRequest<TRequest, TResponse>(HttpContext context)
        where TRequest : class, new()
        where TResponse : class
    {
        var requestHandler = context.RequestServices.GetRequiredService<IEndpointRequestHandler<TRequest, TResponse>>();
        return requestHandler.HandleAsync(context);
    }

    private static RouteHandlerBuilder MapByHttpMethod(
        IEndpointRouteBuilder routeBuilder,
        string pattern,
        string method,
        Delegate handler)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => routeBuilder.MapGet(pattern, handler),
            "POST" => routeBuilder.MapPost(pattern, handler),
            "PUT" => routeBuilder.MapPut(pattern, handler),
            "DELETE" => routeBuilder.MapDelete(pattern, handler),
            "PATCH" => routeBuilder.MapPatch(pattern, handler),
            _ => throw new ArgumentException($"Unsupported HTTP method: {method}", nameof(method))
        };
    }

}
