namespace AppFactory.Framework.Messaging.LambdaHandlers;

public interface ILambdaMessageProcessor<T> where T : Message
{
    Task Process(T message);
}