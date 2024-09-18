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
    /// <summary>
    /// Sends Http Web request
    /// </summary>
    /// <param name="message">HttpMessage</param>
    /// <param name="timeout">Timeout in seconds by default 60 sec</param>
    /// <param name="cancellationToken">Cancelation token</param>
    /// <returns></returns>
    public async Task<ServiceResult> SendRequest(HttpRequestMessage message, double timeout = 60, CancellationToken cancellationToken = default)
    {
        try
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(timeout);
                var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead,
                    cancellationToken);

                var result = await response.Content.ReadAsStringAsync(cancellationToken);

                return new ServiceResult
                {
                    StatusCode = response.StatusCode,
                    Data = result
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error happened during sending http request ");
            throw;
        }
    }
}