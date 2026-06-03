namespace AppFactory.Framework.Messaging.LambdaHandlers;

/// <summary>
/// AWS Lambda-specific message processor interface (legacy).
/// For new code, use IMessageHandler from Messaging.Core.
/// </summary>
/// <typeparam name="T">The message type to process</typeparam>
public interface ILambdaMessageProcessor<T> where T : Message
{
    /// <summary>
    /// Process a message asynchronously
    /// </summary>
    /// <param name="message">The message to process</param>
    /// <returns>Task representing the async operation</returns>
    Task Process(T message);
}