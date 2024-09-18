using System.Net;

namespace AppFactory.Framework.Shared.ServiceClient;

public class ServiceResult
{
    public string Data { get; set; }

    public HttpStatusCode StatusCode { get; set; }
}