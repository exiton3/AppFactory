using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using AppFactory.Framework.Domain.ServiceResult;
using AppFactory.Framework.Infrastructure.Serialization;

namespace AppFactory.Framework.Api.Responses;

class ResponseBuilder
{
    private readonly APIGatewayProxyResponse _response;

    private string _title;
    public ResponseBuilder()
    {
        _response = new APIGatewayProxyResponse
        {
            Headers = new Dictionary<string, string>()
        };

        _response.Headers.Add("Content-Type", "application/json");
        _response.Headers.Add("Access-Control-Allow-Origin", "*");
        _response.Headers.Add("Access-Control-Allow-Methods", "OPTIONS, POST,PUT, DELETE, GET, HEAD");
    }

    public static ResponseBuilder Response => new();

    public ResponseBuilder ProblemTitle(string title)
    {
        _title = title;

        return this;
    }
    public ResponseBuilder Body(string body)
    {
        _response.Body = body;

        return this;
    }

    public ResponseBuilder Body(IEnumerable<Error> body)
    {
        var responseBody = new ProblemResponse { Problem = _title, Errors = body.ToList() };

        _response.Body = new DefaultJsonSerializer().Serialize(responseBody);

        return this;
    }

    public ResponseBuilder ErrorType(string type)
    {
        var key = "x-amzn-ErrorType";

        _response.Headers[key] = type;

        return this;
    }

    public ResponseBuilder StatusCode(HttpStatusCode statusCode)
    {
        _response.StatusCode = (int)statusCode;

        return this;
    }

    public APIGatewayProxyResponse Build()
    {
        return _response;
    }
}