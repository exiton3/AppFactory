using Microsoft.AspNetCore.Http;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public interface IEndpointRequestHandler<TRequest, TResponse>
    where TRequest : class, new()
    where TResponse : class
{
    Task HandleAsync(HttpContext context);
}
