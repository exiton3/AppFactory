namespace AppFactory.Framework.Application.Commands;

public interface ICommandDispatcher
{
    Task<CommandResult> Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
}
