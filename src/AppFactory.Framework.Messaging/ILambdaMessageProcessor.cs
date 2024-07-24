namespace AppFactory.Framework.Messaging;

internal interface ILambdaMessageProcessor<T>
{
    Task Handle(T @event);
}