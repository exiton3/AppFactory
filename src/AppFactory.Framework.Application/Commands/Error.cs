namespace AppFactory.Framework.Application.Commands;

public class Error
{
    public Error()
    {
        
    }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; set; }

    public string Message { get; set; }
}
