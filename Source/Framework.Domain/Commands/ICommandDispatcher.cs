using System.Diagnostics;
using Autofac;

namespace AppFactory.Framework.Domain.Commands
{
    public interface ICommandDispatcher
    {
        CommandResult Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
    }

    class CommandDispatcher : ICommandDispatcher
    {
        private readonly IComponentContext _container;

        public CommandDispatcher(IComponentContext container)
        {
            _container = container;

            Debug.Write($"Created instance {GetHashCode()}");
        }

        public CommandResult Dispatch<TCommand>(TCommand command) where TCommand : ICommand
        {
            var commandHandler = _container.Resolve<ICommandHandler<TCommand>>();
            Debug.Write($"Resolved handler {commandHandler.GetHashCode()}");
            return commandHandler.Handle(command);
        }
    }
}