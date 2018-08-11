﻿namespace XSLibrary.ThreadSafety.Events
{
    public class EventRelay<Relay, Sender, Args> : AutoInvokeEvent<Relay, Args>
    {
        public EventRelay(Relay sender, IEvent<Sender, Args> relayedEvent)
        {
            relayedEvent.Event += (baseSender, baseArgs) => Invoke(sender, baseArgs);
        }
    }
}
