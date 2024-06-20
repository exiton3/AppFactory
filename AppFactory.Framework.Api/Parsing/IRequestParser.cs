namespace AppFactory.Framework.Api.Parsing;

public interface IRequestParser
{
    TOutRequest ParseRequest<TOutRequest>(InputRequest inputRequest) where TOutRequest : class, new();
}