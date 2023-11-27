namespace AppFactory.Framework.Domain.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        OperationResult Handle(TCommand command);
    }
}