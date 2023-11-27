namespace AppFactory.Framework.Domain.Commands
{
    public interface ICommandDispatcher
    {
        CommandResult Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
    }
}