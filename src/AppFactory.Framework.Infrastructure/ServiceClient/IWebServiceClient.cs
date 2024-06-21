using System.Net;

namespace AppFactory.Framework.Infrastructure.ServiceClient;

public interface IWebServiceClient
{
    Task<ServiceResult> SendRequest(string baseUri, string url, CancellationToken cancellationToken = default);
    Task<string> PostRequestAsync(HttpRequestMessage message, CancellationToken cancellationToken = default);
}

public class ServiceResult
{
    public string Data { get; set; }

    public HttpStatusCode StatusCode { get; set; }
}