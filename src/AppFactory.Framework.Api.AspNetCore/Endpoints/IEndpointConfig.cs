using Microsoft.AspNetCore.Routing;

namespace AppFactory.Framework.Api.AspNetCore.Endpoints;

/// <summary>
/// Contract for endpoint configuration classes.
/// </summary>
public interface IEndpointConfig
{
    void Map(IEndpointRouteBuilder endpoints);
}
