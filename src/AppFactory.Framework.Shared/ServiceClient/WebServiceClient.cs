using System.Net.Http.Headers;
using AppFactory.Framework.Logging;

namespace AppFactory.Framework.Shared.ServiceClient;

public class WebServiceClient : IWebServiceClient
{
    private readonly ILogger _logger;

    public WebServiceClient(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<ServiceResult> SendRequest(string baseUri, string url, CancellationToken cancellationToken = default)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(baseUri);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.GetAsync(url, cancellationToken);


            var data = await response.Content.ReadAsStringAsync(cancellationToken);

            return new ServiceResult
            {
                StatusCode = response.StatusCode,
                Data = data
            };
        }
    }

    public async Task<string> PostRequestAsync(HttpRequestMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(60);
                var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead,
                    cancellationToken);

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error happened during sending http request ");
            throw;
        }
    }
}