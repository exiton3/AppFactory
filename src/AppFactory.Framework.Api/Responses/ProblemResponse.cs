using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Api.Responses;

class ProblemResponse
{
    public string Problem { get; set; }

    public List<Error> Errors { get; set; }
}