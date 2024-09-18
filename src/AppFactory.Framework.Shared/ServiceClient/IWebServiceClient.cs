namespace AppFactory.Framework.Shared.ServiceClient;

public interface IWebServiceClient
{
    Task<ServiceResult> SendRequest(string baseUri, string url, CancellationToken cancellationToken = default);
    Task<ServiceResult> SendRequest(HttpRequestMessage message, double timeout, CancellationToken cancellationToken = default);
}