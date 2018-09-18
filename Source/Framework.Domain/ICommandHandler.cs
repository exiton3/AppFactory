using Drawback.Domain;

namespace Framework.Domain
{
    public interface ICommandHandler<in TCommand> where TCommand: ICommand
    {
        OperationResult Handle(TCommand command);
    }
}