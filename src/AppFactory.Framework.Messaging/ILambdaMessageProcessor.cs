namespace AppFactory.Framework.Messaging;

public interface ILambdaMessageProcessor<T> where T : Message
{
    Task Process(T message);
}