using System;

namespace Framework.Domain.Events
{
    public interface IEvent
    {
        DateTime TimeStamp { get; set; }
    }
}