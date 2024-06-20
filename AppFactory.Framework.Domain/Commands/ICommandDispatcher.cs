namespace AppFactory.Framework.Domain.Commands;

public interface ICommandDispatcher
{
    Task<CommandResult> Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
}