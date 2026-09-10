namespace AppFactory.Framework.Domain.ServiceResult;

public enum ResultType
{
    Ok,
    Unexpected,
    NotFound,
    Unauthorized,
    Forbidden,
    Invalid,
    External,
    Accepted
}