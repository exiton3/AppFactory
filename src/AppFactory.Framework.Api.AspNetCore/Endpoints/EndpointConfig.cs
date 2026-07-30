using AppFactory.Framework.Api.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AppFactory.Framework.Api.AspNetCore.Endpoints;

public interface IEndpointRouteStep
{
    IEndpointMetadataStep Get(string pattern);
    IEndpointMetadataStep Post(string pattern);
    IEndpointMetadataStep Put(string pattern);
    IEndpointMetadataStep Patch(string pattern);
    IEndpointMetadataStep Delete(string pattern);
}

public interface IEndpointFinalStep
{
    IEndpointFinalStep Produces<T>(int statusCode);
    IEndpointFinalStep Produces(int statusCode);
    IEndpointFinalStep ConfigureRoute(Action<RouteHandlerBuilder> configure);
}

public interface IEndpointSecurityStep
{
    IEndpointFinalStep AllowAnonymous();
    IEndpointFinalStep RequireAuthorization();
    IEndpointFinalStep RequireAuthorization(string policy);
    IEndpointFinalStep RequireAuthorization(Action<AuthorizationPolicyBuilder> policyBuilder);
}

public interface IEndpointMetadataStep : IEndpointFinalStep
{
    IEndpointMetadataStep Name(string name);
    IEndpointMetadataStep Summary(string summary);
    IEndpointMetadataStep Description(string description);
    IEndpointMetadataStep Tags(params string[] tags);
    IEndpointSecurityStep Security();
}

enum AuthorizationMode
{
    None,
    AllowAnonymous,
    RequireAuthorization
}

/// <summary>
/// Base class for REPR-style endpoint configuration with method-based declaration.
/// </summary>
public abstract class EndpointConfig<TRequest, TResponse> : IEndpointConfig
    where TRequest : class, new()
    where TResponse : class
{
    private string? _pattern;
    private string _method = "POST";
    private string? _name;
    private string? _summary;
    private string? _description;
    private readonly List<string> _tags = [];
    private AuthorizationMode _authorizationMode;
    private string[]? _authorizationPolicies;
    private Action<AuthorizationPolicyBuilder>? _authorizationPolicyBuilder;
    private readonly List<Action<RouteHandlerBuilder>> _routeMetadata = [];
    private readonly EndpointBuilder _builder;

    protected EndpointConfig()
    {
        _builder = new EndpointBuilder(this);
    }

    public void Map(IEndpointRouteBuilder endpoints)
    {
        ResetState();
        Configure();

        if (string.IsNullOrWhiteSpace(_pattern))
        {
            throw new InvalidOperationException($"Endpoint pattern is required for {GetType().Name}. Call Get/Post/Put/Patch/Delete in Configure().");
        }

        var route = endpoints.MapEndpointRoute<TRequest, TResponse>(_pattern, _method);

        if (!string.IsNullOrWhiteSpace(_name))
        {
            route.WithName(_name);
        }

        if (!string.IsNullOrWhiteSpace(_summary))
        {
            route.WithSummary(_summary);
        }

        if (!string.IsNullOrWhiteSpace(_description))
        {
            route.WithDescription(_description);
        }

        if (_tags.Count > 0)
        {
            route.WithTags(_tags.ToArray());
        }

        if (_authorizationMode == AuthorizationMode.AllowAnonymous)
        {
            route.AllowAnonymous();
        }

        if (_authorizationMode == AuthorizationMode.RequireAuthorization)
        {
            if (_authorizationPolicyBuilder is not null)
            {
                route.RequireAuthorization(_authorizationPolicyBuilder);
            }
            else if (_authorizationPolicies is { Length: > 0 })
            {
                route.RequireAuthorization(_authorizationPolicies);
            }
            else
            {
                route.RequireAuthorization();
            }
        }

        foreach (var addMetadata in _routeMetadata)
        {
            addMetadata(route);
        }
    }

    /// <summary>
    /// Configure endpoint method, route pattern, and metadata.
    /// </summary>
    protected abstract void Configure();

    protected IEndpointMetadataStep Get(string pattern)
    {
        SetRoute("GET", pattern);
        return _builder;
    }

    protected IEndpointMetadataStep Post(string pattern)
    {
        SetRoute("POST", pattern);
        return _builder;
    }

    protected IEndpointMetadataStep Put(string pattern)
    {
        SetRoute("PUT", pattern);
        return _builder;
    }

    protected IEndpointMetadataStep Patch(string pattern)
    {
        SetRoute("PATCH", pattern);
        return _builder;
    }

    protected IEndpointMetadataStep Delete(string pattern)
    {
        SetRoute("DELETE", pattern);
        return _builder;
    }

    private void SetAuthorizationMode(AuthorizationMode authorizationMode)
    {
        if (_authorizationMode != AuthorizationMode.None && _authorizationMode != authorizationMode)
        {
            throw new InvalidOperationException(
                $"Conflicting authorization config in {GetType().Name}: '{_authorizationMode}' and '{authorizationMode}' cannot both be configured.");
        }

        _authorizationMode = authorizationMode;
    }

    private void SetAuthorizationPolicies(string[] policies)
    {
        _authorizationPolicies = policies;
        _authorizationPolicyBuilder = null;
    }

    private void SetAuthorizationPolicyBuilder(Action<AuthorizationPolicyBuilder> policyBuilder)
    {
        _authorizationPolicyBuilder = policyBuilder;
        _authorizationPolicies = null;
    }

    private void SetRoute(string method, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Route pattern cannot be null or empty.", nameof(pattern));
        }

        _method = method;
        _pattern = pattern;
    }

    private void ResetState()
    {
        _pattern = null;
        _method = "POST";
        _name = null;
        _summary = null;
        _description = null;
        _tags.Clear();
        _authorizationMode = AuthorizationMode.None;
        _authorizationPolicies = null;
        _authorizationPolicyBuilder = null;
        _routeMetadata.Clear();
    }

    private sealed class EndpointBuilder : IEndpointMetadataStep, IEndpointSecurityStep
    {
        private readonly EndpointConfig<TRequest, TResponse> _endpoint;

        public EndpointBuilder(EndpointConfig<TRequest, TResponse> endpoint)
        {
            _endpoint = endpoint;
        }

        public IEndpointMetadataStep Name(string name)
        {
            _endpoint._name = name;
            return this;
        }

        public IEndpointMetadataStep Summary(string summary)
        {
            _endpoint._summary = summary;
            return this;
        }

        public IEndpointMetadataStep Description(string description)
        {
            _endpoint._description = description;
            return this;
        }

        public IEndpointMetadataStep Tags(params string[] tags)
        {
            _endpoint._tags.AddRange(tags.Where(t => !string.IsNullOrWhiteSpace(t)));
            return this;
        }

        public IEndpointSecurityStep Security()
        {
            return this;
        }

        public IEndpointFinalStep AllowAnonymous()
        {
            _endpoint.SetAuthorizationMode(AuthorizationMode.AllowAnonymous);
            return this;
        }

        public IEndpointFinalStep RequireAuthorization()
        {
            _endpoint.SetAuthorizationMode(AuthorizationMode.RequireAuthorization);
            return this;
        }

        public IEndpointFinalStep RequireAuthorization(string policy)
        {
            if (string.IsNullOrWhiteSpace(policy))
            {
                throw new ArgumentException("Authorization policy cannot be null or empty.", nameof(policy));
            }

            _endpoint.SetAuthorizationMode(AuthorizationMode.RequireAuthorization);
            _endpoint.SetAuthorizationPolicies([policy]);
            return this;
        }

        public IEndpointFinalStep RequireAuthorization(Action<AuthorizationPolicyBuilder> policyBuilder)
        {
            ArgumentNullException.ThrowIfNull(policyBuilder);

            _endpoint.SetAuthorizationMode(AuthorizationMode.RequireAuthorization);
            _endpoint.SetAuthorizationPolicyBuilder(policyBuilder);
            return this;
        }

        public IEndpointFinalStep Produces<T>(int statusCode)
        {
            _endpoint._routeMetadata.Add(route => route.Produces<T>(statusCode));
            return this;
        }

        public IEndpointFinalStep Produces(int statusCode)
        {
            _endpoint._routeMetadata.Add(route => route.Produces(statusCode));
            return this;
        }

        public IEndpointFinalStep ConfigureRoute(Action<RouteHandlerBuilder> configure)
        {
            _endpoint._routeMetadata.Add(configure);
            return this;
        }
    }
}
