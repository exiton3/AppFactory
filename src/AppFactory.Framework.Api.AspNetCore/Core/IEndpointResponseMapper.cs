using AppFactory.Framework.Domain.ServiceResult;
using Microsoft.AspNetCore.Http;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public interface IEndpointResponseMapper<TResponse>
    where TResponse : class
{
    void Map(Result<TResponse> result, AspNetCoreResponseBuilder responseBuilder, HttpContext context);
}
