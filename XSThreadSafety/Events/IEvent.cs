namespace XSLibrary.ThreadSafety.Events
{
    public abstract class IEvent<Sender, Args>
    {
        public delegate void EventHandle(Sender sender, Args arguments);

        /// <summary>
        /// Handle will be invoked if the event was triggered in the past.
        /// <para>Unsubscribing happens automatically after the invocation and is redundant if done from the event handle.</para>
        /// </summary>
        public abstract event EventHandle Event;
    }
}
