using System;

namespace AppFactory.Framework.Domain.Events
{
    public class Event : IEvent
    {
        public DateTime TimeStamp { get; set; }

        public Event()
        {
            TimeStamp = DateTime.Now;
        }
    }
}