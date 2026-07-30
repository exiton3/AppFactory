using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.Responses;

public class ProblemResponse
{
    public string Problem { get; set; }

    public List<Error> Errors { get; set; }
}