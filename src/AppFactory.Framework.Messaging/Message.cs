using System;

namespace AppFactory.Framework.Messaging
{
    public class Message
    {
        public string MessageId { get; set; }

        public DateTime Timestamp { get; set; }

        public string Source { get; set; }
        public string Body { get; set; }
    }
}
