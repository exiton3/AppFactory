using System.Net;

namespace AppFactory.Framework.Shared.ServiceClient;

public class ServiceResult<TResponse>
{
    public TResponse Data { get; set; }

    public HttpStatusCode StatusCode { get; set; }
}

public class ServiceResult: ServiceResult<string>
{
    public string Data { get; set; }

    public HttpStatusCode StatusCode { get; set; }
}