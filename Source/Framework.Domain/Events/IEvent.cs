using System;

namespace AppFactory.Framework.Domain.Events
{
    public interface IEvent
    {
        DateTime TimeStamp { get; set; }
    }
}