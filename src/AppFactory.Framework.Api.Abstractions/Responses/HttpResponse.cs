using AppFactory.Framework.Api.Abstractions;
using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.Responses;

public class HttpResponse
{
    public int StatusCode { get; set; } = HttpStatusCode.OK;
    public string Body { get; set; }
    public string ContentType { get; set; } = "application/json";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string ErrorType { get; set; }
    public string ProblemTitle { get; set; }
    public IEnumerable<Error> Errors { get; set; }
}
