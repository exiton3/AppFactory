namespace AppFactory.Framework.Domain.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        CommandResult Handle(TCommand command);
    }
}