namespace AppFactory.Framework.Messaging.LambdaHandlers
{
    public class Message
    {
        public string MessageId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Source { get; set; }
        public string Body { get; set; }
    }
}
