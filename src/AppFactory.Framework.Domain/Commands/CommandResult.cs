using AppFactory.Framework.Domain.ServiceResult;

namespace AppFactory.Framework.Domain.Commands;

public enum FailureReason
{
    None,
    Validation,
    DomainError,
    ExternalSystem
}
public class CommandResult
{
    private readonly List<Error> _errors = new();

    private CommandResult()
    {
        FailureReason = FailureReason.None;
    }
    private CommandResult(IEnumerable<Error> errors, FailureReason failureReason = FailureReason.Validation)
    {
        FailureReason = failureReason;
        _errors = new List<Error>();
        _errors.AddRange(errors);
    }

    private CommandResult(Error error, FailureReason failureReason = FailureReason.Validation) :this(new List<Error>{error}, failureReason)
    {
      
    }


    public bool IsSuccess => _errors.Count == 0;

    public bool IsFailure => !IsSuccess;

    public FailureReason FailureReason { get; private set; }
    public string Id { get; set; }

    public IEnumerable<Error> Errors => _errors;

    public static CommandResult Success()
    {
        return new CommandResult();
    }

    public static CommandResult Success(string id)
    {
        return new CommandResult { Id = id};
    }
    public static CommandResult ErrorResult(Error error,FailureReason failureReason = FailureReason.Validation)
    {
        var commandResult = new CommandResult(error, failureReason);
        
        return commandResult;
    }

    public static CommandResult ErrorResult(string code, string message)
    {

        var commandResult = new CommandResult(new Error(code,message));

        return commandResult;
    }

    public static CommandResult ErrorResult(IEnumerable<Error> errors, FailureReason failureReason = FailureReason.Validation)
    {

        var commandResult = new CommandResult(errors, failureReason);

        return commandResult;
    }
}