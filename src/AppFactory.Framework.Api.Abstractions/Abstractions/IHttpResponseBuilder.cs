using AppFactory.Framework.Api.Responses;
using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.Abstractions;

public interface IHttpResponseBuilder
{
    IHttpResponseBuilder StatusCode(int statusCode);
    IHttpResponseBuilder Header(string key, string value);
    IHttpResponseBuilder Headers(IDictionary<string, string> headers);
    IHttpResponseBuilder Body(string body);
    IHttpResponseBuilder Body<T>(T data);
    IHttpResponseBuilder ContentType(string contentType);
    IHttpResponseBuilder ErrorType(string errorType);
    IHttpResponseBuilder ProblemTitle(string title);
    IHttpResponseBuilder Errors(IEnumerable<Error> errors);
    HttpResponse Build();
}
