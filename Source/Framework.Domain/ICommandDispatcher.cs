using Drawback.Domain;

namespace Framework.Domain
{
    public interface ICommandDispatcher
    {
        OperationResult Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
    }
}