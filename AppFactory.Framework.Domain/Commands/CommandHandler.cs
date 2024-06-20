namespace AppFactory.Framework.Domain.Commands;

public abstract class CommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
{
    public abstract Task<CommandResult> Handle(TCommand command, CancellationToken cancellationToken = default);
   

    public Task<CommandResult> Handle(object command, CancellationToken cancellationToken = default)
    {
        return Handle((TCommand)command);
    }
}