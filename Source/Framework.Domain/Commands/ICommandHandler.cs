using System.Threading.Tasks;

namespace AppFactory.Framework.Domain.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Task<CommandResult> Handle(TCommand command);
    }
}