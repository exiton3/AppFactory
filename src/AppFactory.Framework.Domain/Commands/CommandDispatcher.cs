namespace AppFactory.Framework.Domain.Commands;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly Dictionary<string, ICommandHandler> _commandHandlers = new();
    public CommandDispatcher(IEnumerable<ICommandHandler> commandHandlers)
    {
        foreach (var commandHandler in commandHandlers)
        {
            var commandType = commandHandler.GetType().BaseType.GetGenericArguments().Single();

            _commandHandlers.Add(commandType.Name, commandHandler);
        }
    }

    public async Task<CommandResult> Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        var typeName = command.GetType().Name;

        if (!_commandHandlers.ContainsKey(typeName))
        {
            throw new KeyNotFoundException($"The command handler for <{typeName}> not registered");
        }

        return await _commandHandlers[typeName].Handle(command, cancellationToken);
    }
}