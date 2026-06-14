namespace AppFactory.Framework.Application.Commands;

/// <summary>
/// Dispatches commands to their registered handlers.
/// Uses IServiceProvider for lazy handler resolution to support both
/// ASP.NET Core (application-scoped DI) and serverless (function-scoped DI).
/// </summary>
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<CommandResult> Dispatch<TCommand>(TCommand command, CancellationToken cancellationToken = default) 
        where TCommand : ICommand
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));
        var handler = _serviceProvider.GetService(handlerType) as ICommandHandler<TCommand>;

        if (handler == null)
        {
            throw new InvalidOperationException(
                $"No handler registered for command type '{typeof(TCommand).Name}'. " +
                $"Ensure a class implementing ICommandHandler<{typeof(TCommand).Name}> is registered.");
        }

        return await handler.Handle(command, cancellationToken);
    }
}
