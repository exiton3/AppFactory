namespace AppFactory.Framework.Application.Commands;

public abstract class CommandHandler<TCommand> : Domain.Commands.ICommandHandler<TCommand> where TCommand : Domain.Commands.ICommand
{
    public abstract Task<Domain.Commands.CommandResult> Handle(TCommand command, CancellationToken cancellationToken = default);


    public Task<Domain.Commands.CommandResult> Handle(object command, CancellationToken cancellationToken = default)
    {
        return Handle((TCommand)command);
    }
}