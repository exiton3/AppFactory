namespace AppFactory.Framework.Domain.Commands
{
    public interface ICommandDispatcher
    {
        OperationResult Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
    }
}