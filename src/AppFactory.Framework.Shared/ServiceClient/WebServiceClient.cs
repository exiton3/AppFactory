using System.Net.Http.Headers;
using System.Net.Mime;
using AppFactory.Framework.Logging;
using AppFactory.Framework.Shared.Serialization;

namespace AppFactory.Framework.Shared.ServiceClient;

public class WebServiceClient(ILogger logger, IJsonSerializer jsonSerializer) : IWebServiceClient
{
    public async Task<ServiceResult> SendRequest(string baseUri, string url, CancellationToken cancellationToken = default)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.BaseAddress = new Uri(baseUri);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

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
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns result with string</returns>
    public async Task<ServiceResult> SendRequest(HttpRequestMessage message, double timeout = 60, CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = new HttpClient();
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error happened during sending http request ");
            throw;
        }
    }

    /// <summary>
    /// Sends Http Web request
    /// </summary>
    /// <param name="message">HttpMessage</param>
    /// <param name="timeout">Timeout in seconds by default 60 sec</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    public async Task<ServiceResult<TResponse>> SendRequest<TResponse>(HttpRequestMessage message, double timeout = 60, CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = new HttpClient();

            httpClient.Timeout = TimeSpan.FromSeconds(timeout);
            var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseContentRead,
                cancellationToken);

            var result = await response.Content.ReadAsStringAsync(cancellationToken);

            var data = jsonSerializer.Deserialize<TResponse>(result);

            return new ServiceResult<TResponse>
            {
                StatusCode = response.StatusCode,
                Data = data
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error happened during sending http request ");
            throw;
        }
    }
}