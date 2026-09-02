using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.AspNetCore.Core;

public interface IEndpointResponseMapper<TResponse>
    where TResponse : class
{
    HttpResponse Map(Result<TResponse> result);
}
