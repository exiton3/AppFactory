using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;

namespace AppFactory.Framework.Domain.Commands
{
    public interface ICommandDispatcher
    {
        Task<CommandResult> Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
    }

    class CommandDispatcher : ICommandDispatcher
    {
        private readonly IComponentContext _container;

        public CommandDispatcher(IComponentContext container)
        {
            _container = container;

            Debug.Write($"Created instance {GetHashCode()}");
        }

        public async Task<CommandResult> Dispatch<TCommand>(TCommand command) where TCommand : ICommand
        {
            var commandHandler = _container.Resolve<ICommandHandler<TCommand>>();
            Debug.Write($"Resolved handler {commandHandler.GetHashCode()}");
            return await commandHandler.Handle(command);
        }
    }
}