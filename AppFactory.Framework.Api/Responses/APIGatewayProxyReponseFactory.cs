using System.Net;
using Amazon.Lambda.APIGatewayEvents;
using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.Responses;

class APIGatewayProxyReponseFactory
{
    public static APIGatewayProxyResponse OK(string data)
    {

        return ResponseBuilder.Response
            .Body(data)
            .StatusCode(HttpStatusCode.OK)
            .Build();
    }

    public static APIGatewayProxyResponse BadRequest(IList<Error> errors)
    {
        return ResponseBuilder.Response
            .ProblemTitle($"{errors.Count} validation error{(errors.Count > 1 ? "s" : string.Empty)} detected:")
            .Body(errors)
            .ErrorType("ValidationException")
            .StatusCode(HttpStatusCode.BadRequest)
            .Build();
    }

    public static APIGatewayProxyResponse BadRequest(IList<Error> errors,string title)
    {

        return ResponseBuilder.Response
            .ProblemTitle(title)
            .Body(errors)
            .ErrorType("ValidationException")
            .StatusCode(HttpStatusCode.BadRequest)
            .Build();
    }
    public static APIGatewayProxyResponse Unexpected(IList<Error> errors)
    {
        return ResponseBuilder.Response
            .ProblemTitle("Unexpected error")
            .Body(errors)
            .StatusCode(HttpStatusCode.InternalServerError)
            .ErrorType("InternalServerError")
            .Build();
    }

    public static APIGatewayProxyResponse NotFound(IList<Error> errors)
    {

        return ResponseBuilder.Response
              .Body(@$"{{""message"": ""{string.Join(",", errors.Select(x => x.Message))}""}}")
              .StatusCode(HttpStatusCode.NotFound)
              .ErrorType("NotFoundException")
              .Build();
    }

    public static APIGatewayProxyResponse ExternalError(ICollection<Error> errors)
    {
        return ResponseBuilder.Response
            .ProblemTitle("External system error")
            .Body(errors)
            .StatusCode(HttpStatusCode.ServiceUnavailable)
            .ErrorType("ExternalSystemError")
            .Build();
    }

    public static APIGatewayProxyResponse Accepted(string data)
    {
        return ResponseBuilder.Response
            .Body(data)
            .StatusCode(HttpStatusCode.Accepted)
            .Build();
    }
}