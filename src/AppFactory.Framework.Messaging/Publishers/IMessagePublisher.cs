namespace AppFactory.Framework.Messaging.Publishers;

public interface IMessagePublisher
{
    Task<PublishResponse> Publish<TMessage>(TMessage message, CancellationToken token = default) where TMessage : MessageBase;

}

public class MessageBase
{
    public string Body { get; set; }
}