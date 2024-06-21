namespace AppFactory.Framework.Domain.Commands;

public interface ICommandHandler
{
    Task<CommandResult> Handle(object command, CancellationToken cancellationToken = default);
}
public interface ICommandHandler<in TCommand> : ICommandHandler where TCommand : ICommand
{
    Task<CommandResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}