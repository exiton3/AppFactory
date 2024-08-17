namespace AppFactory.Framework.Messaging.LambdaHandlers
{
    public class Message
    {
        public Message()
        {
            Attributes = new Dictionary<string, string>();
        }
        public string MessageId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Source { get; set; }
        public string Body { get; set; }

        public Dictionary<string,string> Attributes { get; set; }
    }
}
